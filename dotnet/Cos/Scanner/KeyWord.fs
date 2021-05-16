namespace Volight.Cos.Parser

open System
open System.Linq
open System.Collections.Generic
open Volight.Cos.Utils
open Volight.Cos.Utils.SubStrEx
open Microsoft.FSharp.Reflection

[<Struct>]
type KeyWord =
| Underscore
| Let | Mut | Alt
| If | Else | Case | Of | Is | As
| Do | For | While | With | In
| Break | Continue | Return | Goto
| Try | Throw | Catch | Throws | Delay
| Fun | Co | Inline | NoInline | RefCtx | Tail
| Class | Trait | Enum | Type | Def
| TSelf | Self | Where | Static | New
| Public | Private | Internal | Protected
| Module | Import | Export
| True | False
| TryE | TryQ

module KeyWords =
    [<AbstractClass; Sealed>]
    type private DoInit() =
        static let makeKeyWords(i) = struct (FSharpValue.MakeUnion(i, [| |]) :?> KeyWord, i.Name)
        static let keyWord2Strs struct (i: KeyWord, n) = 
            struct(i, 
                match n with 
                | "TSelf" -> "Self" 
                | "Underscore" -> "_" 
                | "TryE" -> "try!"
                | "TryQ" -> "try?"
                | _ -> n
            )
        static let _strs: struct (KeyWord * string) [] = 
            FSharpType.GetUnionCases(typeof<KeyWord>)
                .Select(makeKeyWords)
                .Select(keyWord2Strs)
                .ToArray()
        static let _unionToStr: Dictionary<KeyWord, string> = Dictionary()
        static let _strToUnion: Dictionary<string, KeyWord> = Dictionary()
        static let _subStrToUnion: Dictionary<SubStr, KeyWord> = Dictionary()
        static let mutable len = 0
        static do
            for (e, s) in _strs do
                if s.Length > len then len <- s.Length
                _unionToStr.Add(e, s)
                _strToUnion.Add(s, e)
                _subStrToUnion.Add(s.ToSubStr, e)
        static member inline Strs = _strs
        static member inline UnionToStr = _unionToStr
        static member inline StrToUnion = _strToUnion
        static member inline SubStrToUnion = _subStrToUnion
        static member inline MaxLen = len
    let Strs = DoInit.Strs
    let UnionToStr = DoInit.UnionToStr
    let StrToUnion = DoInit.StrToUnion
    let SubStrToUnion = DoInit.SubStrToUnion
    let MaxLen = DoInit.MaxLen

    let idAllowed k =
        match k with
        | Co | Inline | NoInline | RefCtx | Tail | Throws
        | Underscore | Self | TSelf | Where
        | True | False
            -> true
        | _ -> false
