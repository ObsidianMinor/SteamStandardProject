module FSharp.Steam.Web
    open FSharp.Steam.Rest
    open Newtonsoft.Json
    open System
    open System.IO
    open System.Net
    open System.Text

    type Parameter = 
        /// One value
        | One of string
        /// Many values printed as an array parameter
        | Many of seq<string>

    let param x = Some (One (x.ToString ()))
    /// Serializes many values as many strings
    let many x = Some (Many (seq { for y in x do yield y.ToString () }))

    /// Optionally serializes a value as a string
    let optional x = 
        match x with
            | Some y -> param y
            | None -> None

    /// Optionally serializes many values as a string
    let optionalMany x = 
        match x with
            | Some y -> many y
            | None -> None

    /// A simple Steam Web API request that when sent through a SteamWebClient returns an Async<RestResponse>
    type SimpleWebRequest = {
        Interface : string
        Method : string
        Version : int
        RequireKey : bool
        HttpMethod : HttpMethod
        Parameters : seq<string * Parameter option>
    }

    type ResponseType = 
        | Json
        | Vdf
        | Xml

    /// An interpreted Steam Web API request that when sent through a SteamWebClient returns a converted response as Async<'t>
    type InterpretedWebRequest<'t> = {
        Simple : SimpleWebRequest
        Type : ResponseType
        Converter : RestResponse -> 't
    }

    let defaultConverter r = {
        Simple = r;
        Type = Json
        Converter = (fun x -> x)
    }

    let jsonConverter<'t> r = 
        let converter response =
            use reader = new StreamReader(response.Content)
            use jsonReader = new JsonTextReader(reader)
            let serializer = JsonSerializer.CreateDefault()
            serializer.Deserialize<'t> jsonReader

        {
            Simple = r;
            Type = Json;
            Converter = converter
        }

    type ConfiguredInterpretedWebRequest<'t> = {
        Interpreted : InterpretedWebRequest<'t>
        Timeout : int option
        RetryMode : RetryMode option
    }

    let options timeout retryMode request = {
        Interpreted = request
        Timeout = timeout
        RetryMode = retryMode
    }

    /// https://api.steampowered.com/
    [<Literal>]
    let defaultApiUrl = "https://api.steampowered.com"

    /// https://partner.steam-api.com
    [<Literal>]
    let partnerApiUrl = "https://partner.steam-api.com"

    type SteamWebConfig = {
        ApiUri : Uri
        ApiKey : string option
    }

    module SteamWebConfig = 
        let Default = {
            ApiUri = Uri defaultApiUrl;
            ApiKey = None
        }

    /// Handles authentication and sending API requests to the Steam Web API using the specified SteamRestClient
    type SteamWebClient (client : SteamRestClient, config) = 
        let make uri key request = {
            RequestUri = 
                let path = sprintf "/%s/%s/v%i" request.Interpreted.Simple.Interface request.Interpreted.Simple.Method request.Interpreted.Simple.Version
                let paramString = 
                    let addKey = 
                        match request.Interpreted.Simple.RequireKey with
                            | true ->
                                match key with
                                    | Some key -> Seq.append [ sprintf "key=%s" (WebUtility.UrlEncode key) ]
                                    | None -> invalidOp "Cannot send request that requires key when no key was provided"
                            | false -> Seq.append Seq.empty
                    let addFormat = 
                        let formatString = 
                            match request.Interpreted.Type with
                                | Json -> "format=json"
                                | Vdf -> "format=vdf"
                                | Xml -> "format=xml"
                        Seq.append [ formatString ]
                    request.Interpreted.Simple.Parameters
                    |> Seq.choose (fun (key, value) ->
                        match value with
                            | Some value -> 
                                match value with
                                    | One value -> Some (sprintf "%s=%s" (WebUtility.UrlEncode key) (WebUtility.UrlEncode value))
                                    | Many values when not (Seq.isEmpty values) -> 
                                        values
                                        |> Seq.indexed
                                        |> Seq.choose (fun (index, value) -> if String.IsNullOrWhiteSpace value then None else Some (sprintf "%s[%i]=%s" key index value))
                                        |> Seq.reduce (fun x y -> x + ("&" + y))
                                        |> (+) (sprintf "count=%i&" (Seq.length values))
                                        |> Some
                                    | _ -> invalidOp "Cannot make array parameter with zero elements"
                            | None -> None)
                    |> addKey
                    |> addFormat
                    |> Seq.reduce (fun x y -> x + ("&" + y))
                    |> (+) "?"
                Uri (uri, sprintf "%s%s" path paramString);
            Method = request.Interpreted.Simple.HttpMethod;
            Content = None;
            Headers = Map.empty;
            Timeout = request.Timeout
            RetryMode = request.RetryMode
        }

        /// Sends a configured interpreted request to the web API
        member __.AsyncSend request = async {
            let! response = client.AsyncSend (make config.ApiUri config.ApiKey request)
            return request.Interpreted.Converter (response)
        }

    type WebResponse<'t> = {
        [<JsonProperty("response")>]
        Response : 't
        [<JsonProperty("result")>]
        Result : Result
        [<JsonProperty("message")>]
        Message : string
    }
