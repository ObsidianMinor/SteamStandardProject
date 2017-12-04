namespace Steam

open Steam.Logging
open System

[<AutoOpen>]
module Logs =
    
    type Logger with 
        member this.debug m ex = this.DebugAsync(m, ex) |> Async.AwaitTask
        member this.debug (m : string) = this.DebugAsync(m) |> Async.AwaitTask
        member this.debug (ex : Exception) = this.DebugAsync(ex) |> Async.AwaitTask
        member this.verbose m ex = this.VerboseAsync(m, ex) |> Async.AwaitTask
        member this.verbose (m : string) = this.VerboseAsync(m) |> Async.AwaitTask
        member this.verbose (ex : Exception) = this.VerboseAsync(ex) |> Async.AwaitTask
        member this.info m ex = this.InfoAsync(m, ex) |> Async.AwaitTask
        member this.info (m : string) = this.InfoAsync(m) |> Async.AwaitTask
        member this.info (ex : Exception) = this.InfoAsync(ex) |> Async.AwaitTask
        member this.warning m ex = this.WarningAsync(m, ex) |> Async.AwaitTask
        member this.warning (m : string) = this.WarningAsync(m) |> Async.AwaitTask
        member this.warning (ex : Exception) = this.WarningAsync(ex) |> Async.AwaitTask
        member this.error m ex = this.ErrorAsync(m, ex) |> Async.AwaitTask
        member this.error (m : string) = this.ErrorAsync(m) |> Async.AwaitTask
        member this.error (ex : Exception) = this.ErrorAsync(ex) |> Async.AwaitTask
        member this.critical m ex = this.LogAsync(LogSeverity.Critical, m, ex) |> Async.AwaitTask
        member this.critical (m : string) = this.LogAsync(LogSeverity.Critical, m) |> Async.AwaitTask
        member this.critical (m : Exception) = this.LogAsync(LogSeverity.Critical, m) |> Async.AwaitTask
