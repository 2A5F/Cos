// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.Runtime.InteropServices
open System.Collections.Generic
open Volight.Cos.Parser.Scanner
open Volight.Cos.Parser.Parser
open Volight.Cos.Parser
open Volight.Cos.Utils
open Volight.Cos.SrcPos

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom

[<EntryPoint>]
let main argv =
    let code = "try! 1"
    let tks = scan code
    let r = parser (tks.ToArray())
    Console.WriteLine(r);

    //let message = from "F#" // Call the function
    //printfn "Hello world %s" message
    0 // return an integer exit code