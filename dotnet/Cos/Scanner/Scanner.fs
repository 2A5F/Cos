module rec Volight.Cos.Parser.Scanner

open System
open System.Collections.Generic
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

let internal spaceBody (ctx: Ctx) (code: Code) i =
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

let internal idBody (ctx: Ctx) (code: Code) i =
    match code.[i] with
    | Just c when isIdBody c -> idBody ctx code (i + 1)
    | _ ->
    let range = code.CodeRangeTo i
    let loc = ctx.Loc range
    let str = ctx.SubStr range
    let id = 
        match SubStrToEnum.TryGet str with
        | Just k -> Key(k, loc)
        | Nil -> ID(Token.New(str, loc))
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
    | '!' | '%' | '+' | '-' | '*' | '/' | '^' | '|' | '&' | '>' | '<' | '.' | '=' | '?' | ':' -> true 
    | _ -> false

let internal operBody (ctx: Ctx) (code: Code) i =
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

// todo num

////////////////////////////////////////////////////////////////////////////////////////////////////

let inline internal blockItem (ctx: Ctx) (code: Code) =
    match space ctx code with
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
                            // | Nil ->
                            //     match num ctx code with
                            //     | Nil ->
                            //         match char ctx code with
                            //         | Nil ->
                            //             match str ctx code with
                            //             | r -> r
                            //         | r -> r
                            //     | r -> r
                            | r -> r
                        | r -> r
                    | r -> r
                | r -> r
            | r -> r
        | r -> r
    | r -> r
    
////////////////////////////////////////////////////////////////////////////////////////////////////

let internal blockBodyLoop (ctx: Ctx) (code: Code) close =
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

let inline internal blockBody (ctx: Ctx) (code: Code) close typ =
    let tokens = ctx.tokens
    ctx.tokens <- List()
    let struct(rloc, r) = blockBodyLoop ctx code.Tail close
    let lloc = ctx.Loc (code.CodeRangeTo 1)
    let bl = TBlock.New(typ, lloc, rloc, tokens.ToArray())
    ctx.tokens <- tokens
    tokens.Add(Tokens.Block bl)
    Just r

let internal block (ctx: Ctx) (code: Code) =
    match code.First with
    | Just '{' -> blockBody ctx code '}' BracketsType.Curly
    | Just '(' -> blockBody ctx code ')' BracketsType.Round
    | Just '[' -> blockBody ctx code ']' BracketsType.Square
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

// todo str char

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal root (ctx: Ctx) (code: Code) = 
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
