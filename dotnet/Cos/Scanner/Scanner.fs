module Volight.Cos.Parser.Scanner

open System
open System.Collections.Generic
open System.Globalization
open Volight.Cos.SrcPos
open Volight.Cos.Utils
open Volight.Cos.Utils.Utils
open Volight.Cos.Utils.SlicedEx
open Volight.Cos.Parser.KeyWords

type internal Code = char Sliced

type internal Ctx =
    val raw: string
    val pos: Pos[]
    val mutable tokens: Tokens List
    val errs: ScannerError List
    new (raw, pos) = { raw = raw; pos = pos; tokens = List(); errs = List() }

    member inline self.Loc (range: CodeRange) : Loc = { From = self.pos.[range.From]; To = self.pos.[range.To - 1] }
    member inline self.SubStr range = SubStr(self.raw, range)

////////////////////////////////////////////////////////////////////////////////////////////////////

let inline internal isSpace c = Char.IsWhiteSpace c

let rec internal spaceBody (ctx: Ctx) (code: Code) i =
    match code.[i] with
    | Just c when isSpace c -> spaceBody ctx code (i + 1)
    | _ ->
    Just <| code.CodeRangeFrom i

let internal space (ctx: Ctx) (code: Code) = 
    match code.First with 
    | Just c when isSpace c -> spaceBody ctx code 1
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let inline internal isIdFirst c = match c with '_' | '$' -> true | _ -> Char.IsLetter c
let inline internal isIdBody c = match c with '_' | '$' -> true | _ -> Char.IsLetterOrDigit c

let rec internal idBody (ctx: Ctx) (code: Code) i =
    match code.[i] with
    | Just c when isIdBody c -> idBody ctx code (i + 1)
    | _ ->
    let range = code.CodeRangeTo i
    let loc = ctx.Loc range
    let str = ctx.SubStr range
    let id = TId.New(str, loc)
    ctx.tokens.Add(Tokens.ID id)
    Just <| code.CodeRangeFrom i

let internal id (ctx: Ctx) (code: Code) =
    match code.First with 
    | Just c when isIdFirst c -> idBody ctx code 1
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal split (ctx: Ctx) (code: Code) =
    match code.First with 
    | Just ';' -> 
        let loc = ctx.Loc (code.CodeRangeTo 1)
        ctx.tokens.Add(Split { Loc = loc })
        Just (code.CodeRangeFrom 1)
    | _ -> Nil

let internal comma (ctx: Ctx) (code: Code) =
    match code.First with 
    | Just ',' -> 
        let loc = ctx.Loc (code.CodeRangeTo 1)
        ctx.tokens.Add(Comma { Loc = loc })
        Just (code.CodeRangeFrom 1)
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let inline internal isOper c = 
    match c with 
    | '!' | '%' | '+' | '-' | '*' | '/' | '^' | '|' | '&' | '>' | '<' | '.' | '=' | '?' | ':' | '~' -> true 
    | _ -> false

let rec internal operBody (ctx: Ctx) (code: Code) i =
    match code.[i] with
    | Just c when isOper c -> operBody ctx code (i + 1)
    | _ ->
    let range = code.CodeRangeTo i
    let loc = ctx.Loc range
    let str = code.[..i].ToStr
    let oper = TOper.New(str, loc)
    ctx.tokens.Add(Oper oper)
    Just <| code.CodeRangeFrom i

let internal oper (ctx: Ctx) (code: Code) =
    match code.First with
    | Just c when isOper c -> operBody ctx code 1 
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal at (ctx: Ctx) (code: Code) =
    match code.First with 
    | Just '@' -> 
        let loc = ctx.Loc (code.CodeRangeTo 1)
        ctx.tokens.Add(At { Loc = loc })
        Just (code.CodeRangeFrom 1)
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let inline internal isNumFirst c = (c >= '0' && c <= '9')
let inline internal isNumBody c = (c = '_') || (c >= '0' && c <= '9')
let inline internal isNumHexBody c = (c = '_') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || (c >= '0' && c <= '9')
let inline internal isNumBinaryBody c = match c with '_' | '0' | '1'  -> true | _ -> false
let inline internal isNumBinaryIllegal c = (c >= '2' && c <= '9')

