module Protobuf2 = 
    open FParsec

    type NamedValue<'a> = { Name: string; Value: 'a }

    type Label = Required
               | Optional
               | Repeated

    type ImportOption = Weak
                      | Public

    type ImportStatement = { Option: ImportOption; Value: string }

    type ProtoType = Double      of double
                   | Float       of float
                   | Int32       of int32
                   | Int64       of int64
                   | UInt32      of uint32
                   | UInt64      of uint64
                   | SInt32      of int32
                   | SInt64      of int64
                   | Fixed32     of int32
                   | Fixed64     of int64
                   | SFixed32    of int32
                   | SFixed64    of int64
                   | Bool        of bool
                   | String      of string
                   | Bytes       of byte[]
                   | MessageType 
                   | EnumType    
    
    type FieldOption = NamedValue<string>

    type Field = { Label: Label; Type: ProtoType; Number: int }

    type Specifier = Import of ImportStatement
                   | Package of string
                   | Option of NamedValue<string>
                   | Syntax of string

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code
