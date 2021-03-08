namespace rec Volight.Cos.Parser

open System.Collections.Generic
open Volight.Cos.SrcPos

exception ParserException of ParserError

type ParserError =
| MultipleErrors of ParserError List
| InternalError of string
| UnexpectedBlockEnd of Loc
| UnknownOperator of TOper
| IllegalAloneOperator of TOper
| IllegalEdgeOperator of TOper