module internal Volight.Cos.Utils.Utils

open System
open System.Text
open System.Collections.Generic

let inline isSameObject a b = LanguagePrimitives.PhysicalEquality a b
let inline notSameObject a b = not <| LanguagePrimitives.PhysicalEquality a b
let inline notNull v = not <| isNull v

let maybe = MaybeBuilder()

type Dictionary<'K, 'V> with
    member self.TryGet k =
        let mutable v = Unchecked.defaultof<'V>
        if self.TryGetValue(k, &v) then Just v
        else Nil

let inline todo () = raise <| NotImplementedException("todo")
let inline todoBy msg = raise <| NotImplementedException($"todo {msg}")

let tryToStr a =
    let sb = StringBuilder()
    for i in a do
        sb.Append(string i) |> ignore
    if sb.Length = 0 then "" else
    string sb

let tryToStrMap a (p: string) (s: string) (c: string) =
    let sb = StringBuilder()
    let mutable first = true
    for i in a do
        if first then first <- true else  sb.Append(c) |> ignore
        sb.Append(string i) |> ignore
    if sb.Length = 0 then "" else
    $"{p}{string sb}{s}"

let inline llnTryValue (n: 'a LinkedListNode) = if isNull n then Nil else Just n.Value

let inline (|NotNull|_|) v = if isNull v then None else Some v

let inline appF (list: ('a -> 'a) list) a = 
    let mutable v = a
    for i in list do v <- i v
    v

let inline id<'T> (v: 'T) = Operators.id v
