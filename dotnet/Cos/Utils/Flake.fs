namespace rec Volight.Cos.Utils

open System.Runtime.CompilerServices
open Volight.Cos.SrcPos
open System

[<Struct; IsReadOnly;>]
type 'T Flake =
    { arr: 'T []; off: int; len: int }

    static member New(arr) = { arr = arr; off = 0; len = arr.Length }
    static member New(arr, off) = { arr = arr; off = off; len = arr.Length - off }
    static member New(arr, off, len) = { arr = arr; off = off; len = len }

    member inline self.Len = self.len
    member inline self.Length = self.len
    member self.IsEmpty = self.len = 0
    member self.Item with get i = if i < 0 || i >= self.len then Nil else Just self.arr.[i + self.off]
    member self.GetUnchecked i = self.arr.[i + self.off]
    member inline self.Get i = self.[i]
    member self.OGet i = if i < 0 || i >= self.len then None else Some self.arr.[i + self.off]
    member self.VGet i = if i < 0 || i >= self.len then ValueNone else ValueSome self.arr.[i + self.off]

    member inline self.Slice() = self
    member self.Slice(s) = { arr = self.arr; off = self.off + s; len = self.len - s }
    member self.Slice(s, e) = { arr = self.arr; off = self.off + s; len = e - s }
    member self.SliceTo(e) = { arr = self.arr; off = self.off; len = e }
    member inline self.TrySlice() = Just self
    member self.TrySlice(s) = 
        if s < 0 || s > self.len then Nil else
        Just { arr = self.arr; off = self.off + s; len = self.len - s }
    member self.TrySlice(s, e) = 
        if s < 0 || e < 0 || s > e || e > self.len then Nil else
        Just { arr = self.arr; off = self.off + s; len = e - s }
    member self.TrySliceTo(e) = 
        if e < 0 || e > self.len then Nil else
        Just { arr = self.arr; off = self.off; len = e }

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

    member self.First = if self.IsEmpty then Nil else Just self.arr.[self.off]
    member self.Last = if self.IsEmpty then Nil else Just self.arr.[self.off + self.len - 1]
    member self.Tail = self.Slice(1)
    member inline self.Self = self

    member self.OFirst = if self.IsEmpty then None else Some self.arr.[self.off]
    member self.OLast = if self.IsEmpty then None else Some self.arr.[self.off + self.len - 1]
    member self.VFirst = if self.IsEmpty then ValueNone else ValueSome self.arr.[self.off]
    member self.VLast = if self.IsEmpty then ValueNone else ValueSome self.arr.[self.off + self.len - 1]

    member self.RawIndex i = self.off + i
    member self.FromRawIndex i = i - self.off

    member self.ToSpan() = Span(self.arr, self.off, self.len)
    member self.ToReadOnlySpan() = ReadOnlySpan(self.arr, self.off, self.len)
    member self.ToSliced() = Sliced(self.ToReadOnlySpan(), self.off)
    member self.ToArray() = self.ToReadOnlySpan().ToArray()
    member self.ToArraySegment() = ArraySegment(self.arr, self.off, self.len)
    override self.ToString() = 
        if typeof<'T> = typeof<char> then
            self.ToReadOnlySpan().ToString()
        else if self.IsEmpty then
            "[]"
        else
            let str = String.Join(", ", self.ToArraySegment())
            $"[{str}]"

    member self.GetEnumerator() = self.ToReadOnlySpan().GetEnumerator()

    member self.ToArr = self.ToArray()
    member self.ToStr = self.ToString()
    member self.span = self.ToSpan()
    member self.roSpan = self.ToReadOnlySpan()
    member self.ToSlice = self.ToSliced()
    member self.ToArrSeg = self.ToArraySegment()

    member self.ToCodeRange : CodeRange = { From = self.off; To = self.RawIndex self.Len } 
    member self.CodeRangeTo i : CodeRange = { From = self.off; To = self.RawIndex i } 
    member self.CodeRangeFrom i : CodeRange = { From = self.RawIndex i; To = self.RawIndex self.Len } 
    member inline self.CodeRangeTail : CodeRange = self.CodeRangeFrom 1
    member self.CodeRange(s, e) : CodeRange = { From = self.RawIndex s; To = self.RawIndex e } 
    member self.ByCodeRange (c: CodeRange) = self.Slice(self.FromRawIndex c.From, self.FromRawIndex c.To)

module FlakeEx =
    type ``[]``<'T> with
        member self.AsFlake() = Flake<'T []>.New(self)
        member self.AsFlake(start) = Flake<'T []>.New(self).Slice(start)
        member self.AsFlake(start, ``end``) = Flake<'T []>.New(self).Slice(start, ``end``)
