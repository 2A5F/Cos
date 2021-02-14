namespace Volight.Cos.Utils

open System.Runtime.CompilerServices

[<Struct; IsReadOnly;>]
type Maybe<'T> =
| Nil
| Just of 'T

type MaybeBuilder() =
    member inline _.Bind(m, f) =
        match m with
        | Just v -> f v
        | Nil -> Nil

    member inline _.Zero() = Nil
    member inline _.Yield(v) = Just v
    member inline _.Return v = v
    
    member inline _.Combine(m, f) =
        match m with
        | Just v -> Just v
        | Nil -> f()

    member inline _.Delay f = f
    member inline _.Run f = f()
