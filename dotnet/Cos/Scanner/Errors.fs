namespace rec Volight.Cos.Parser

open Volight.Cos.SrcPos

exception ScannerException of ScannerError

type ScannerError =
    | MultipleErrors of ScannerError []
    | UnknownSymbol of Pos
    | UnexpectedEof of Pos
    | IllegalNumber of Loc
    | BlockNotClosed of Pos * char
    | CharNotClosed of Pos
    | StringNotClosed of Pos
    | UnknownEscape of Pos
    | IllegalEscape of Loc * IllegalEscape

type IllegalEscape =
    | Digit
    | Hex
    | Unicode
