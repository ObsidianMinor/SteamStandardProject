namespace Steam.Net

open System.Runtime.InteropServices
open System.Reflection
open System.Threading.Tasks
open System

/// <summary> Resolves methods with Task, void, Async<unit>, and unit return types <summary/>
type FSharpDefaultReceiveMethodResolver() = 
    inherit DefaultReceiveMethodResolver()

    let resolveAsync ( method, target ) =

        (false, None)

    let resolveUnit ( method, target ) = 

        (false, None)    

    let resolveGCAsync ( method, target ) = 

        (false, None)

    let resolveGCUnit ( method, target ) =

        (false, None)    

    override this.TryResolve ( method : MethodInfo, target : obj, [<Out>] receiver : byref<MessageReceiver> ) = 
        match method.ReturnType with
            | t when t = typeof<Task> -> 
                let success, value = this.ResolveTask ( method, target = target )
                receiver <- value
                success
            | t when t = typeof<Void> -> 
                let success, value = this.ResolveVoid ( method, target = target )
                receiver <- value
                success
            | t when t = typeof<Async<unit>> -> 
                let success, value = resolveAsync ( method, target )
                match value with
                    | Some del -> receiver <- del
                    | None -> receiver <- null
                success                
            | t when t = typeof<unit> -> 
                let success, value = resolveUnit ( method, target )
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
            | t when t = typeof<Void> -> 
                let success, value = this.ResolveVoid ( method, gc = target )
                receiver <- value
                success
            | t when t = typeof<Async<unit>> -> 
                let success, value = resolveGCAsync ( method, target )
                match value with
                    | Some del -> receiver <- del
                    | None -> receiver <- null
                success                
            | t when t = typeof<unit> -> 
                let success, value = resolveGCUnit ( method, target )
                match value with
                    | Some del -> receiver <- del
                    | None -> receiver <- null
                success                                
            | _ -> false