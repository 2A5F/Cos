namespace rec Volight.Cos.SrcPos

open System.Runtime.CompilerServices
open System.Diagnostics
open Volight.Cos.Utils

[<Struct; IsReadOnly; DebuggerDisplay(@"Pos \{ {ToString(),nq} \}")>]
type Pos = 
    { Line: uint; Column: uint }
    override self.ToString() = $"{self.Line}:{self.Column}"
    member inline self.Pos = self
    static member inline OfV line column = { Line = line; Column = column }
    static member zero = Pos.OfV 0u 0u

[<Struct; IsReadOnly; DebuggerDisplay(@"Loc \{ {ToString(),nq} \}")>]
type Loc = 
    { From: Pos; To: Pos }
    override self.ToString() = $"{self.From.Line}:{self.From.Column} .. {self.To.Line}:{self.To.Column}"
    member inline self.Loc = self
    static member inline OfP from To = { From = from; To = To }
    static member inline OfV fl fc tl tc = { From = Pos.OfV fl fc; To = Pos.OfV tl tc }
    static member inline OfMP< ^a, ^b, ^c when ^a : (member Loc: Loc) and ^b : (member Loc: Loc) and ^c : (member Loc: Loc)> (f: ^a Maybe) (t: ^b Maybe) (e: ^c)  = 
        match f with
        | Just f ->
            match t with
            | Just t -> Loc.OfP (^a : (member Loc: Loc) f).From (^b : (member Loc: Loc) t).To
            | Nil -> Loc.OfP (^a : (member Loc: Loc) f).From (^c : (member Loc: Loc) e).To
        | Nil -> 
            match t with
            | Just t -> Loc.OfP (^c : (member Loc: Loc) e).From (^b : (member Loc: Loc) t).To
            | Nil -> (^c : (member Loc: Loc) e)
    static member zero = Loc.OfP Pos.zero Pos.zero
    member inline self.Merge (other: Loc) = Loc.OfP self.From other.To
    static member inline May< ^a when ^a : (member Loc: Loc)> (m: ^a Maybe) d = 
        match m with
        | Just v -> (^a : (member Loc: Loc) v)
        | Nil -> d
    member inline self.MayMerge< ^a when ^a : (member Loc: Loc)> (m: ^a Maybe) =
        match m with
        | Just v -> self.Merge (^a : (member Loc: Loc) v)
        | Nil -> self

[<Struct; IsReadOnly; DebuggerDisplay(@"CodeRange \{ {ToString(),nq} \}")>]
type CodeRange =
    { From: int; To: int }
    member self.Len = self.To - self.From
    override self.ToString() = $"{self.From}..{self.To}"
    static member inline New (from, To) = { From = from; To = To }

    member inline self.Slice() = self
    member self.Slice(start) = CodeRange.New(self.From + start, self.To)
    member self.Slice(start, _end) = CodeRange.New(self.From + start, self.From + _end)
    member self.SliceTo(_end) = CodeRange.New(self.From, self.From + _end)
    
    member self.GetSlice(start, _end) = 
        match start with
        | None -> 
            match _end with
            | None -> self.Slice()
            | Some e -> self.SliceTo(e)
        | Some s -> 
            match _end with
            | None -> self.Slice(s)
            | Some e -> self.Slice(s, e)

    static member inline OfV from To = { From = from; To = To }
