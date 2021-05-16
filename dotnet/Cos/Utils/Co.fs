namespace Volight.Cos.Utils

type 'T Co = (('T -> unit) -> unit)

type CoBuilder() =
    member inline _.Bind(m, f) = fun c -> m <| fun a -> (f a) c
    member inline _.Return(v) = fun c -> c v
    member inline _.ReturnFrom(v) = v
    member inline _.Zero() = fun c -> c ()