let inline internal numFinish (ctx: Ctx) (code: Code) prefix suffxi f e i illegal =
    let range = code.CodeRange(f, e)
    let loc = ctx.Loc range
    let fuloc = Loc.OfMP prefix suffxi loc
    if illegal then ctx.errs.Add(IllegalNumber fuloc)
    else 
        let str = ctx.SubStr range
        let tk = Token.New(str, loc)
        let num = TNum(tk, prefix, suffxi, fuloc)
        ctx.tokens.Add(Num num)
    Just <| code.CodeRangeFrom i

let rec internal numSuffix (ctx: Ctx) (code: Code) prefix f e i illegal =
    match code.[i] with
    | Just c when isIdBody c -> numSuffix ctx code prefix f e (i + 1) illegal
    | _ ->
        let range = code.CodeRange(e, i)
        let loc = ctx.Loc range
        let str = ctx.SubStr range
        let tk = Token.New(str, loc)
        numFinish ctx code prefix (Just <| TId.ID tk) f e i illegal

let rec internal numBodyBinary (ctx: Ctx) (code: Code) prefix f i first illegal =
    match code.[i] with
    | Just c when isNumBinaryBody c -> numBodyBinary ctx code prefix f (i + 1) false illegal
    | Just c when isNumBinaryIllegal c -> numBodyBinary ctx code prefix f (i + 1) false true
    | Just c when isIdFirst c -> numSuffix ctx code (Just prefix) f i (i + 1) illegal
    | _ -> 
    if first then ctx.errs.Add(UnknownSymbol ctx.pos.[code.RawIndex i])
    numFinish ctx code (Just prefix) Nil f i i illegal

let rec internal numBodyHex (ctx: Ctx) (code: Code) prefix f i first illegal =
    match code.[i] with
    | Just c when isNumHexBody c -> numBodyHex ctx code prefix f (i + 1) false illegal
    | Just c when isIdFirst c -> numSuffix ctx code (Just prefix) f i (i + 1) illegal
    | _ -> 
    if first then ()
    numFinish ctx code (Just prefix) Nil f i i illegal

let rec internal numBody (ctx: Ctx) (code: Code) i =
    match code.[i] with
    | Just c when isNumBody c -> numBody ctx code (i + 1)
    | Just c when isIdFirst c -> numSuffix ctx code Nil 0 i (i + 1) false
    | _ -> numFinish ctx code Nil Nil 0 i i false

let internal numZeroFirst (ctx: Ctx) (code: Code) =
    match code.[1] with
    | Just ('x' | 'X') -> 
        let range = code.CodeRangeTo 2
        let loc = ctx.Loc range
        let str = ctx.SubStr range
        let tk = Token.New(str, loc)
        numBodyHex ctx code tk 2 2 true false
    | Just ('b' | 'B') -> 
        let range = code.CodeRangeTo 2
        let loc = ctx.Loc range
        let str = ctx.SubStr range
        let tk = Token.New(str, loc)
        numBodyBinary ctx code tk 2 2 true false
    | Just c when isNumBody c -> numBody ctx code 2
    | Just c when isIdFirst c -> numSuffix ctx code Nil 0 2 2 false
    | _ -> numFinish ctx code Nil Nil 0 1 1 false

