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
    member self.Len = self.span.Length
    member self.IsEmpty = self.span.IsEmpty
    member self.Item with get i = if i < 0 || i >= self.span.Length then Nil else Just self.span.[i]
    member self.GetUnchecked i = self.span.[i]
    member inline self.Get i = self.[i]
    member self.OGet i = if i < 0 || i >= self.span.Length then None else Some self.span.[i]
    member self.VGet i = if i < 0 || i >= self.span.Length then ValueNone else ValueSome self.span.[i]

    member inline self.Slice() = self
    member self.Slice(s) = Sliced(self.span.Slice(s), self.offset + s)
    member self.Slice(s, e) = Sliced(self.span.Slice(s, e - s), self.offset + s)
    member self.SliceTo(e) = Sliced(self.span.Slice(0, e), self.offset)

    member inline self.GetSlice(s, e) = 
        match s with
        | None -> 
            match e with
            | None -> self.Slice()
            | Some e -> self.SliceTo(e)
        | Some s -> 
            match e with
            | None -> self.Slice(s)
            | Some e -> self.Slice(s, e)

    member self.First = if self.IsEmpty then Nil else Just self.span.[0]
    member self.Last = if self.IsEmpty then Nil else Just self.span.[self.span.Length - 1]
    member self.Tail = self.Slice(1)

    member self.OFirst = if self.IsEmpty then None else Some self.span.[0]
    member self.OLast = if self.IsEmpty then None else Some self.span.[self.span.Length - 1]
    member self.VFirst = if self.IsEmpty then ValueNone else ValueSome self.span.[0]
    member self.VLast = if self.IsEmpty then ValueNone else ValueSome self.span.[self.span.Length - 1]

    member self.RawIndex i = self.offset + i
    member self.FromRawIndex i = i - self.offset
    
    member self.ToArray() = self.span.ToArray()
    override self.ToString() = self.span.ToString()

    member self.ToArr = self.span.ToArray()
    member self.ToStr = self.span.ToString()

    member self.GetEnumerator() = self.span.GetEnumerator()
    
    member self.ToCodeRange : CodeRange = { From = self.offset; To = self.RawIndex self.Len } 
    member self.CodeRangeTo i : CodeRange = { From = self.offset; To = self.RawIndex i } 
    member self.CodeRangeFrom i : CodeRange = { From = self.RawIndex i; To = self.RawIndex self.Len } 
    member self.CodeRange(s, e) : CodeRange = { From = self.RawIndex s; To = self.RawIndex e } 
    member self.ByCodeRange (c: CodeRange) = self.Slice(self.FromRawIndex c.From, self.FromRawIndex c.To)


module SlicedEx =
    type Span<'T> with
        member inline self.ToSliced() = Sliced(Span<_>.op_Implicit(self))
    type ReadOnlySpan<'T> with
        member inline self.ToSliced() = Sliced(self)
    type String with
        member self.AsSliced() = self.AsSpan().ToSliced()
        member self.AsSliced(start) = self.AsSpan(start).ToSliced()
        member self.AsSliced(start, ``end``) = self.AsSpan().ToSliced().Slice(start, ``end``)
    type ``[]``<'T> with
        member self.AsSliced() = self.AsSpan().ToSliced()
        member self.AsSliced(start) = self.AsSpan().ToSliced().Slice(start)
        member self.AsSliced(start, ``end``) = self.AsSpan().ToSliced().Slice(start, ``end``)
