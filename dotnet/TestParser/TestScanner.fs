module TestScanner

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
let TestId1 () =
    let code = "asd"
    let r = scan code
    let a = r.[0]
    let e = Tokens.ID <| TId.New(SubStr(code, CodeRange.OfV 0 3), Loc.OfV 0u 0u 0u 2u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestSpace () =
    let code = "   "
    let r = scan code
    Assert.Zero(r.Count)
    ()

[<Test>]
let TestId2 () =
    let code = "else"
    let r = scan code
    let a = r.[0]
    let e = Tokens.ID <| TId.New(KeyWord.Else, Loc.OfV 0u 0u 0u 3u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestIdSpace () =
    let code = "asd else"
    let r = scan code
    let a1 = r.[0]
    let a2 = r.[1]
    let e1 = Tokens.ID <| TId.New(SubStr(code, CodeRange.OfV 0 3), Loc.OfV 0u 0u 0u 2u)
    let e2 = Tokens.ID <| TId.New(KeyWord.Else, Loc.OfV 0u 4u 0u 7u)
    Assert.AreEqual(e1, a1)
    Assert.AreEqual(e1.Loc, a1.Loc)
    Assert.AreEqual(e2, a2)
    Assert.AreEqual(e2.Loc, a2.Loc)
    ()

[<Test>]
let TestNum1 () =
    let code = "123"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 0 3), Loc.OfV 0u 0u 0u 2u), Nil, Nil, Loc.OfV 0u 0u 0u 2u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestNum2 () =
    let code = "123.456"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 0 3), Loc.OfV 0u 0u 0u 2u), Nil, Nil, Loc.OfV 0u 0u 0u 2u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestNum3 () =
    let code = "0x2a5F"
    let r = scan code
    let a = r.[0]
    let prefix = Token.New(SubStr(code, CodeRange.OfV 0 2), Loc.OfV 0u 0u 0u 1u)
    let e = Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 2 6), Loc.OfV 0u 2u 0u 5u), Just prefix, Nil, Loc.OfV 0u 0u 0u 5u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestNum4 () =
    let code = "000"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 0 3), Loc.OfV 0u 0u 0u 2u), Nil, Nil, Loc.OfV 0u 0u 0u 2u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestNum5 () =
    let code = "123u8"
    let r = scan code
    let a = r.[0]
    let suffix = TId.New(SubStr(code, CodeRange.OfV 3 5), Loc.OfV 0u 3u 0u 4u)
    let e = Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 0 3), Loc.OfV 0u 0u 0u 2u), Nil, Just suffix, Loc.OfV 0u 0u 0u 4u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestNum6 () =
    let code = "000u8"
    let r = scan code
    let a = r.[0]
    let suffix = TId.New(SubStr(code, CodeRange.OfV 3 5), Loc.OfV 0u 3u 0u 4u)
    let e = Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 0 3), Loc.OfV 0u 0u 0u 2u), Nil, Just suffix, Loc.OfV 0u 0u 0u 4u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestNum7 () =
    let code = "0x2a5fu8"
    let r = scan code
    let a = r.[0]
    let prefix = Token.New(SubStr(code, CodeRange.OfV 0 2), Loc.OfV 0u 0u 0u 1u)
    let suffix = TId.New(SubStr(code, CodeRange.OfV 6 8), Loc.OfV 0u 6u 0u 7u)
    let e = Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 2 6), Loc.OfV 0u 2u 0u 5u), Just prefix, Just suffix, Loc.OfV 0u 0u 0u 7u)
    Assert.AreEqual(e, a)
    Assert.AreEqual(e.Loc, a.Loc)
    ()

[<Test>]
let TestNum8 () =
    let code = "0b123"
    let e = Assert.Throws<ScannerException>(fun _ -> ignore <| scan code)
    let a = e.Data0 
    let e = ScannerError.IllegalNumber(Loc.OfV 0u 0u 0u 4u)
    Assert.AreEqual(e, a)
    ()

[<Test>]
let TestBlock1 () =
    let code = "{}"
    let r = scan code
    let a = r.[0]
    let a = a.GetBlock
    let e = TBlock.New(BracketsType.Curly, Loc.OfV 0u 0u 0u 0u, Loc.OfV 0u 1u 0u 1u, [||])
    Assert.AreEqual(e.Type, a.Type)
    Assert.AreEqual(e.Left, a.Left)
    Assert.AreEqual(e.Right, a.Right)
    Assert.AreEqual(e.Items, a.Items)
    ()

