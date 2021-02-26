namespace rec Volight.Cos.Parser

open System.Collections.Generic
open Volight.Cos.SrcPos

exception ParserException of ParserError

type ParserError =
| MultipleErrors of ParserError List
| UnexpectedBlockEnd of Loc

