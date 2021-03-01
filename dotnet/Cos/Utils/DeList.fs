namespace rec Volight.Cos.Utils

open System.Collections.Generic
open System.Collections
open System.Diagnostics.CodeAnalysis

[<AllowNullLiteral>]
type 'T dlist (value: 'T, prev: 'T dlist, next: 'T dlist) =
    let mutable prev = prev
    let mutable next = next

    new (value) = dlist(value, null, null)

    member val Value = value with get, set
    member _.Prev with get() : [<MaybeNull>] 'T dlist = prev and internal set ([<AllowNull>] v: 'T dlist) = prev <- v
    member _.Next with get() : [<MaybeNull>] 'T dlist = next and internal set ([<AllowNull>] v: 'T dlist) = next <- v

    member _.HasPrev = prev <> null
    member _.HasNext = next <> null

    member _.TryPrev = if prev = null then Nil else Just prev
    member _.TryNext = if next = null then Nil else Just next

    member _.GetPrev = if prev = null then Nil else Just prev.Value
    member _.GetNext = if next = null then Nil else Just next.Value

    member s.InsertNext v : [<NotNull>] 'T dlist = 
        let now = next
        let node = dlist(v, s, now)
        if now <> null then now.Prev <- node
        next <- node
        node
    member s.InsertPrev v : [<NotNull>] 'T dlist = 
        let now = prev
        let node = dlist(v, now, s)
        if now <> null then now.Next <- node
        prev <- node
        node

    member s.ReplaceNext ([<AllowNull>] v: 'T dlist) = 
        next <- v
        if v <> null then v.Prev <- s
    member s.ReplacePrev ([<AllowNull>] v: 'T dlist) = 
        prev <- v
        if v <> null then v.Next <- s

    
    member s.RemoveNext() : [<MaybeNull>] 'T dlist =
        if next = null then null else 
        let now = next
        s.Next <- now.Next
        now
    member s.RemovePrev() : [<MaybeNull>] 'T dlist =
        if prev = null then null else 
        let now = prev
        s.Prev <- now.Prev
        now

    member s.ToEnd = DListToEnd(s)
    member s.ToHead = DListToHead(s)

    member _.Remove() =
        if next <> null then next.Prev <- prev
        if prev <> null then prev.Next <- next
        let r = struct (prev, next)
        next <- null
        prev <- null
        r

type DListToEnd<'T> =
    val list: 'T dlist

    new (l) = { list = l; }

    member inline s.GetEnumerator() = new DListToEndIter<'T>(s.list)

    interface IEnumerable<'T> with
        member s.GetEnumerator(): IEnumerator = s.GetEnumerator() :> IEnumerator
        member s.GetEnumerator() = s.GetEnumerator() :> IEnumerator<'T>

type DListToEndIter<'T> =
    val source: 'T dlist
    val mutable list: 'T dlist

    new (l) = { source = l; list = l }
    new (s, l) = { source = s; list = l }

    member inline s.Current = s.list.Value
    member inline s.MoveNext(): bool = 
        if not s.list.HasNext then false else
        s.list <- s.list.Next
        true
    member inline s.Reset(): unit = s.list <- s.source

    member inline s.GetEnumerator() = s

    interface IEnumerator<'T> with
        member s.Current = s.Current
        member s.Current: obj = s.Current :> obj
        member _.Dispose(): unit = ()
        member s.MoveNext(): bool = s.MoveNext()
        member s.Reset(): unit = s.Reset()

    interface IEnumerable<'T> with
        member s.GetEnumerator(): IEnumerator = s :> IEnumerator
        member s.GetEnumerator() = s :> IEnumerator<'T>
      
type DListToHead<'T> =
    val list: 'T dlist

    new (l) = { list = l; }

    member inline s.GetEnumerator() = new DListToHeadIter<'T>(s.list)

    interface IEnumerable<'T> with
        member s.GetEnumerator(): IEnumerator = s.GetEnumerator() :> IEnumerator
        member s.GetEnumerator() = s.GetEnumerator() :> IEnumerator<'T>

type DListToHeadIter<'T> =
    val source: 'T dlist
    val mutable list: 'T dlist

    new (l) = { source = l; list = l }
    new (s, l) = { source = s; list = l }

    member inline s.Current = s.list.Value
    member inline s.MoveNext(): bool = 
        if not s.list.HasPrev then false else
        s.list <- s.list.Prev
        true
    member inline s.Reset(): unit = s.list <- s.source

    member inline s.GetEnumerator() = s

    interface IEnumerator<'T> with
        member s.Current = s.Current
        member s.Current: obj = s.Current :> obj
        member _.Dispose(): unit = ()
        member s.MoveNext(): bool = s.MoveNext()
        member s.Reset(): unit = s.Reset()

    interface IEnumerable<'T> with
        member s.GetEnumerator(): IEnumerator = s :> IEnumerator
        member s.GetEnumerator() = s :> IEnumerator<'T>

module DList =
    let rec Last_ (n: 'a dlist) =
        let next = n.Next
        if next = null then n else 
        Last_ next
    let Last (n: 'a dlist) : [<MaybeNull>] 'a dlist =
        if n = null then null else
        Last_ n

    let rec Head_ (n: 'a dlist) : [<MaybeNull>] 'a dlist =
        let prev = n.Prev
        if prev = null then n else 
        Head_ prev
    let rec Head (n: 'a dlist) : [<MaybeNull>] 'a dlist =
        if n = null then null else
        Head_ n
