namespace Volight.Cos.Parser

open System
open System.Linq
open System.Collections.Generic
open Volight.Cos.Utils
open Volight.Cos.Utils.SubStrEx

type KeyWord =
| Var = 0uy | Let = 1uy | If = 2uy | Else = 3uy | Do = 4uy | Case = 5uy | Of = 6uy | With = 7uy | For = 8uy | While = 9uy | Break = 10uy 
| Continue = 11uy | Return = 12uy | Try = 13uy | Throw = 14uy | Catch = 15uy | Finally = 16uy | Fn = 17uy | Def = 18uy | Data = 19uy | Kind = 20uy | Enum = 21uy
| TMe = 22uy | Me = 23uy | Where = 24uy | Module = 25uy | Import = 26uy | Export = 27uy | Is = 28uy | As = 29uy | Const = 30uy | Static = 31uy
| Co = 32uy | Inline = 33uy | Throws = 34uy | Tail = 35uy | Public = 36uy | Private = 37uy | Internal = 38uy | Protected = 39uy | Goto = 40uy | Need = 41uy
| In = 42uy | Underscore = 43uy | False = 44uy | True = 45uy | Mut = 46uy

module KeyWords =
    [<AbstractClass; Sealed>]
    type private DoInit() =
        static let keyWord2Strs i = struct(i, match i with KeyWord.TMe -> "Me" | KeyWord.Underscore -> "_" | _ -> Enum.GetName(i).ToLower() ) 
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
        | KeyWord.Co | KeyWord.Inline | KeyWord.Tail | KeyWord.Throws
        | KeyWord.Data | KeyWord.Kind | KeyWord.Enum | KeyWord.Need
        | KeyWord.Underscore | KeyWord.Me | KeyWord.TMe
            -> true
        | _ -> false
