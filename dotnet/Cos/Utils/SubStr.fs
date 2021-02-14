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
    member self.len = self.range.len
    override self.ToString() = self.raw.AsSliced().ByCodeRange(self.range).toStr
    override self.GetHashCode() =
        let hash = HashCode()
        for c in self.raw.AsSliced().ByCodeRange(self.range) do
            hash.Add c
        hash.ToHashCode()
    member self.Equals(other: SubStr) =
        if self.range.len <> other.range.len then false
        else if self.range.len = 0 then true
        else 
        let self = self
        let len = self.range.len
        let rec find i =
            if i >= len then true
            else if self.raw.[i + self.range.from] <> other.raw.[i + other.range.from] then false
            else find (i + 1)
        find 0
    override self.Equals(other: obj) = match other with :? SubStr as o -> self.Equals o | _ -> false

    interface IEquatable<SubStr> with
        member self.Equals other = self.Equals other

module SubStrEx =
    type String with
        member inline self.ToSubStr = SubStr(self, CodeRange.New(0, self.Length))
