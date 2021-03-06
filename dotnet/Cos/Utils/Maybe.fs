namespace Volight.Cos.Utils

open System.Runtime.CompilerServices

[<Struct; IsReadOnly>]
type Maybe<'T> =
    | Nil
    | Just of 'T

    member self.TryToStr =
        match self with
        | Nil -> ""
        | Just v -> v.ToString()

    member self.TryToStrMap(p) =
        match self with
        | Nil -> ""
        | Just v -> $"{p}{v.ToString()}"

    member self.TryToStrMap(p, s) =
        match self with
        | Nil -> ""
        | Just v -> $"{p}{v.ToString()}{s}"

    member self.TryToStrSL =
        match self with
        | Nil -> ""
        | Just v -> $" {v.ToString()}"

    member self.TryToStrSR =
        match self with
        | Nil -> ""
        | Just v -> $"{v.ToString()} "

    member self.TryToStrSLR =
        match self with
        | Nil -> ""
        | Just v -> $" {v.ToString()} "

module Maybe =
    let inline nullable(v: 'T) = if isNull v then Nil else Just v

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
        | Nil -> f ()

    member inline _.Delay f = f
    member inline _.Run f = f ()
