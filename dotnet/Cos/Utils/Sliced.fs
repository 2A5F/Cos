namespace rec Volight.Cos.Utils

open System
open System.Runtime.CompilerServices
open Volight.Cos.SrcPos

[<Struct; IsReadOnly; IsByRefLike;>]
type Sliced<'T> =
    val span: ReadOnlySpan<'T>
    val offset: int
    new (span) = { span = span; offset = 0 }
    new (span, offset) = { span = span; offset = offset }
    member self.len = self.span.Length
    member self.isEmpty = self.span.IsEmpty
    member self.Item with get i = if i < 0 || i >= self.span.Length then Nil else Just self.span.[i]
    member self.get_unchecked i = self.span.[i]
    member inline self.get i = self.[i]
    member self.oget i = if i < 0 || i >= self.span.Length then None else Some self.span.[i]
    member self.vget i = if i < 0 || i >= self.span.Length then ValueNone else ValueSome self.span.[i]

    member inline self.slice() = self
    member self.slice(s) = Sliced(self.span.Slice(s), self.offset + s)
    member self.slice(s, e) = Sliced(self.span.Slice(s, e - s), self.offset + s)
    member self.sliceTo(e) = Sliced(self.span.Slice(0, e), self.offset)

    member inline self.GetSlice(s, e) = 
        match s with
        | None -> 
            match e with
            | None -> self.slice()
            | Some e -> self.sliceTo(e)
        | Some s -> 
            match e with
            | None -> self.slice(s)
            | Some e -> self.slice(s, e)

    member self.first = if self.isEmpty then Nil else Just self.span.[0]
    member self.last = if self.isEmpty then Nil else Just self.span.[self.span.Length - 1]
    member self.tail = self.slice(1)

    member self.ofirst = if self.isEmpty then None else Some self.span.[0]
    member self.olast = if self.isEmpty then None else Some self.span.[self.span.Length - 1]
    member self.vfirst = if self.isEmpty then ValueNone else ValueSome self.span.[0]
    member self.vlast = if self.isEmpty then ValueNone else ValueSome self.span.[self.span.Length - 1]

    member self.rawIndex i = self.offset + i
    member self.fromRawIndex i = i - self.offset
    
    member self.ToArray() = self.span.ToArray()
    override self.ToString() = self.span.ToString()

    member self.toArr = self.span.ToArray()
    member self.toStr = self.span.ToString()

    member self.GetEnumerator() = self.span.GetEnumerator()
    
    member self.ToCodeRange : CodeRange = { from = self.offset; ``to`` = self.rawIndex self.len } 
    member self.CodeRangeTo i : CodeRange = { from = self.offset; ``to`` = self.rawIndex i } 
    member self.CodeRangeFrom i : CodeRange = { from = self.rawIndex i; ``to`` = self.rawIndex self.len } 
    member self.CodeRange(s, e) : CodeRange = { from = self.rawIndex s; ``to`` = self.rawIndex e } 
    member self.ByCodeRange (c: CodeRange) = self.slice(self.fromRawIndex c.from, self.fromRawIndex c.``to``)


module SlicedEx =
    type Span<'T> with
        member inline self.ToSliced() = Sliced(Span<_>.op_Implicit(self))
    type ReadOnlySpan<'T> with
        member inline self.ToSliced() = Sliced(self)
    type String with
        member self.AsSliced() = self.AsSpan().ToSliced()
        member self.AsSliced(start) = self.AsSpan(start).ToSliced()
        member self.AsSliced(start, ``end``) = self.AsSpan().ToSliced().slice(start, ``end``)
