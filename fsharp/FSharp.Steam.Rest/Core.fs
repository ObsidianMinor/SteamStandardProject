[<AutoOpen>]
/// Provides the core types for Steam rest requests
module FSharp.Steam.Rest.Core
    open System
    open System.IO
    open System.Linq
    open System.Net
    open System.Net.Http

    type HttpMethod = 
        | Get
        | Head
        | Post
        | Put
        | Delete
        | Options
        | Patch

    type RestHeaderValue = 
        | Value of string
        | Values of seq<string>

    [<Flags>]
    type RetryMode = 
        | AlwaysFail = 0
        | Timeouts = 1
        | BadGateway = 4
        | AlwaysRetry = 5

    type RestRequest = {
        Method : HttpMethod
        RequestUri : Uri
        Content : HttpContent option
        Headers : Map<string, RestHeaderValue>
    }

    type RestResponse = {
        StatusCode : HttpStatusCode
        Headers : Map<string, RestHeaderValue>
        Content : Stream
    }

    type RequestOptions = {
        Timeout : int option
        RetryMode : RetryMode option
    }

    exception HttpException of RestRequest * RestResponse

    type IRestClient =
        abstract member AsyncSend: RestRequest -> Async<RestResponse>
        abstract member SetCookie: Uri -> Cookie -> unit
        abstract member SetHeader: string -> RestHeaderValue option -> unit

    type DefaultRestClient () =
        let handler = new HttpClientHandler()

        do
            handler.UseProxy <- false
            handler.UseCookies <- true

        let client = new HttpClient(handler)

        interface IRestClient with 
            member __.AsyncSend request = async {
                let createRequest request = 
                    let message = new HttpRequestMessage ((HttpMethod ((request.Method.ToString()).ToUpper())), request.RequestUri)

                    for header in request.Headers do
                        message.Headers.Remove header.Key |> ignore

                        let headers = header.Value
                        match headers with
                            | Value value -> message.Headers.Add (header.Key, value)
                            | Values values -> message.Headers.Add (header.Key, values)

                    match request.Content with
                        | Some content -> message.Content <- content
                        | None -> ()

                    message

                let createResponse (response : HttpResponseMessage) = async { 
                    let! content = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
                    let headers = 
                        response.Headers.ToDictionary ((fun x -> x.Key), (fun x -> Values x.Value)) 
                        |> Seq.map (|KeyValue|) 
                        |> Map.ofSeq
                    return {
                        StatusCode = response.StatusCode
                        Headers = headers
                        Content = content
                    }
                }
                
                let! send = createRequest request |> client.SendAsync |> Async.AwaitTask
                return! createResponse send
            }

            member __.SetCookie uri cookie = handler.CookieContainer.Add (uri, cookie)
            member __.SetHeader key value = 
                client.DefaultRequestHeaders.Remove key |> ignore
                match value with
                    | Some values -> 
                        match values with
                            | Value v -> client.DefaultRequestHeaders.Add(key, v)
                            | Values v2 -> client.DefaultRequestHeaders.Add(key, v2)
                    | None -> ()

        interface IDisposable with
            member __.Dispose () =
                handler.Dispose ()
                client.Dispose ()
                ()

    let defaultRestClientProvider () = new DefaultRestClient () :> IRestClient

    type SteamRestConfig = {
        DefaultRequestTimeout : int
        DefaultRetryMode : RetryMode
        RestClient : unit -> IRestClient
    }

    /// Specifies the default values for a SteamRestConfig
    let defaultSteamRestConfig = {
        DefaultRequestTimeout = 15000;
        DefaultRetryMode = RetryMode.AlwaysFail;
        RestClient = defaultRestClientProvider
    }

    type SteamRestClient ( config : SteamRestConfig option ) =

        let someOrDefault maybe someFunc defaultVal =
            match maybe with
                | Some some -> someFunc some
                | None -> defaultVal

        let client = someOrDefault config (fun x -> x.RestClient) (defaultSteamRestConfig.RestClient) ()

        let defaultTimeout = someOrDefault config (fun x -> x.DefaultRequestTimeout) defaultSteamRestConfig.DefaultRequestTimeout

        let defaultRetry = someOrDefault config (fun x -> x.DefaultRetryMode) defaultSteamRestConfig.DefaultRetryMode

        let createOptionsWithDefaults options = 
            match options with
                | Some options ->
                    let { Timeout = optionTimeout; RetryMode = optionRetry } = options
                    let timeout = someOrDefault optionTimeout (fun x -> x) defaultTimeout
                    let retry = someOrDefault optionRetry (fun x -> x) defaultRetry

                    (timeout, retry)
                | None -> (defaultTimeout, defaultRetry)

        let rec sendRequest request timeout (retry : RetryMode) = async {
            let send = request |> client.AsyncSend
            let! sendWithTimeout = Async.StartChild (send, timeout)

            let tryBindWithCatch = async {
                try
                    let! response = sendWithTimeout
                    match response.StatusCode with
                    | status when status >= HttpStatusCode.OK || status < HttpStatusCode.Ambiguous -> return Some response
                    | HttpStatusCode.BadGateway when retry.HasFlag RetryMode.BadGateway -> return None
                    | _ -> return raise (HttpException(request, response))
                with
                    | :? TimeoutException when retry.HasFlag RetryMode.Timeouts -> return None
            }

            let! result = tryBindWithCatch
            match result with
                | Some response -> return response
                | None -> return! sendRequest request timeout retry
        }

        member __.SetHeader key value = client.SetHeader key value

        member __.SetCookie uri value = client.SetCookie uri value

        member __.AsyncSend request options = 
            let (timeout, retry) = createOptionsWithDefaults options

            sendRequest request timeout retry