let internal num (ctx: Ctx) (code: Code) =
    match code.First with
    | Just '0' -> numZeroFirst ctx code
    | Just c when isNumFirst c -> numBody ctx code 1
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let inline internal isHex c = (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || (c >= '0' && c <= '9')

let internal escapeDigit (ctx: Ctx) (code: Code) i =
    match code.[i + 1] with
    | Just c when Char.IsDigit(c) ->
        match code.[i + 2] with
        | Just c when Char.IsDigit(c) ->
            let span = code.[i..i + 3].span
            let mutable c = 0uy
            let s = Byte.TryParse(span, &c)
            if not s then
                let loc = ctx.Loc <| code.CodeRange(i, i + 3)
                ctx.errs.Add(IllegalEscape(loc, Digit))
                struct (i + 3, Nil)
            else
                struct (i + 3, Just <| char c)
        | _ ->
            let loc = ctx.Loc <| code.CodeRange(i, i + 2)
            ctx.errs.Add(IllegalEscape(loc, Digit))
            struct (i + 2, Nil)
    | _ ->
        let loc = ctx.Loc <| code.CodeRange(i, i + 1)
        ctx.errs.Add(IllegalEscape(loc, Digit))
        struct (i + 1, Nil)

let internal escapeHex (ctx: Ctx) (code: Code) i =
    match code.[i + 1] with
    | Just c when isHex c ->
        match code.[i + 2] with
        | Just c when isHex c ->
            let span = code.[i + 1 .. i + 3].span
            let mutable c = 0uy
            let s = Byte.TryParse(span, NumberStyles.HexNumber, null, &c)
            if not s then
                let loc = ctx.Loc <| code.CodeRange(i, i + 3)
                ctx.errs.Add(IllegalEscape(loc, Hex))
                struct (i + 3, Nil)
            else
                struct (i + 3, Just <| char c)
        | _ ->
            let loc = ctx.Loc <| code.CodeRange(i, i + 1)
            ctx.errs.Add(IllegalEscape(loc, Hex))
            struct (i + 2, Nil)
    | _ ->
        let loc = ctx.Loc <| code.CodeRange(i, i)
        ctx.errs.Add(IllegalEscape(loc, Hex))
        struct (i + 1, Nil)

let rec internal escapeUnicode (ctx: Ctx) (code: Code) f i =
    match code.[i] with
    | Just c when isHex c ->
        if i - f < 4 then escapeUnicode ctx code f (i + 1) else
        let span = code.[f + 1 .. i + 1].span
        let mutable c = 0us
        let s = UInt16.TryParse(span, NumberStyles.HexNumber, null, &c)
        if not s then
            let loc = ctx.Loc <| code.CodeRange(f, i + 1)
            ctx.errs.Add(IllegalEscape(loc, Unicode))
            struct (i + 1, Nil)
        else
            struct (i + 1, Just <| char c)
    | _ ->
        let loc = ctx.Loc <| code.CodeRange(f, i)
        ctx.errs.Add(IllegalEscape(loc, Unicode))
        struct (i + 1, Nil)

let rec internal escapeBigUnicode (ctx: Ctx) (code: Code) f i =
    match code.[i] with
    | Just c when isHex c ->
        if i - f < 6 then escapeBigUnicode ctx code f (i + 1) else
        let span = code.[f + 1 .. i + 1].span
        let mutable c = 0
        let s = Int32.TryParse(span, NumberStyles.HexNumber, null, &c)
        if not s then
            let loc = ctx.Loc <| code.CodeRange(f, i)
            ctx.errs.Add(IllegalEscape(loc, Unicode))
            struct (i + 1, Nil)
        else
            struct (i + 1, Just <| Char.ConvertFromUtf32 c)
    | _ ->
        let loc = ctx.Loc <| code.CodeRange(f, i)
        ctx.errs.Add(IllegalEscape(loc, Unicode))
        struct (i + 1, Nil)

let internal charEscape (ctx: Ctx) (code: Code) i =
    match code.[i] with
    | Just '0' -> struct (i + 1, Just '\000')
    | Just 'a' -> struct (i + 1, Just '\a')
    | Just 'b' -> struct (i + 1, Just '\b')
    | Just 'f' -> struct (i + 1, Just '\f')
    | Just 'n' -> struct (i + 1, Just '\n')
    | Just 'r' -> struct (i + 1, Just '\r')
    | Just 't' -> struct (i + 1, Just '\t')
    | Just 'v' -> struct (i + 1, Just '\v')
    | Just '\\' -> struct (i + 1, Just '\\')
    | Just ''' -> struct (i + 1, Just ''')
    | Just '"' -> struct (i + 1, Just '"')
    | Just c when Char.IsDigit(c) -> escapeDigit ctx code i
    | Just 'x' -> escapeHex ctx code i
    | Just 'u' -> escapeUnicode ctx code i (i + 1)
    | _ -> 
        let pos = ctx.pos.[code.RawIndex i]
        ctx.errs.Add(UnknownEscape(pos))
        struct (i + 1, Nil)

