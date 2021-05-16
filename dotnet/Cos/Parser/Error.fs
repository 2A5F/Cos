namespace rec Volight.Cos.Parser

open System.Collections.Generic
open Volight.Cos.SrcPos

exception ParserException of ParserError

type ParserError =
| MultipleErrors of ParserError List
| UnexpectedToken of Tokens
| InternalError of string
| UnexpectedBlockEnd of Loc
| UnknownOperator of TOper
| IllegalAloneOperator of TOper
| IllegalEdgeOperator of TOper
| IllegalFloatingNumber of TNum * TOper * TNum
| LetNeedPat of TId
| LetEqNeedExpr of TId * TOper