namespace rec Volight.Cos.Parser

open System.Runtime.CompilerServices
open Volight.Cos.SrcPos
open Volight.Cos.Utils
open Volight.Cos.Utils.Utils
open System.Diagnostics
open System.Text
open KeyWords

type Tokens =
    | ID of TId
    | Block of TBlock
    | Split of TSplit
    | Comma of TComma
    | Oper of TOper
    | At of TAt
    | Num of TNum
    | Char of TChar
    | Str of TStr

    member self.Loc =
        match self with
        | ID v -> v.Loc
        | Block v -> v.Loc
        | Split v -> v.Loc
        | Comma v -> v.Loc
        | Oper v -> v.Loc
        | At v -> v.Loc
        | Num v -> v.Loc
        | Char v -> v.Loc
        | Str v -> v.Loc

    member self.GetID =
        match self with
        | ID t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetBlock =
        match self with
        | Block t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetTSplit =
        match self with
        | Split t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetComma =
        match self with
        | Comma t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetOper =
        match self with
        | Oper t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetAt =
        match self with
        | At t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetNum =
        match self with
        | Num t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetChar =
        match self with
        | Char t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetStr =
        match self with
        | Str t -> t
        | _ -> raise <| System.Exception("type error")

    override self.ToString() =
        match self with
        | ID v -> v.ToString()
        | Block v -> v.ToString()
        | Split v -> v.ToString()
        | Comma v -> v.ToString()
        | Oper v -> v.ToString()
        | At v -> v.ToString()
        | Num v -> v.ToString()
        | Char v -> v.ToString()
        | Str v -> v.ToString()

[<Struct; IsReadOnly; DebuggerDisplay(@"Token \{ {ToString(),nq} \}")>]
type Token =
    { Str: SubStr
      Loc: Loc }

    static member New(raw, range, loc) = { Str = SubStr(raw, range); Loc = loc }
    static member New(str, loc) = { Str = str; Loc = loc }

    override self.ToString() = self.Str.ToString()

[<Struct; IsReadOnly; DebuggerDisplay(@"TID \{ {ToString(),nq} \}")>]
type TId =
    | ID of Token
    | Key of KeyWord * Loc

    static member New(str: SubStr, loc) =
        match SubStrToEnum.TryGet str with
        | Just k -> Key(k, loc)
        | Nil -> ID(Token.New(str, loc))

    override self.ToString() =
        match self with
        | ID t -> t.ToString()
        | Key (k, _) -> KeyWords.EnumToStr.[k]

    member inline self.Str = self.ToString()

    member self.Loc =
        match self with
        | ID v -> v.Loc
        | Key (_, v) -> v

    member self.IsIdAllowed =
        match self with
        | ID _ -> true
        | Key (k, _) -> KeyWords.idAllowed k

    member self.IsKeyOf key =
        match self with
        | ID _ -> false
        | Key (k, _) -> k = key

/// <summary> <code>;</code> </summary>
[<Struct; IsReadOnly>]
type TSplit =
    { Loc: Loc }

    override _.ToString() = ";"

/// <summary> <code>,</code> </summary>
[<Struct; IsReadOnly>]
type TComma =
    { Loc: Loc }

    override _.ToString() = ","

type BracketsType =
    /// <summary> <code>( )</code> </summary>
    | Round = 0uy
    /// <summary> <code>[ ]</code> </summary>
    | Square = 1uy
    /// <summary> <code>{ }</code> </summary>
    | Curly = 2uy

[<Struct; IsReadOnly; DebuggerDisplay(@"TBlock \{ {ToString(),nq} \}")>]
type TBlock =
    { Type: BracketsType
      Left: Loc
      Right: Loc
      Items: Tokens [] }

    static member New(typ, left, right, items) =
        { Type = typ
          Left = left
          Right = right
          Items = items }

    member self.Loc = self.Left.Merge self.Right

    override self.ToString() =
        let sb = StringBuilder()
        sb.Append("{ ") |> ignore

        for i in self.Items do
            sb.Append(i.ToString()) |> ignore
            sb.Append(' ') |> ignore

        sb.Append('}') |> ignore
        sb.ToString()

/// % ! + - * / ^ | & > < . = ? : ~
[<Struct; IsReadOnly; DebuggerDisplay(@"TOper \{ {ToString(),nq} \}")>]
type TOper =
    { Str: string
      Loc: Loc }

    static member New(str, loc) = { Str = str; Loc = loc }

    override self.ToString() = self.Str

/// <summary> <code>@</code> </summary>
[<Struct; IsReadOnly>]
type TAt =
    { Loc: Loc }

    override _.ToString() = "@"

[<Struct; IsReadOnly; DebuggerDisplay(@"TNum \{ {ToString(),nq} \}")>]
type TNum =
    val Num: Token
    val Prefix: Token Maybe
    val Suffix: TId Maybe
    val Loc: Loc

    new(num, prefix, suffix, loc) =
        { Num = num
          Prefix = prefix
          Suffix = suffix
          Loc = loc }

    override self.ToString() =
        match self.Suffix with
        | Nil ->
            match self.Prefix with
            | Nil -> self.Num.ToString()
            | Just p -> $"{p}{self.Num}"
        | Just s ->
            match self.Prefix with
            | Nil -> $"{self.Num}{s}"
            | Just p -> $"{p}{self.Num}{s}"

[<Struct; IsReadOnly; DebuggerDisplay(@"TChar \{ '{ToString(),nq}' \}")>]
type TChar =
    { Char: char
      Raw: SubStr Maybe
      Loc: Loc }

    static member New(char, raw, loc) = { Char = char; Raw = raw; Loc = loc }

    override self.ToString() =
        match self.Raw with
        | Just s -> s.ToString()
        | _ -> self.Char.ToString()


[<Struct; IsReadOnly; DebuggerDisplay(@"TStr \{ {ToString(),nq} \}")>]
type TStr =
    { Left: Loc
      Right: Loc
      Items: TStrPart [] }

    static member New(left, right, items) =
        { Left = left
          Right = right
          Items = items }

    member self.Loc = self.Left.Merge self.Right

    override self.ToString() =
        let sb = StringBuilder()
        sb.Append('"') |> ignore

        for i in self.Items do
            match i with
            | Str s -> sb.Append s |> ignore
            | Escape e -> sb.Append e.Raw |> ignore
            | Block b -> sb.Append(b.ToString()) |> ignore

        sb.Append('"') |> ignore
        sb.ToString()

type TStrPart =
    | Str of SubStr
    | Escape of TStrEscape
    | Block of TStrBlock

    override self.ToString() =
        match self with
        | Str s -> s.ToString()
        | Escape e -> e.ToString()
        | Block b -> b.ToString()

    member self.GetStr =
        match self with
        | Str t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetEscape =
        match self with
        | Escape t -> t
        | _ -> raise <| System.Exception("type error")

    member self.GetBlock =
        match self with
        | Block t -> t
        | _ -> raise <| System.Exception("type error")

[<Struct; IsReadOnly; DebuggerDisplay(@"TStrEscape \{ {ToString(),nq} \}")>]
type TStrEscape =
    { Str: string
      Raw: SubStr
      Loc: Loc }

    static member New(str, raw, loc) = { Str = str; Raw = raw; Loc = loc }

    override self.ToString() = self.Raw.ToString()

[<Struct; IsReadOnly; DebuggerDisplay(@"TStrBlock \{ {ToString(),nq} \}")>]
type TStrBlock =
    { Dollar: Loc
      Block: TBlock }

    override self.ToString() = "$" + self.Block.ToString()
    member self.Loc = self.Dollar.Merge self.Block.Right
