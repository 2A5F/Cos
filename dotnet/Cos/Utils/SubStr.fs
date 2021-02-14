namespace Volight.Cos.Utils

open System.Runtime.CompilerServices
open System
open Volight.Cos.Utils.SlicedEx
open Volight.Cos.SrcPos

[<Struct; IsReadOnly; CustomEquality; NoComparison>]
type SubStr = 
    val raw: string 
    val range: CodeRange
    new (raw, range) = { raw = raw; range = range }
    member self.Len = self.range.Len
    override self.ToString() = self.raw.AsSliced().ByCodeRange(self.range).ToStr
    override self.GetHashCode() =
        let hash = HashCode()
        for c in self.raw.AsSliced().ByCodeRange(self.range) do
            hash.Add c
        hash.ToHashCode()
    member self.Equals(other: SubStr) =
        if self.range.Len <> other.range.Len then false
        else if self.range.Len = 0 then true
        else 
        let this = self
        let len = self.range.Len
        let rec find i =
            if i >= len then true
            else if this.raw.[i + this.range.From] <> other.raw.[i + other.range.From] then false
            else find (i + 1)
        find 0
    override self.Equals(other: obj) = match other with :? SubStr as o -> self.Equals o | _ -> false

    interface IEquatable<SubStr> with
        member self.Equals other = self.Equals other

module SubStrEx =
    type String with
        member inline self.ToSubStr = SubStr(self, CodeRange.New(0, self.Length))
