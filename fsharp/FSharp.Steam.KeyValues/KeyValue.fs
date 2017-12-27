namespace FSharp.Steam.KeyValues

open System.Drawing
open FParsec

[<AutoOpen>]
module rec Core = 

    /// Represents a basic KeyValue structure with a string key and value
    type KeyValue = {
        Key : string;
        Value : ValueType
    }
    
    type ValueType =
        | None of KeyValue list
        | String of string
        | Int32 of int32
        | Float of float
        | Pointer of nativeint
        | WideString of string
        | Color of Color
        | UInt64 of uint64
        | Int64 of int64