let internal strEscape (ctx: Ctx) (code: Code) i =
    match code.[i] with
    | Just '0' -> struct (i + 1, Just "\000")
    | Just 'a' -> struct (i + 1, Just "\a")
    | Just 'b' -> struct (i + 1, Just "\b")
    | Just 'f' -> struct (i + 1, Just "\f")
    | Just 'n' -> struct (i + 1, Just "\n")
    | Just 'r' -> struct (i + 1, Just "\r")
    | Just 't' -> struct (i + 1, Just "\t")
    | Just 'v' -> struct (i + 1, Just "\v")
    | Just '\\' -> struct (i + 1, Just "\\")
    | Just ''' -> struct (i + 1, Just "'")
    | Just '"' -> struct (i + 1, Just "\"")
    | Just '$' -> struct (i + 1, Just "$")
    | Just c when Char.IsDigit(c) -> 
        match escapeDigit ctx code i with
        | (i, Just c) -> (i, Just <| c.ToString())
        | (i, Nil) -> (i, Nil)
    | Just 'x' -> 
        match escapeHex ctx code i with
        | (i, Just c) -> (i, Just <| c.ToString())
        | (i, Nil) -> (i, Nil)
    | Just 'u' -> 
        match escapeUnicode ctx code i (i + 1) with
        | (i, Just c) -> (i, Just <| c.ToString())
        | (i, Nil) -> (i, Nil)
    | Just 'U' -> escapeBigUnicode ctx code i (i + 1)
    | _ -> 
        let pos = ctx.pos.[code.RawIndex i]
        ctx.errs.Add(UnknownEscape(pos))
        struct (i + 1, Nil)

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal charEnd (ctx: Ctx) (code: Code) q i c raw =
    match code.[i] with 
    | Just e when e = q -> 
        let range = code.CodeRangeTo (i + 1)
        let loc = ctx.Loc range
        let c = TChar.New(c, raw, loc)
        ctx.tokens.Add(Tokens.Char c)
        Just <| code.CodeRangeFrom (i + 1)
    | _ -> 
        let pos = ctx.pos.[code.RawIndex i]
        ctx.errs.Add(CharNotClosed(pos))
        Just <| code.CodeRangeFrom (i + 1) // or Nil

let internal char (ctx: Ctx) (code: Code) =
    match code.First with
    | Just 'c' ->
        match code.[1] with
        | Just (q & (''' | '"')) -> 
            match code.[2] with
            | Just '\\' -> 
                match charEscape ctx code 3 with
                | (i, Just c) -> 
                    let raw = ctx.SubStr <| code.CodeRange(2, i)
                    charEnd ctx code q i c (Just raw) 
                | (i, Nil) -> Just <| code.CodeRangeFrom i // or Nil
            | Just c -> charEnd ctx code q 3 c Nil 
            | Nil -> 
                let pos = ctx.pos.[code.offset]
                raise <| ScannerException(UnexpectedEof(pos))
        | _ -> Nil
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let rec inline internal blockItem (ctx: Ctx) (code: Code) =
    match space ctx code with
    | Nil ->
        match char ctx code with 
        | Nil ->
            match id ctx code with 
            | Nil -> 
                match split ctx code with 
                | Nil -> 
                    match comma ctx code with
                    | Nil -> 
                        match block ctx code with
                        | Nil ->
                            match oper ctx code with
                            | Nil ->
                                match at ctx code with
                                | Nil ->
                                    match num ctx code with
                                    | Nil ->
                                        match str ctx code with
                                        | r -> r
                                    | r -> r
                                | r -> r
                            | r -> r
                        | r -> r
                    | r -> r
                | r -> r
            | r -> r
        | r -> r
    | r -> r
    
////////////////////////////////////////////////////////////////////////////////////////////////////

and internal blockBodyLoop (ctx: Ctx) (code: Code) close =
    if code.IsEmpty then 
        let pos = ctx.pos.[code.offset]
        ctx.errs.Add(BlockNotClosed(pos, close))
        raise <| ScannerException(UnexpectedEof pos)
    else
    match code.First with
    | Just c when c = close -> 
        let rloc = ctx.Loc (code.CodeRangeTo 1)
        let r = code.CodeRangeFrom 1
        struct(rloc, r)
    | _ ->
    let r = blockItem ctx code
    match r with 
    | Just r -> blockBodyLoop ctx (code.ByCodeRange r) close
    | _ ->
    raise <| ScannerException(UnknownSymbol <| ctx.pos.[code.offset])

