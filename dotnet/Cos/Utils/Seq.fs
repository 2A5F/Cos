module internal Volight.Cos.Utils.SeqEx

open System.Collections.Generic

module Internal =
    let rec tryItem index (e : IEnumerator<'T>) =
        if not (e.MoveNext()) then Nil
        elif index = 0 then Just(e.Current)
        else tryItem (index-1) e

[<CompiledName("TryItem")>]
let tryItem index (source : seq<'T>) =
    if index < 0 then Nil else
    use e = source.GetEnumerator()
    Internal.tryItem index e
