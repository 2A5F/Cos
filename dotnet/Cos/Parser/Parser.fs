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

let rec internal pCollectExprOpers (ctx: Ctx) (tks: Tks) (list: PCExprOper List) =
    match pExpr ctx tks with
    | Just (e, cr) -> 
        list.Add(PCExpr e)
        pCollectExprOpers ctx (tks.ByCodeRange cr) list
    | Nil ->
    match tks.First with
    | Just (Tokens.Oper o) ->
        list.Add(PCOper o)
        pCollectExprOpers ctx tks.Tail list
    | _ -> (list, tks.ToCodeRange)

type internal PCExprOper2 = PC2Expr of PExpr | PC2OperMid of TOper * OperInfo | PC2OperEdge of TOper * OperInfo

let rec internal pCollectExprOpers2 (ctx: Ctx) (list: PCExprOper List) i (list2: PCExprOper2 LinkedList) =
    if i >= list.Count then list2 else
    match list.[i] with
    | PCOper o -> 
        if (i > 1 && i < list.Count - 1 && match (list.[i - 1], list.[i + 1]) with (PCExpr _, PCExpr _) -> true | _ -> false) then
            let info = 
                if Operators.midCanAlone o then
                    match Operators.midInfoMap.TryGet(o.Str) with 
                    | Just info -> info
                    | _ -> 
                        ctx.Err(ParserError.UnknownOperator o)
                        Operators.defaultInfo o.Str
                else 
                    ctx.Err(ParserError.IllegalAloneOperator o)
                    Operators.defaultInfo o.Str
            list2.PushLast(PC2OperMid (o, info)) |> ignore
            pCollectExprOpers2 ctx list (i + 1) list2
        else
            todo()
    | PCExpr e -> 
        list2.PushLast(PC2Expr e) |> ignore
        pCollectExprOpers2 ctx list (i + 1) list2

let rec internal pExprOpersEdgePart1 (list: PCExprOper2 LinkedList) (node: PCExprOper2 LinkedListNode) =
    if isNull node then () else
    match node.Value with
    | PC2Expr e -> pExprOpersEdgePart2 list node e
    | _ -> pExprOpersEdgePart1 list node.Next
and internal pExprOpersEdgePart2 (list: PCExprOper2 LinkedList) (node: PCExprOper2 LinkedListNode) e =
    match llnTryValue node.Prev, llnTryValue node.Next with
    | Just (PC2OperEdge (lo, { Level = ll; Assoc = Left })), Just (PC2OperEdge (_, { Level = rl; Assoc = Right })) when ll >= rl -> 
        let ne = OperLeft <| { Oper = lo; Right = e } 
        node.RemovePrev(list) |> ignore
        node.Value <- PC2Expr ne
        pExprOpersEdgePart2 list node ne
    | Just (PC2OperEdge (_, { Level = ll; Assoc = Left })), Just (PC2OperEdge (ro, { Level = rl; Assoc = Right })) when ll < rl -> 
        let ne = OperRight <| { Left = e; Oper = ro } 
        node.RemoveNext(list) |> ignore
        node.Value <- PC2Expr ne
        pExprOpersEdgePart2 list node ne
    | Just (PC2OperEdge (lo, { Assoc = Left })), _ -> 
        let ne = OperLeft <| { Oper = lo; Right = e } 
        node.Value <- PC2Expr ne
        pExprOpersEdgePart2 list node ne
    | _, Just (PC2OperEdge (ro, { Assoc = Right })) ->
        let ne = OperRight <| { Left = e; Oper = ro } 
        node.Value <- PC2Expr ne
        pExprOpersEdgePart2 list node ne
    | _ -> pExprOpersEdgePart1 list node.Next

type internal PCExprOper3 = PC3Expr of PExpr | PC3Oper of TOper * OperInfo

let internal pExprOpersClean (ctx: Ctx) (list1: PCExprOper2 LinkedList) (list2: PCExprOper3 LinkedList) =
    for i in list1 do
        match i with
        | PC2Expr e -> list2.PushLast(PC3Expr e) |> ignore
        | PC2OperMid (o, i) -> list2.PushLast(PC3Oper (o, i)) |> ignore
        | PC2OperEdge (o, _) -> ctx.Err(IllegalEdgeOperator o)
    list2

let rec internal pExprOpersMid (ctx: Ctx) (list: PCExprOper3 LinkedList) (node: PCExprOper3 LinkedListNode) =
    todo()

//let rec internal pExprOpersStart (ctx: Ctx) (list: PCExprOper2 LinkedList) (node: PCExprOper2 LinkedListNode) =
//    match node.Value with
//    | PC2OperMid (o, { Level = level; Assoc = Left }) -> 
//        let leftOper = llnTryValue node.Prev.Prev
//        let rightOper = llnTryValue node.Next.Next
//        match leftOper with
//        | Just (PC2OperMid (_, { Level = ll })) when ll > level -> pExprOpersStart ctx list node.Next
//        | Just (PC2OperMid (_, { Level = ll; Assoc = Left })) when ll = level -> pExprOpersStart ctx list node.Next
//        | _ -> todo()
        
//    | PC2OperMid (o, { Level = level; Assoc = Right }) -> 
//        todo()
//    | PC2OperEdge (o, info, Left) -> 
//        todo()
//    | PC2OperEdge (o, info, Right) -> 
//        todo()
//    | PC2Expr e ->
//        if node.HasNext then pExprOpersStart ctx list node.Next else 
//        if node.HasPrev then pExprOpersStart ctx list list.First else e

let inline internal pExprOpersTakeFirst (ctx: Ctx) (l: PCExprOper3 LinkedList) (cr: CodeRange) =
    match l.First.Value with
    | PC3Expr e -> Just (e, cr)
    | PC3Oper (_, _) -> raise <| ParserException(InternalError "pExprOpersTakeFirst")

let internal pExprOpers (ctx: Ctx) (tks: Tks) = 
    let (eos, cr) = pCollectExprOpers ctx tks (List())
    if eos.Count = 0 then Nil else
    let eos = pCollectExprOpers2 ctx eos 0 (LinkedList())
    pExprOpersEdgePart1 eos eos.First
    let eos = pExprOpersClean ctx eos (LinkedList())
    if eos.IsEmpty then raise <| ParserException(InternalError "pExprOpers")
    if eos.IsOnlyOne then pExprOpersTakeFirst ctx eos cr else
    let r = pExprOpersMid ctx eos eos.First
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
