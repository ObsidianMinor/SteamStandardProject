[<AutoOpen>]
module FSharp.Steam.Web.Core
    open FSharp.Steam.Rest
    open System
    open System.Text
    open System.Net
    open System.IO
    open Newtonsoft.Json

    /// A simple Steam Web API request that when sent through a SteamWebClient returns an Async<RestResponse>
    type SimpleWebRequest = {
        Interface : string
        Method : string
        Version : int
        RequireKey : bool
        HttpMethod : HttpMethod
        Parameters : seq<string * obj>
    }

    type ResponseType = 
        | Json
        | Vdf
        | Xml

    /// An interpreted Steam Web API request that when sent through a SteamWebClient returns a converted response as Async<'t>
    type InterpretedWebRequest<'t> = {
        Request : SimpleWebRequest
        Type : ResponseType
        Converter : RestResponse -> 't
    }

    let defaultConverter r = {
        Request = r;
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
            Request = r;
            Type = Json;
            Converter = converter
        }

    type SteamWebConfig = {
        ApiUri : Uri
        /// Specifies the function to transform objects into parameter values
        Converter : string * obj -> list<string * string> option
        ApiKey : string option
    }

    /// https://api.steampowered.com/
    [<Literal>]
    let defaultApiUrl = "https://api.steampowered.com"

    /// https://partner.steam-api.com
    [<Literal>]
    let partnerApiUrl = "https://partner.steam-api.com"

    let defaultParameterConverter (key : string, value : obj) = 
        match value with
            | null -> None
            | _ -> Some [ key, value.ToString() ]

    let defaultSteamWebConfig = {
        ApiUri = Uri defaultApiUrl;
        Converter = defaultParameterConverter
        ApiKey = None
    }

    type SteamWebClient (restClient : SteamRestClient option, config : SteamWebConfig option) =
        let client =
            match restClient with
                | Some c -> c
                | None -> SteamRestClient None

        let key = 
            match config with
                | Some someConfig -> someConfig.ApiKey
                | None -> defaultSteamWebConfig.ApiKey

        let apiUri =
            match config with
                | Some someConfig -> someConfig.ApiUri
                | None -> defaultSteamWebConfig.ApiUri

        let convert = 
            match config with
                | Some someConfig -> someConfig.Converter
                | None -> defaultSteamWebConfig.Converter

        let createPath interfaceName method version = sprintf "/%s/%s/v%i" interfaceName method version

        let createParams useKey parameters responseType = 
            let keyString = 
                match useKey with
                    | true ->
                        match key with
                            | Some key -> sprintf "&key=%s" key
                            | None -> invalidOp "Cannot send request that requires key when no key was provided"
                    | false -> ""
            let format = 
                match responseType with
                    | Json -> "json"
                    | Vdf -> "vdf"
                    | Xml -> "xml"
            let builder = StringBuilder ""
            for parameter in parameters do
                let data = convert parameter
                match data with 
                    | Some results -> 
                        for (key, value) in results do
                            builder.Append (sprintf "&%s=%s" (WebUtility.UrlEncode (key)) (WebUtility.UrlEncode (value))) |> ignore
                    | None -> ()
            let built = builder.ToString ()
            sprintf "?format=%s%s%s" format built keyString

        let createUri interfaceName method version key parameters responseType =
            let path = createPath interfaceName method version
            let paramString = createParams key parameters responseType
            Uri (apiUri, sprintf "%s%s" path paramString)

        let createRequest request responseType = {
            RequestUri = createUri request.Interface request.Method request.Version request.RequireKey request.Parameters responseType;
            Method = request.HttpMethod;
            Content = None;
            Headers = Map.empty
        }

        member this.AsyncSend simple = simple |> defaultConverter |> this.AsyncSend<RestResponse>

        member this.AsyncSend<'t> (interpreted : InterpretedWebRequest<'t>) : Async<'t> = this.AsyncSend (interpreted, None)

        member __.AsyncSend<'t> (interpreted : InterpretedWebRequest<'t>, options : RequestOptions option) : Async<'t> = async {
            let request = createRequest interpreted.Request interpreted.Type
            let! response = client.AsyncSend request options
            return interpreted.Converter (response)
        }