[<Test>]
let TestBlock2 () =
    let code = "{ asd }"
    let r = scan code
    let a = r.[0]
    let a = a.GetBlock
    let e = TBlock.New(BracketsType.Curly, Loc.OfV 0u 0u 0u 0u, Loc.OfV 0u 6u 0u 6u, [| Tokens.ID <| TId.New(SubStr(code, CodeRange.OfV 2 5), Loc.OfV 0u 2u 0u 4u) |])
    Assert.AreEqual(e.Type, a.Type)
    Assert.AreEqual(e.Left, a.Left)
    Assert.AreEqual(e.Right, a.Right)
    Assert.AreEqual(e.Items, a.Items)
    ()

[<Test>]
let TestBlock3 () =
    let code = "( asd )"
    let r = scan code
    let a = r.[0]
    let a = a.GetBlock
    let e = TBlock.New(BracketsType.Round, Loc.OfV 0u 0u 0u 0u, Loc.OfV 0u 6u 0u 6u,[| Tokens.ID <| TId.New(SubStr(code, CodeRange.OfV 2 5), Loc.OfV 0u 2u 0u 4u) |])
    Assert.AreEqual(e.Type, a.Type)
    Assert.AreEqual(e.Left, a.Left)
    Assert.AreEqual(e.Right, a.Right)
    Assert.AreEqual(e.Items, a.Items)
    ()

[<Test>]
let TestChar1 () =
    let code = "c'a'"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Char <| TChar.New('a', Nil, Loc.OfV 0u 0u 0u 3u)
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e, a)
    ()

[<Test>]
let TestChar2 () =
    let code = "c'\\n'"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Char <| TChar.New('\n', Just <| SubStr(code, CodeRange.OfV 2 4), Loc.OfV 0u 0u 0u 4u)
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e, a)
    ()

[<Test>]
let TestChar3 () =
    let code = "c'\\235'"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Char <| TChar.New('\235', Just <| SubStr(code, CodeRange.OfV 2 6), Loc.OfV 0u 0u 0u 6u)
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e, a)
    ()

[<Test>]
let TestChar4 () =
    let code = "c'\\x35'"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Char <| TChar.New('\x35', Just <| SubStr(code, CodeRange.OfV 2 6), Loc.OfV 0u 0u 0u 6u)
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e, a)
    ()

[<Test>]
let TestChar5 () =
    let code = "c'\\u2a5F'"
    let r = scan code
    let a = r.[0]
    let e = Tokens.Char <| TChar.New('\u2a5F', Just <| SubStr(code, CodeRange.OfV 2 8), Loc.OfV 0u 0u 0u 8u)
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e, a)
    ()

[<Test>]
let TestStr1 () =
    let code = "\"asd\""
    let r = scan code
    let a = r.[0].GetStr
    let e = TStr.New(Loc.OfV 0u 0u 0u 0u, Loc.OfV 0u 0u 0u 4u, [|
            TStrPart.Str <| SubStr(code, CodeRange.OfV 1 4)
        |])
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e.Items, a.Items)
    ()

[<Test>]
let TestStr2 () =
    let code = "\"asd${1}123\""
    let r = scan code
    let a = r.[0].GetStr
    let b = TStrPart.Block <| { Dollar = Loc.OfV 0u 4u 0u 4u; Block = TBlock.New(BracketsType.Curly, Loc.OfV 0u 4u 0u 5u, Loc.OfV 0u 4u 0u 7u, [|
            Tokens.Num <| TNum(Token.New(SubStr(code, CodeRange.OfV 6 7), Loc.OfV 0u 6u 0u 6u), Nil, Nil, Loc.OfV 0u 6u 0u 6u)
        |]) }
    let e = TStr.New(Loc.OfV 0u 0u 0u 0u, Loc.OfV 0u 0u 0u 11u, [|
            TStrPart.Str <| SubStr(code, CodeRange.OfV 1 4)
            b
            TStrPart.Str <| SubStr(code, CodeRange.OfV 8 11)
        |])
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e.Items.[0], a.Items.[0])
    Assert.AreEqual(e.Items.[2], a.Items.[2])
    let be = e.Items.[1].GetBlock
    let ba = a.Items.[1].GetBlock
    Assert.AreEqual(be.Dollar, ba.Dollar)
    Assert.AreEqual(be.Block.Loc, ba.Block.Loc)
    Assert.AreEqual(be.Block.Items, ba.Block.Items)
    ()

[<Test>]
let TestStr3 () =
    let code = "\"\\\"\""
    let r = scan code
    let a = r.[0].GetStr
    let e = TStr.New(Loc.OfV 0u 0u 0u 0u, Loc.OfV 0u 0u 0u 3u, [|
            TStrPart.Escape <| TStrEscape.New("\"", SubStr(code, CodeRange.OfV 1 3), Loc.OfV 0u 1u 0u 2u)
        |])
    Assert.AreEqual(e.Loc, a.Loc)
    Assert.AreEqual(e.Items, a.Items)
    ()