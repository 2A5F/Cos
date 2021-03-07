module Volight.Cos.Parser.Parser

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open Volight.Cos.SrcPos
open Volight.Cos.Utils
open Volight.Cos.Utils.Utils
open Volight.Cos.Utils.SlicedEx
open Volight.Cos.Utils.LinkedListExt
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

    member self.Err(e) = self.ctx.errs.Add(e)

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


type internal PCExprOper = PCExpr of PExpr | PCOper of TOper

let rec internal pCollectExprOpers (ctx: Ctx) (tks: Tks) (list: PCExprOper LinkedList) (exprs: struct (PCExprOper LinkedListNode * PExpr ref) LinkedList) =
    match pExpr ctx tks with
    | Just (e, cr) -> 
        let n = list.PushLast(PCExpr e)
        exprs.PushLast(struct (n, ref e)) |> ignore
        pCollectExprOpers ctx (tks.ByCodeRange cr) list exprs
    | Nil ->
    match tks.First with
    | Just (Tokens.Oper o) ->
        list.PushLast(PCOper o) |> ignore
        pCollectExprOpers ctx tks.Tail list exprs
    | _ -> (list, exprs, tks.ToCodeRange)

let internal pExprOpersEdgeDoLeft (list: PCExprOper LinkedList) (node: PCExprOper LinkedListNode) (e: PExpr ref) (o: TOper) =
    let ne = OperLeft <| { Oper = o; Right = !e }
    node.RemovePrev(list) |> ignore
    node.Value <- PCExpr ne
    e := ne
    false

let internal pExprOpersEdgeDoRight (list: PCExprOper LinkedList) (node: PCExprOper LinkedListNode) (e: PExpr ref) (o: TOper) =
    let ne = OperRight <| { Left = !e; Oper = o }
    node.RemoveNext(list) |> ignore
    node.Value <- PCExpr ne
    e := ne
    false

let internal pExprOpersEdgeLeft (list: PCExprOper LinkedList) (node: PCExprOper LinkedListNode) (e: PExpr ref) (o: TOper) =
    match Operators.leftInfoMap.TryGet(o.Str) with
    | Just _ -> pExprOpersEdgeDoRight list node e o
    | Nil -> true

let internal pExprOpersEdgeRight (list: PCExprOper LinkedList) (node: PCExprOper LinkedListNode) (e: PExpr ref) (o: TOper) =
    match Operators.rightInfoMap.TryGet(o.Str) with
    | Just _ -> pExprOpersEdgeDoLeft list node e o
    | Nil -> true

let internal pExprOpersEdgeBoth (list: PCExprOper LinkedList) (node: PCExprOper LinkedListNode) (e: PExpr ref) (lo: TOper) (ro: TOper) =
    match Operators.leftInfoMap.TryGet(lo.Str), Operators.rightInfoMap.TryGet(ro.Str) with
    | Just { Level = ll }, Just { Level = rl } -> 
        if ll >= rl then pExprOpersEdgeDoRight list node e lo
        else pExprOpersEdgeDoRight list node e ro
    | Nil, Just _ -> pExprOpersEdgeDoRight list node e ro
    | Just _, Nil -> pExprOpersEdgeDoLeft list node e lo
    | _ -> true

let internal pExprOpersEdgeBody (list: PCExprOper LinkedList) (node: PCExprOper LinkedListNode) (e: PExpr ref) =
    match llnTryValue node.Prev, llnTryValue node.Next with
    | Just (PCOper lo), Just (PCOper ro) -> // 1 + 1
        match llnTryValue node.Prev.Prev, llnTryValue node.Next.Next with
        | Just (PCExpr _), Just (PCExpr _) -> true
        | _, Just (PCExpr _) -> pExprOpersEdgeLeft list node e lo
        | Just (PCExpr _), _ -> pExprOpersEdgeRight list node e ro
        | _ -> pExprOpersEdgeBoth list node e lo ro
    | _, Just (PCOper ro) -> 
        match llnTryValue node.Next.Next with
        | Just (PCExpr _) -> true
        | _ -> pExprOpersEdgeRight list node e ro
    | Just (PCOper lo), _ -> 
        match llnTryValue node.Prev.Prev with
        | Just (PCExpr _) -> true
        | _ -> pExprOpersEdgeLeft list node e lo
    | _ -> true

let rec internal pExprOpersEdge (list: PCExprOper LinkedList) (exprs: struct (PCExprOper LinkedListNode * PExpr ref) LinkedList) (n: struct (PCExprOper LinkedListNode * PExpr ref) LinkedListNode) = 
    if exprs.IsEmpty then () else
    if isNull n then pExprOpersEdge list exprs exprs.First else
    let struct (node, e) = n.Value
    if pExprOpersEdgeBody list node e then 
        let next = n.Next
        n.Remove(exprs)
        pExprOpersEdge list exprs next
    else pExprOpersEdge list exprs n.Next

type internal PCExprOper2 = PC2Expr of PExpr | PC2Oper of TOper * OperInfo

let rec internal pExprOpersClean (ctx: Ctx) (list: PCExprOper LinkedList) (list2: PCExprOper2 LinkedList) =
    list2

let internal pExprOpers (ctx: Ctx) (tks: Tks) = 
    let (eos, exprs, cr) = pCollectExprOpers ctx tks (LinkedList()) (LinkedList())
    if eos.Count = 0 then Nil else
    pExprOpersEdge eos exprs exprs.First
    let eos = pExprOpersClean ctx eos (LinkedList())
    todo()

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal root (ctx: Ctx) (tks: Tks) =
    let r = pExprOpers ctx tks
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
