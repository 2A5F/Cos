module rec Volight.Cos.Parser.Parser

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open Volight.Cos.SrcPos
open Volight.Cos.Utils
open Volight.Cos.Utils.Utils
open Volight.Cos.Utils.FlakeEx
open Volight.Cos.Utils.LinkedListExt
open Volight.Cos.Parser.KeyWords


type internal Tks = Tokens Flake

type internal CtxRef =
    val errs: ParserError List
    val endloc: Loc
    new(endloc) = { errs = List(); endloc = endloc }
    member self.ToCtx = Ctx(self, self.endloc)

and internal Ctx =
    val ctx: CtxRef
    val endloc: Loc
    new(ctx, endloc) = { ctx = ctx; endloc = endloc }

    member self.Err e = self.ctx.errs.Add(e)

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal endLocOf (tks: Tks) = match tks.Last with Just t -> t.Loc | Nil -> Loc.zero

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal pBool (tks: Tks) =
    match tks.First with
    | Just (Tokens.ID (v & { Id = Key KeyWord.True })) -> Just struct (True v |> Bool |> Just, tks.Tail)
    | Just (Tokens.ID (v & { Id = Key KeyWord.False })) -> Just struct (False v |> Bool |> Just, tks.Tail)
    | _ -> Nil

let internal pNum (ctx: Ctx) (tks: Tks) =
    match tks.First with
    | Just (Tokens.Num n) -> 
        match n.Suffix with
        | Just _ -> Just struct (PNum.New(n) |> Num |> Just, tks.Tail)
        | Nil ->
            match tks.[1] with
            | Just (Tokens.Oper (o & { Str = "." })) ->
                match tks.[2] with
                | Just (Tokens.Num d) -> 
                    match n.Prefix with
                    | Just _ -> 
                        ctx.Err(IllegalFloatingNumber(n, o, d))
                        Just struct (PNum.New(n) |> Num |> Just, tks.Tail)
                    | Nil -> Just struct (PNum.New(n, o, d) |> Num |> Just, tks.Slice 3)
                | _ -> Just struct (PNum.New(n) |> Num |> Just, tks.Tail)
            | _ -> Just struct (PNum.New(n) |> Num |> Just, tks.Tail)
    | _ -> Nil

let internal pId (tks: Tks) =
    match tks.First with
    | Just (Tokens.ID v) when v.IsIdAllowed -> Just struct (PExpr.Id v |> Just, tks.Tail)
    | _ -> Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

let internal pLabelUse (tks: Tks) =
    match tks.First with
    | Just (Tokens.At at) -> 
        match tks.[1] with
        | Just (Tokens.ID v) when v.IsIdAllowed -> 
            struct ({ At = at; Name = v } |> Just, tks.Slice(2))
        | _ -> Nil, tks
    | _ -> Nil, tks

let internal pBreak (ctx: Ctx) (tks: Tks) (thenf: pExprRes -> pExprRet) =
    match tks.First with
    | Just (Tokens.ID (v & { Id = Key KeyWord.Break })) -> 
        let struct (labelUse, tks) = pLabelUse tks.Tail
        pExprOpersThen ctx tks <| function
        | Just struct (e, r) -> thenf <| Just struct (Break <| { TBreak = v; Label = labelUse; Expr = Just e } |> Just, r)
        | Nil -> thenf <| Just struct (Break <| { TBreak = v; Label = labelUse; Expr = Nil } |> Just, tks)
    | _ -> thenf Nil

let internal pReturn (ctx: Ctx) (tks: Tks) (thenf: pExprRes -> pExprRet) =
    match tks.First with
    | Just (Tokens.ID (v & { Id = Key KeyWord.Return })) -> 
        let struct (labelUse, tks) = pLabelUse tks.Tail
        pExprOpersThen ctx tks <| function
        | Just struct (e, r) -> thenf <| Just struct (Return <| { TReturn = v; Label = labelUse; Expr = Just e } |> Just, r)
        | Nil -> thenf <| Just struct (Return <| { TReturn = v; Label = labelUse; Expr = Nil } |> Just, tks)
    | _ -> thenf Nil

let internal pContinue (tks: Tks) =
    match tks.First with
    | Just (Tokens.ID (v & { Id = Key KeyWord.Continue })) ->
        let struct (labelUse, tks) = pLabelUse tks.Tail
        Just struct (Continue <| { TContinue = v; Label = labelUse } |> Just, tks)
    | _ -> Nil

let internal pReturnArrow (ctx: Ctx) (tks: Tks) (thenf: pExprRes -> pExprRet) =
    match tks.First with
    | Just (Tokens.DArrow a) -> 
        let struct (labelUse, tks) = pLabelUse tks.Tail
        pExprOpersThen ctx tks <| function
        | Just struct (e, r) -> thenf <| Just struct (ReturnArrow <| { Arrow = a; Label = labelUse; Expr = e } |> Just, r)
        | Nil -> thenf <| Nil
    | _ -> thenf Nil

////////////////////////////////////////////////////////////////////////////////////////////////////

type internal pExprRet = (struct (PExpr * Tks)) Maybe

type internal pExprRes = (struct (PExpr Maybe * Tks)) Maybe

let internal pExprFinish (thenf: pExprRet -> pExprRet) e cr =
    match e with
    | Nil -> thenf Nil
    | Just e -> thenf <| Just struct (e, cr)

