namespace Steam.Net

open System.Runtime.InteropServices
open System.Reflection
open System.Threading.Tasks
open Steam.Net.Messages
open System
open Steam.Net.GameCoordinators.Messages

/// Resolves methods with Task, Async<unit>, and unit return types
type FSharpDefaultReceiveMethodResolver() = 
    inherit DefaultReceiveMethodResolver()
        
    static member private _AsyncInvokeBody<'T> ( message : NetworkMessage, del : Delegate ) =
        let func = del :?> Func<'T, Async<unit>>
        let body = message.Deserialize<'T>()
        func.Invoke body |> Async.StartAsTask :> Task

    static member private _InvokeBody<'T> ( message : NetworkMessage, del : Delegate ) =
        let func = del :?> Func<'T, unit>
        let body = message.Deserialize<'T>()
        async { func.Invoke body } |> Async.StartAsTask :> Task

    static member private _AsyncInvokeGCBody<'T> ( message : GameCoordinatorMessage, del : Delegate ) =
        let func = del :?> Func<'T, Async<unit>>
        let body = message.Deserialize<'T>()
        func.Invoke body |> Async.StartAsTask :> Task

    static member private _InvokeGCBody<'T> ( message : GameCoordinatorMessage, del : Delegate ) =
        let func = del :?> Func<'T, unit>
        let body = message.Deserialize<'T>()
        async { func.Invoke body } |> Async.StartAsTask :> Task

    static member private AsyncInvokeBodyInfo = typeof<FSharpDefaultReceiveMethodResolver>.GetMethod("_AsyncInvokeBody", BindingFlags.Static ||| BindingFlags.NonPublic);
    static member private InvokeBodyInfo = typeof<FSharpDefaultReceiveMethodResolver>.GetMethod("_InvokeBody", BindingFlags.Static ||| BindingFlags.NonPublic);
    static member private AsyncInvokeGCBodyInfo = typeof<FSharpDefaultReceiveMethodResolver>.GetMethod("_AsyncInvokeGCBody", BindingFlags.Static ||| BindingFlags.NonPublic);
    static member private InvokeGCBodyInfo = typeof<FSharpDefaultReceiveMethodResolver>.GetMethod("_InvokeGCBody", BindingFlags.Static ||| BindingFlags.NonPublic);

    member private __.ResolveAsync ( method : MethodInfo, target : obj ) =
        let paramaters = method.GetParameters()
        if paramaters.Length <> 1 
            then (false, None)
        else
            let param = paramaters.[0]

            match param.ParameterType with
                | t when t = typeof<NetworkMessage> ->
                    let d = method.CreateDelegate(typeof<Func<NetworkMessage, Async<unit>>>, target) :?> Func<NetworkMessage, Async<unit>>
                    let invoker = new MessageReceiver(fun msg -> d.Invoke(msg) |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<Header> ->
                    let d = method.CreateDelegate(typeof<Func<Header, Async<unit>>>, target) :?> Func<Header, Async<unit>>
                    let invoker = new MessageReceiver(fun msg -> d.Invoke(msg.Header) |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<ClientHeader> ->
                    let d = method.CreateDelegate(typeof<Func<ClientHeader, Async<unit>>>, target) :?> Func<ClientHeader, Async<unit>>
                    let invoker = new MessageReceiver(fun msg -> d.Invoke(msg.Header :?> ClientHeader) |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<ProtobufClientHeader> ->
                    let d = method.CreateDelegate(typeof<Func<ProtobufClientHeader, Async<unit>>>, target) :?> Func<ProtobufClientHeader, Async<unit>>
                    let invoker = new MessageReceiver(fun msg -> d.Invoke(msg.Header :?> ProtobufClientHeader) |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t -> 
                    let invoke = FSharpDefaultReceiveMethodResolver.AsyncInvokeBodyInfo.MakeGenericMethod([| param.ParameterType |]).CreateDelegate(typeof<Func<NetworkMessage, Delegate, Task>>) :?> Func<NetworkMessage, Delegate, Task>
                    let del = method.CreateDelegate(typedefof<Func<_,_>>.MakeGenericType([| param.ParameterType; typeof<Async<unit>> |]), target)
                    (true, Some (new MessageReceiver(fun message -> invoke.Invoke(message, del))))

    member private __.ResolveUnit ( method : MethodInfo, target : obj ) = 
        let paramaters = method.GetParameters()
        if paramaters.Length <> 1 
            then (false, None)
        else
            let param = paramaters.[0]

            match param.ParameterType with
                | t when t = typeof<NetworkMessage> ->
                    let d = method.CreateDelegate(typeof<Func<NetworkMessage, unit>>, target) :?> Func<NetworkMessage, unit>
                    let invoker = new MessageReceiver(fun msg -> async { d.Invoke msg } |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<Header> ->
                    let d = method.CreateDelegate(typeof<Func<Header, unit>>, target) :?> Func<Header, unit>
                    let invoker = new MessageReceiver(fun msg -> async { d.Invoke msg.Header } |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<ClientHeader> ->
                    let d = method.CreateDelegate(typeof<Func<ClientHeader, unit>>, target) :?> Func<ClientHeader, unit>
                    let invoker = new MessageReceiver(fun msg -> async { d.Invoke ( msg.Header :?> ClientHeader ) } |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<ProtobufClientHeader> ->
                    let d = method.CreateDelegate(typeof<Func<ProtobufClientHeader, unit>>, target) :?> Func<ProtobufClientHeader, unit>
                    let invoker = new MessageReceiver(fun msg -> async { d.Invoke ( msg.Header :?> ProtobufClientHeader ) } |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t -> 
                    let invoke = FSharpDefaultReceiveMethodResolver.InvokeBodyInfo.MakeGenericMethod([| param.ParameterType |]).CreateDelegate(typeof<Func<NetworkMessage, Delegate, Task>>) :?> Func<NetworkMessage, Delegate, Task>
                    let del = method.CreateDelegate(typedefof<Func<_,_>>.MakeGenericType([| param.ParameterType; typeof<unit> |]), target)
                    (true, Some (new MessageReceiver(fun message -> invoke.Invoke(message, del))))

    member private __.ResolveGCAsync ( method : MethodInfo, target : obj ) = 
        let paramaters = method.GetParameters()
        if paramaters.Length <> 1 
            then (false, None)
        else
            let param = paramaters.[0]

            match param.ParameterType with
                | t when t = typeof<GameCoordinatorMessage> ->
                    let d = method.CreateDelegate(typeof<Func<GameCoordinatorMessage, Async<unit>>>, target) :?> Func<GameCoordinatorMessage, Async<unit>>
                    let invoker = new GameCoordinatorReceiver(fun msg -> d.Invoke(msg) |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<Header> ->
                    let d = method.CreateDelegate(typeof<Func<Header, Async<unit>>>, target) :?> Func<Header, Async<unit>>
                    let invoker = new GameCoordinatorReceiver(fun msg -> d.Invoke(msg.Header) |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<ProtobufClientHeader> ->
                    let d = method.CreateDelegate(typeof<Func<GameCoordinatorProtobufHeader, Async<unit>>>, target) :?> Func<GameCoordinatorProtobufHeader, Async<unit>>
                    let invoker = new GameCoordinatorReceiver(fun msg -> d.Invoke(msg.Header :?> GameCoordinatorProtobufHeader) |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t -> 
                    let invoke = FSharpDefaultReceiveMethodResolver.AsyncInvokeGCBodyInfo.MakeGenericMethod([| param.ParameterType |]).CreateDelegate(typeof<Func<GameCoordinatorMessage, Delegate, Task>>) :?> Func<GameCoordinatorMessage, Delegate, Task>
                    let del = method.CreateDelegate(typedefof<Func<_,_>>.MakeGenericType([| param.ParameterType; typeof<Async<unit>> |]), target)
                    (true, Some (new GameCoordinatorReceiver(fun message -> invoke.Invoke(message, del))))

    member private __.ResolveGCUnit ( method : MethodInfo, target : obj ) =
        let paramaters = method.GetParameters()
        if paramaters.Length <> 1 
            then (false, None)
        else
            let param = paramaters.[0]

            match param.ParameterType with
                | t when t = typeof<GameCoordinatorMessage> ->
                    let d = method.CreateDelegate(typeof<Func<GameCoordinatorMessage, unit>>, target) :?> Func<GameCoordinatorMessage, unit>
                    let invoker = new GameCoordinatorReceiver(fun msg -> async { d.Invoke msg } |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<Header> ->
                    let d = method.CreateDelegate(typeof<Func<Header, unit>>, target) :?> Func<Header, unit>
                    let invoker = new GameCoordinatorReceiver(fun msg -> async { d.Invoke msg.Header } |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t when t = typeof<GameCoordinatorProtobufHeader> ->
                    let d = method.CreateDelegate(typeof<Func<GameCoordinatorProtobufHeader, unit>>, target) :?> Func<GameCoordinatorProtobufHeader, unit>
                    let invoker = new GameCoordinatorReceiver(fun msg -> async { d.Invoke ( msg.Header :?> GameCoordinatorProtobufHeader ) } |> Async.StartAsTask :> Task)
                    (true, Some invoker)
                | t -> 
                    let invoke = FSharpDefaultReceiveMethodResolver.InvokeGCBodyInfo.MakeGenericMethod([| param.ParameterType |]).CreateDelegate(typeof<Func<GameCoordinatorMessage, Delegate, Task>>) :?> Func<GameCoordinatorMessage, Delegate, Task>
                    let del = method.CreateDelegate(typedefof<Func<_,_>>.MakeGenericType([| param.ParameterType; typeof<unit> |]), target)
                    (true, Some (new GameCoordinatorReceiver(fun message -> invoke.Invoke(message, del))))

    override this.TryResolve ( method : MethodInfo, target : obj, [<Out>] receiver : byref<MessageReceiver> ) = 
        match method.ReturnType with
            | t when t = typeof<Task> -> 
                let success, value = this.ResolveTask ( method, target = target )
                receiver <- value
                success
            | t when t = typeof<Async<unit>> -> 
                let success, value = this.ResolveAsync ( method, target )
                match value with
                    | Some del -> receiver <- del
                    | None -> receiver <- null
                success                
            | t when t = typeof<unit> -> 
                let success, value = this.ResolveUnit ( method, target )
                match value with
                    | Some del -> receiver <- del
                    | None -> receiver <- null
                success
            | _ -> false

    override this.TryResolve ( method : MethodInfo, target : obj, [<Out>] receiver : byref<GameCoordinatorReceiver> ) =
        match method.ReturnType with
            | t when t = typeof<Task> -> 
                let success, value = this.ResolveTask ( method, gc = target )
                receiver <- value
                success
            | t when t = typeof<Async<unit>> -> 
                let success, value = this.ResolveGCAsync ( method, target )
                match value with
                    | Some del -> receiver <- del
                    | None -> receiver <- null
                success
            | t when t = typeof<unit> -> 
                let success, value = this.ResolveGCUnit ( method, target )
                match value with
                    | Some del -> receiver <- del
                    | None -> receiver <- null
                success
            | _ -> false