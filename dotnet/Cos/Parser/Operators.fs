namespace Volight.Cos.Parser

open System.Linq
open System.Runtime.CompilerServices

[<Struct; IsReadOnly>]
type OperAssoc = Left | Right

[<Struct; IsReadOnly>]
type OperInfo =
    { Name: string; Level: int; Assoc: OperAssoc }

module Operators =
    let inline operInfo name level assoc = { Name = name; Level = level; Assoc = assoc }
    let inline defaultInfo s = operInfo s 10000 Left
    let canAlone (o: TOper) =
        let str = o.Str
        if str.Length <> 1 then true else
        match str with "!" | ":" | "." | "=" -> false | _ -> true
    let midInfos = [|
        operInfo "??" 0000 Left     // null coalescing
        operInfo "?:" 0000 Left     // optional continuation
        operInfo "&" 2100 Left      // and
        operInfo "|" 2000 Left      // or
        operInfo "!&" 2100 Left     // not and
        operInfo "!|" 2000 Left     // not or
        operInfo "^^" 3100 Right    // bit xor
        operInfo "&|" 3100 Right    // bit xor
        operInfo "&&" 3200 Left     // bit and
        operInfo "||" 3000 Left     // bit or
        operInfo "!^^" 3100 Right   // not bit xor
        operInfo "!&|" 3100 Right   // not bit xor
        operInfo "!&&" 3200 Left    // not bit and
        operInfo "!||" 3000 Left    // not bit or
        operInfo "==" 4000 Left     // eq
        operInfo "!=" 4000 Left     // ne
        operInfo "<>" 4000 Left     // ne
        operInfo "<" 5000 Left      // lt
        operInfo ">" 5000 Left      // gt
        operInfo "!<" 5000 Left     // nl ge
        operInfo "!>" 5000 Left     // ng le
        operInfo "<=" 5000 Left     // le
        operInfo ">=" 5000 Left     // ge
        operInfo "<<" 6000 Left     // bit shift left
        operInfo ">>" 6000 Left     // bit shift right
        operInfo ">>>" 6000 Left    // bit shift right full
        operInfo ".." 6500 Left     // pow
        operInfo "..=" 6500 Left    // pow
        operInfo "+" 7000 Left      // add
        operInfo "-" 7000 Left      // sub
        operInfo "*" 8000 Left      // mul
        operInfo "/" 8000 Left      // div
        operInfo "%" 8000 Left      // mod
        operInfo "^" 9000 Right     // pow
    |]
    let midInfoMap = midInfos.ToDictionary(fun i -> i.Name)
    ()
