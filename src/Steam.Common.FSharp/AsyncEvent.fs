namespace Steam

open System
open System.Threading.Tasks 
open System.Reflection
open Microsoft.FSharp.Reflection

[<AutoOpen>]
module AsyncEvent =

    type AsyncEventHandler with
        member this.AsyncInvoke (sender : obj, args : 'T) = AsyncEventExtensions.InvokeAsync (this, sender, args) |> Async.AwaitTask 

    type AsyncEventHandler<'T when 'T :> EventArgs> with
        member this.AsyncInvoke (sender : obj, args : 'T) = AsyncEventExtensions.InvokeAsync (this, sender, args) |> Async.AwaitTask

    type AsyncEventWrapper<'Delegate, 'Args> = delegate of 'Delegate * obj * 'Args -> Async<unit>

    [<Interface>]
    type IAsyncEvent<'Delegate,'Args when 'Delegate : delegate<'Args, Async<unit>> and 'Delegate :> Delegate> =
        inherit IDelegateEvent<'Delegate>
        inherit IObservable<'Args>

    type AsyncHandler<'Args> = delegate of sender:obj * args:'Args -> Async<unit>

    type IAsyncEvent<'Args> = IAsyncEvent<AsyncHandler<'Args>, 'Args>

    type EventDelegee<'Args>(observer: IObserver<'Args>) =
        static let makeTuple =
            if FSharpType.IsTuple(typeof<'Args>) then
                FSharpValue.PreComputeTupleConstructor(typeof<'Args>)
            else
                fun _ -> assert false; null // should not be called, one-argument case don't use makeTuple function

        member x.Invoke(_sender:obj, args: 'Args) =
            observer.OnNext args
        member x.Invoke(_sender:obj, a, b) =
            let args = makeTuple([|a; b|]) :?> 'Args
            observer.OnNext args
        member x.Invoke(_sender:obj, a, b, c) =
            let args = makeTuple([|a; b; c|]) :?> 'Args
            observer.OnNext args
        member x.Invoke(_sender:obj, a, b, c, d) =
            let args = makeTuple([|a; b; c; d|]) :?> 'Args
            observer.OnNext args
        member x.Invoke(_sender:obj, a, b, c, d, e) = 
            let args = makeTuple([|a; b; c; d; e|]) :?> 'Args
            observer.OnNext args
        member x.Invoke(_sender:obj, a, b, c, d, e, f) =
            let args = makeTuple([|a; b; c; d; e; f|]) :?> 'Args
            observer.OnNext args

    [<CompiledName("FSharpAsyncEvent`2")>]
    type AsyncEvent<'Delegate, 'Args when 'Delegate : delegate<'Args, Async<unit>> and 'Delegate :> Delegate>() =
        let mutable multicast : 'Delegate = Unchecked.defaultof<_>

        static let mi, argTypes = 
            let instanceBindingFlags = BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly
            let mi = typeof<'Delegate>.GetMethod("Invoke",instanceBindingFlags)
            let actualTypes = mi.GetParameters() |> Array.map (fun p -> p.ParameterType)
            mi, actualTypes.[1..]

        static let invoker = 
            if argTypes.Length = 1 then
                (Delegate.CreateDelegate(typeof<AsyncEventWrapper<'Delegate, 'Args>>, mi) :?> AsyncEventWrapper<'Delegate, 'Args>)
            else
                null

        static let invokeInfo = 
            let instanceBindingFlags = BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly
            let mi =
                typeof<EventDelegee<'Args>>.GetMethods(instanceBindingFlags)
                |> Seq.filter(fun mi -> mi.Name = "Invoke" && mi.GetParameters().Length = argTypes.Length + 1)
                |> Seq.exactlyOne
            if mi.IsGenericMethodDefinition then
                mi.MakeGenericMethod argTypes
            else
                mi

        member this.Trigger (sender:obj, args:'Args) =
            match box multicast with
            | null -> async { () }
            | _ ->
                match invoker with
                | null ->
                    let args = Array.append [| sender|] (FSharpValue.GetTupleFields(box args))
                    async { multicast.DynamicInvoke(args) |> ignore }
                | _ ->
                    async { invoker.Invoke(multicast, sender, args) |> ignore }

        member this.Publish =
            { new obj() with
                  member this.ToString() = "<published event>"
              interface IAsyncEvent<'Delegate, 'Args>
              interface IDelegateEvent<'Delegate> with 
                member e.AddHandler(d) = 
                    multicast <- Delegate.Combine(multicast, d) :?> 'Delegate
                member e.RemoveHandler(d) = 
                    multicast <- Delegate.Remove(multicast, d)  :?> 'Delegate 
              interface IObservable<'Args> with 
                member e.Subscribe(observer) = 
                   let obj = new EventDelegee<'Args>(observer)
                   let h = Delegate.CreateDelegate(typeof<'Delegate>, obj, invokeInfo) :?> 'Delegate
                   (e :?> IDelegateEvent<'Delegate>).AddHandler(h)
                   { new IDisposable with 
                        member x.Dispose() = (e :?> IDelegateEvent<'Delegate>).RemoveHandler(h) } }

    [<CompiledName("FSharpAsyncEvent`1")>]
    type AsyncEvent<'T> =
        val mutable multicast : AsyncHandler<'T>
        new() = { multicast = null }

        member this.Trigger (arg: 'T) = async {
            match this.multicast with
            | null -> ()
            | d -> for handler in d.GetInvocationList() do
                     let! result = (handler :?> AsyncHandler<'T>).Invoke (null, arg)
                     result |> ignore
        }

        member this.Publish = 
            { new obj() with 
                    member this.ToString() = "<published event>"
              interface IAsyncEvent<'T> 
              interface IDelegateEvent<AsyncHandler<'T>> with
                    member e.AddHandler(d) = 
                        this.multicast <- (Delegate.Combine(this.multicast, d) :?> AsyncHandler<'T>)
                    member e.RemoveHandler(d) =
                        this.multicast <- (Delegate.Combine(this.multicast, d) :?> AsyncHandler<'T>)
              interface System.IObservable<'T> with
                    member e.Subscribe(observer) = 
                        let h = new AsyncHandler<_>(fun sender args -> async { observer.OnNext(args) })
                        (e :?> IDelegateEvent<_>).AddHandler(h)
                        { new System.IDisposable with
                            member x.Dispose() = (e :?> IDelegateEvent<_>).RemoveHandler(h) } }