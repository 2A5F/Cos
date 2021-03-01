module Volight.Cos.Parser.Parser

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open Volight.Cos.SrcPos
open Volight.Cos.Utils
open Volight.Cos.Utils.Utils
open Volight.Cos.Utils.SlicedEx
open Volight.Cos.Parser.KeyWords


type internal Tks = Tokens Sliced

type internal CtxRef =
    val errs: ParserError List
    val endloc: Loc
    new(endloc) = { errs = List(); endloc =endloc }
    member self.ToCtx = Ctx(self, self.endloc)

and internal Ctx =
    val ctx: CtxRef
    val endloc: Loc
    new(ctx, endloc) = { ctx = ctx; endloc = endloc }

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal endLocOf (tks: Tks) = match tks.Last with Just t -> t.Loc | Nil -> Loc.zero

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal pBool (tks: Tks) =
    match tks.First with
    | Just (Tokens.ID (v & (Key (KeyWord.True, _)))) -> Just struct (True v |> Bool |> Just, tks.CodeRangeTail)
    | Just (Tokens.ID (v & (Key (KeyWord.False, _)))) -> Just struct (False v |> Bool |> Just, tks.CodeRangeTail)
    | _ -> Nil

let internal pNum (tks: Tks) =
    match tks.First with
    | Just (Tokens.Num n) -> Just struct (Num n |> Just, tks.CodeRangeTail)
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal pExpr (ctx: Ctx) (tks: Tks) = 
    let struct (e, cr) = 
        match pBool tks with
        | Just r -> r
        | Nil -> 
        match pNum tks with
        | Just r -> r
        | Nil -> 
        (Nil, tks.ToCodeRange)
    match e with
    | Nil -> Nil
    | Just e -> Just (e, cr)

let internal pTryOperMid (ctx: Ctx) (tks: Tks) = 
    match tks.First with
    | Just (Tokens.Oper o) -> todo()
    | _ -> todo()
    todo()

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal root (ctx: Ctx) (tks: Tks) =
    todo()

////////////////////////////////////////////////////////////////////////////////////////////////////

let parser (tks: Tokens []) =
    let span = tks.AsSliced()
    let ctx = CtxRef(endLocOf span)
    let r = 
        try root ctx.ToCtx span with 
        | ParserException(k) -> 
            if ctx.errs.Count > 1 then 
                ctx.errs.Add(k)
                raise <| ParserException(MultipleErrors ctx.errs)
            reraise()
    if ctx.errs.Count = 1 then raise <| ParserException(ctx.errs.[0])
    if ctx.errs.Count > 1 then raise <| ParserException(MultipleErrors ctx.errs)
    r
