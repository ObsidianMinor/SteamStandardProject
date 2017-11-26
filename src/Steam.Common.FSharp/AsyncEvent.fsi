namespace Steam

open System

module AsyncEvent = 

    [<Interface>]
    type IAsyncEvent<'Delegate, 'Args when  'Delegate : delegate<'Args, Async<unit>> and 'Delegate :> Delegate> =
        inherit IDelegateEvent<'Delegate>
        inherit IObservable<'Args>

    type AsyncHandler<'Args> = delegate of sender:obj * args:'Args -> Async<unit>

    type IAsyncEvent<'Args> = IAsyncEvent<AsyncHandler<'Args>, 'Args>

    type AsyncEvent<'Delegate, 'Args when 'Delegate : delegate<'Args, Async<unit>> and 'Delegate :> Delegate> =
        new : unit -> AsyncEvent<'Delegate, 'Args>
        member Trigger : sender:obj * args:'Args -> Async<unit>
        member Publish : unit -> IAsyncEvent<'Delegate, 'Args>

    type AsyncEvent<'T when 'T :> EventArgs> =
        new : unit -> AsyncEvent<'T>
        member Trigger : arg:'T -> Async<unit>
        member Publish : unit -> IAsyncEvent<'T>