and internal blockBody (ctx: Ctx) (code: Code) close typ =
    let tokens = ctx.tokens
    ctx.tokens <- List()
    let struct(rloc, r) = blockBodyLoop ctx code.Tail close
    let lloc = ctx.Loc (code.CodeRangeTo 1)
    let bl = TBlock.New(typ, lloc, rloc, ctx.tokens.ToArray())
    ctx.tokens <- tokens
    tokens.Add(Tokens.Block bl)
    Just r

and internal block (ctx: Ctx) (code: Code) =
    match code.First with
    | Just '{' -> blockBody ctx code '}' BracketsType.Curly
    | Just '(' -> blockBody ctx code ')' BracketsType.Round
    | Just '[' -> blockBody ctx code ']' BracketsType.Square
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

and internal strPartEscape (ctx: Ctx) (code: Code) (items: TStrPart List) =
    match strEscape ctx code 1 with
    | (i, Just str) -> 
        let range = code.CodeRangeTo i
        let loc = ctx.Loc range
        let raw = ctx.SubStr range
        let esc = TStrEscape.New(str, raw, loc)
        items.Add(Escape esc)
        code.CodeRangeFrom i
    | (i, Nil) -> code.CodeRangeFrom i

and internal strPartBlock (ctx: Ctx) (code: Code) (items: TStrPart List) =
    let dollar = ctx.Loc (code.CodeRangeTo 1)
    let tokens = ctx.tokens
    ctx.tokens <- List()
    let struct(rloc, r) = blockBodyLoop ctx code.[2..] '}' 
    let lloc = ctx.Loc (code.CodeRangeTo 2)
    let block = TBlock.New(BracketsType.Curly, lloc, rloc, ctx.tokens.ToArray())
    ctx.tokens <- tokens
    let tbl = { Dollar = dollar; Block = block }
    items.Add(Block tbl)
    r

and internal strPartStr (ctx: Ctx) (code: Code) q (items: TStrPart List) i =
    match code.[i] with
    | Nil | Just '\\' | Just '$' -> 
        let range = code.CodeRangeTo i
        let str = ctx.SubStr range
        let part = Str str
        items.Add(part)
        code.CodeRangeFrom i
    | Just c when c = q ->
        let range = code.CodeRangeTo i
        let str = ctx.SubStr range
        let part = Str str
        items.Add(part)
        code.CodeRangeFrom i
    | _ -> strPartStr ctx code q items (i + 1)

and internal strBody (ctx: Ctx) (code: Code) q lloc (items: TStrPart List) =
    match code.First with
    | Nil -> 
        let pos = ctx.pos.[code.offset]
        ctx.errs.Add(StringNotClosed(pos))
        raise <| ScannerException(UnexpectedEof pos)
    | Just c when c = q -> 
        let rloc = ctx.Loc (code.CodeRangeTo 1)
        let tstr = TStr.New(lloc, rloc, items.ToArray())
        ctx.tokens.Add(Tokens.Str tstr)
        Just <| code.CodeRangeFrom 1
    | Just c ->
    let r = 
        if c = '$' && (match code.[1] with Just '{' -> true | _ -> false) then strPartBlock ctx code items
        elif c = '\\' then strPartEscape ctx code items
        else strPartStr ctx code q items 1
    strBody ctx (code.ByCodeRange r) q lloc items

and internal str (ctx: Ctx) (code: Code) =
    match code.First with
    | Just (q & (''' | '"')) -> 
        let lloc = (ctx.Loc (code.CodeRangeTo 1))
        let items = List<TStrPart>()
        strBody ctx code.Tail q lloc items
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let rec internal root (ctx: Ctx) (code: Code) = 
    if code.IsEmpty then () else 
    let r = blockItem ctx code
    match r with 
    | Just r -> root ctx (code.ByCodeRange r)
    | _ ->
    raise <| ScannerException(UnknownSymbol <| ctx.pos.[code.offset])

////////////////////////////////////////////////////////////////////////////////////////////////////

let scan code =
    let pos = Reader.read code |> Seq.toArray
    let ctx = Ctx(code, pos)
    let span = code.AsSliced()
    try root ctx span with 
    | ScannerException(k) -> ctx.errs.Add(k)
    if ctx.errs.Count = 1 then raise <| ScannerException(ctx.errs.[0])
    if ctx.errs.Count > 1 then raise <| ScannerException(MultipleErrors(ctx.errs.ToArray()))
    ctx.tokens
