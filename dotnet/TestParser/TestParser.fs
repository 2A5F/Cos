module TestParser

open System
open System.Runtime.InteropServices
open System.Collections.Generic
open NUnit.Framework
open Volight.Cos.Parser.Scanner
open Volight.Cos.Parser.Parser
open Volight.Cos.Parser
open Volight.Cos.Utils
open Volight.Cos.SrcPos

[<SetUp>]
let Setup () =
    ()

[<Test>]
let Test1 () =
    let code = "=> 1"
    let tks = scan code
    let r = parser (tks.ToArray())
    ()
