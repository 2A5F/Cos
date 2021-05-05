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
        static let keyWord2Strs i = 
            struct(i, 
                match i with 
                | TSelf -> "Self" 
                | Underscore -> "_" 
                | TryE -> "try!"
                | TryQ -> "try?"
                | _ -> match FSharpValue.GetUnionFields(i, typeof<KeyWord>) with | case, _ -> case.Name.ToLower() 
            ) 
        static let _strs: struct (KeyWord * string) [] = Enum.GetValues(typeof<KeyWord>).Cast<KeyWord>().Select(keyWord2Strs).ToArray()
        static let _enumToStr: Dictionary<KeyWord, string> = Dictionary()
        static let _strToEnum: Dictionary<string, KeyWord> = Dictionary()
        static let _subStrToEnum: Dictionary<SubStr, KeyWord> = Dictionary()
        static let mutable len = 0
        static do
            for (e, s) in _strs do
                if s.Length > len then len <- s.Length
                _enumToStr.Add(e, s)
                _strToEnum.Add(s, e)
                _subStrToEnum.Add(s.ToSubStr, e)
        static member inline Strs = _strs
        static member inline EnumToStr = _enumToStr
        static member inline StrToEnum = _strToEnum
        static member inline SubStrToEnum = _subStrToEnum
        static member inline MaxLen = len
    let Strs = DoInit.Strs
    let EnumToStr = DoInit.EnumToStr
    let StrToEnum = DoInit.StrToEnum
    let SubStrToEnum = DoInit.SubStrToEnum
    let MaxLen = DoInit.MaxLen

    let idAllowed k =
        match k with
        | Co | Inline | NoInline | RefCtx | Tail | Throws
        | Underscore | Self | TSelf | Where
        | True | False
            -> true
        | _ -> false
