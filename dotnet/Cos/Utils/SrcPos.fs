namespace rec Volight.Cos.SrcPos

open System.Runtime.CompilerServices
open System.Diagnostics
open Volight.Cos.Utils

[<Struct; IsReadOnly; DebuggerDisplay(@"Pos \{ {ToString(),nq} \}")>]
type Pos = 
    { line: uint; column: uint }
    override self.ToString() = $"{self.line}:{self.column}"
    member inline self.pos = self
    static member inline ofv line column = { line = line; column = column }
    static member zero = Pos.ofv 0u 0u

[<Struct; IsReadOnly; DebuggerDisplay(@"Loc \{ {ToString(),nq} \}")>]
type Loc = 
    { from: Pos; ``to``: Pos }
    override self.ToString() = $"{self.from.line}:{self.from.column} .. {self.``to``.line}:{self.``to``.column}"
    member inline self.loc = self
    static member inline ofp from ``to`` = { from = from; ``to`` = ``to`` }
    static member inline ofv fl fc tl tc = { from = Pos.ofv fl fc; ``to`` = Pos.ofv tl tc }
    static member inline ofmp< ^a, ^b, ^c when ^a : (member loc: Loc) and ^b : (member loc: Loc) and ^c : (member loc: Loc)> (f: ^a Maybe) (t: ^b Maybe) (e: ^c)  = 
        match f with
        | Just f ->
            match t with
            | Just t -> Loc.ofp (^a : (member loc: Loc) f).from (^b : (member loc: Loc) t).``to``
            | Nil -> Loc.ofp (^a : (member loc: Loc) f).from (^c : (member loc: Loc) e).``to``
        | Nil -> 
            match t with
            | Just t -> Loc.ofp (^c : (member loc: Loc) e).from (^b : (member loc: Loc) t).``to``
            | Nil -> (^c : (member loc: Loc) e)
    static member zero = Loc.ofp Pos.zero Pos.zero
    member inline self.merge (other: Loc) = Loc.ofp self.from other.``to``
    static member inline may< ^a when ^a : (member loc: Loc)> (m: ^a Maybe) d = 
        match m with
        | Just v -> (^a : (member loc: Loc) v)
        | Nil -> d
    member inline self.mayMerge< ^a when ^a : (member loc: Loc)> (m: ^a Maybe) =
        match m with
        | Just v -> self.merge (^a : (member loc: Loc) v)
        | Nil -> self

[<Struct; IsReadOnly; DebuggerDisplay(@"CodeRange \{ {ToString(),nq} \}")>]
type CodeRange =
    { from: int; ``to``: int }
    member self.len = self.``to`` - self.from
    override self.ToString() = $"{self.from}..{self.``to``}"
    static member inline New (from, ``to``) = { from = from; ``to`` = ``to`` }

    member inline self.slice() = self
    member self.slice(start) = CodeRange.New(self.from + start, self.``to``)
    member self.slice(start, ``end``) = CodeRange.New(self.from + start, self.from + ``end``)
    member self.sliceTo(``end``) = CodeRange.New(self.from, self.from + ``end``)
    
    member self.GetSlice(start, ``end``) = 
        match start with
        | None -> 
            match ``end`` with
            | None -> self.slice()
            | Some e -> self.sliceTo(e)
        | Some s -> 
            match ``end`` with
            | None -> self.slice(s)
            | Some e -> self.slice(s, e)

    static member inline ofv from ``to`` = { from = from; ``to`` = ``to`` }