let internal pExpr (ctx: Ctx) (tks: Tks) (thenf: pExprRet -> pExprRet) = 
    match pBool tks with
    | Just (e, cr) -> pExprFinish thenf e cr
    | Nil -> 
    match pNum ctx tks with
    | Just (e, cr) -> pExprFinish thenf e cr
    | Nil -> 
    match pContinue tks with
    | Just (e, cr) -> pExprFinish thenf e cr
    | Nil ->
    pBreak ctx tks <| function
    | Just (e, cr) -> pExprFinish thenf e cr
    | Nil -> 
    pReturn ctx tks <| function
    | Just (e, cr) -> pExprFinish thenf e cr
    | Nil ->
    pReturnArrow ctx tks <| function
    | Just (e, cr) -> pExprFinish thenf e cr
    | Nil -> 
    match pId tks with
    | Just (e, cr) -> pExprFinish thenf e cr
    | Nil ->
    pExprFinish thenf Nil tks 



type internal PCExprOper = PCExpr of PExpr | PCOper of TOper

type internal TCollectExprOpersThen = LinkedList<PCExprOper> -> LinkedList<struct (LinkedListNode<PCExprOper> * PExpr ref)> -> Tks -> pExprRet

let internal pCollectExprOpers (ctx: Ctx) (tks: Tks) (list: PCExprOper LinkedList) (exprs: struct (PCExprOper LinkedListNode * PExpr ref) LinkedList) (thenf: TCollectExprOpersThen) =
    pExpr ctx tks <| function
    | Just struct (e, cr) -> 
        let n = list.PushLast(PCExpr e)
        exprs.PushLast(struct (n, ref e)) |> ignore
        pCollectExprOpers ctx cr list exprs thenf
    | Nil ->
    match tks.First with
    | Just (Tokens.Oper o) ->
        list.PushLast(PCOper o) |> ignore
        pCollectExprOpers ctx tks.Tail list exprs thenf
    | _ -> thenf list exprs tks

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
    | Just _ -> pExprOpersEdgeDoLeft list node e o
    | Nil -> true

let internal pExprOpersEdgeRight (list: PCExprOper LinkedList) (node: PCExprOper LinkedListNode) (e: PExpr ref) (o: TOper) =
    match Operators.rightInfoMap.TryGet(o.Str) with
    | Just _ -> pExprOpersEdgeDoRight list node e o
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

let internal pExprOpersClean (ctx: Ctx) (list: PCExprOper LinkedList) (list2: PCExprOper2 LinkedList) =
    for i in list do
        match i with
        | PCExpr e -> list2.PushLast(PC2Expr e) |> ignore
        | PCOper o -> 
            match Operators.midInfoMap.TryGet o.Str with
            | Just info -> 
                list2.PushLast(PC2Oper (o, info)) |> ignore
            | Nil -> 
                ctx.Err(UnknownOperator o)
                list2.PushLast(PC2Oper (o, Operators.defaultInfo o.Str)) |> ignore
    list2

let rec inline internal pExprOpersTakeExpr (node: PCExprOper2) =
    match node with
    | PC2Expr e -> e
    | PC2Oper (_, _) -> raise <| NotImplementedException($"internal error at {nameof pExprOpersTakeExpr} : Find {nameof PC2Oper}")

let rec inline internal pExprOpersTakeOper (node: PCExprOper2) =
    match node with
    | PC2Expr _ -> raise <| NotImplementedException($"internal error at {nameof pExprOpersTakeOper} : Find {nameof PC2Expr}")
    | PC2Oper (o, i) -> o, i

let rec internal pExprOpersMid (list: PCExprOper2 LinkedList) (node: PCExprOper2 LinkedListNode) =
    if list.IsOnlyOne then pExprOpersTakeExpr list.First.Value else
    if isNull node then pExprOpersMid list list.First else
    match node.Value with
    | PC2Expr _ -> pExprOpersMid list node.Next
    | PC2Oper (o, info) ->
    let left, right = node.Prev, node.Next
    if isNull left || isNull right then raise <| NotImplementedException($"internal error at {nameof pExprOpersMid} : Unexpected Edge Operator")
    let nextNext = llnTryValue right.Next
    let left, right = pExprOpersTakeExpr left.Value, pExprOpersTakeExpr right.Value
    match nextNext with
    | Just nn -> 
        let _, i = pExprOpersTakeOper nn
        match info, i with
        | { Level = ll }, { Level = rl } when ll > rl -> pExprOpersMidReduce list node left right o
        | { Level = ll }, { Level = rl } when ll < rl -> pExprOpersMid list node.Next
        | { Assoc = Left }, _ -> pExprOpersMidReduce list node left right o
        | { Assoc = Right }, _ -> pExprOpersMid list node.Next
    | Nil -> pExprOpersMidReduce list node left right o

and internal pExprOpersMidReduce (list: PCExprOper2 LinkedList) (node: PCExprOper2 LinkedListNode) (l: PExpr) (r: PExpr) (o: TOper) =
    let ne = Oper { Left = l; Oper = o; Right = r }
    node.Value <- PC2Expr ne
    node.RemoveNext(list) |> ignore
    node.RemovePrev(list) |> ignore
    pExprOpersMid list node.Next

let internal pExprOpersThen (ctx: Ctx) (tks: Tks) (thenf: pExprRet -> pExprRet) = 
    pCollectExprOpers ctx tks (LinkedList()) (LinkedList()) <| fun eos exprs cr ->
    if eos.Count = 0 then Nil else
        pExprOpersEdge eos exprs exprs.First
        let eos = pExprOpersClean ctx eos (LinkedList())
        let r = pExprOpersMid eos eos.First
        thenf <| Just (r, cr)

let internal pExprOpers (ctx: Ctx) (tks: Tks) = 
    pExprOpersThen ctx tks Operators.id



////////////////////////////////////////////////////////////////////////////////////////////////////

let internal root (ctx: Ctx) (tks: Tks) =
    let r = pExprOpers ctx tks
    r
    //todo()

////////////////////////////////////////////////////////////////////////////////////////////////////

let parser (tks: Tokens []) =
    let span = tks.AsFlake()
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
