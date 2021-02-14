namespace Volight.Cos.Parser

open System
open System.Linq
open System.Collections.Generic
open Volight.Cos.Utils
open Volight.Cos.Utils.SubStrEx

type KeyWord =
| Var = 0uy | Let = 1uy | If = 2uy | Else = 3uy | Do = 4uy | Case = 5uy | Of = 6uy | With = 7uy | For = 8uy | Break = 9uy 
| Continue = 10uy | Return = 11uy | Try = 12uy | Catch = 13uy | Finally = 14uy | Fn = 15uy | Def = 16uy | Data = 17uy | Kind = 18uy | Enum = 19uy
| TMe = 20uy | Me = 21uy | Where = 22uy | Module = 23uy | Import = 24uy | Export = 25uy | Is = 26uy | As = 27uy | Const = 28uy | Static = 29uy
| Co = 30uy | Inline = 31uy | Tail = 32uy | Public = 33uy | Private = 34uy | Internal = 35uy | Protected = 36uy

module KeyWords =
    [<AbstractClass; Sealed>]
    type private DoInit() =
        static let keyWord2Strs i = struct(i, match i with KeyWord.TMe -> "Me" | _ -> Enum.GetName(i).ToLower() ) 
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
        | KeyWord.Co | KeyWord.Inline | KeyWord.Tail 
            -> true
        | _ -> false
