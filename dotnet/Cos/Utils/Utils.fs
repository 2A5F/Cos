module internal Volight.Cos.Utils.Utils

open System
open System.Collections.Generic

let maybe = MaybeBuilder()

type Dictionary<'K, 'V> with
    member self.TryGet k =
        let mutable v = Unchecked.defaultof<'V>
        if self.TryGetValue(k, &v) then Just v
        else Nil

let inline todo () = raise <| NotImplementedException("todo")
let inline todoBy msg = raise <| NotImplementedException($"todo {msg}")
