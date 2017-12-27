(* README
 * This project is an untracked directory,
 * it serves as a sandbox to test features
 * you've added to the Steam Standard projects.
 * Changes made here skip the working tree and
 * will not be commited or pushed to the upstream repository
 *)

open FSharp.Steam

[<EntryPoint>]
let main argv =
    let id = steamid64 76561198092222042UL
    printfn "%s" (id.ToString())
    0 // return an integer exit code
