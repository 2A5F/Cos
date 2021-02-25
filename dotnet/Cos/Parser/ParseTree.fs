namespace rec Volight.Cos.Parser

open Volight.Cos.Utils
open Volight.Cos.Utils.Utils
open Volight.Cos.SrcPos

////////////////////////////////////////////////////////////////////////////////////////////////////

type PVar =
    { TVar: TId
      Pat: PPat
      Type: PTypeAnno Maybe
      Val: PVarVal Maybe
      Split: TSplit }

    override self.ToString() =
        $"var {self.Pat}{self.Type.TryToStr}{self.Val.TryToStrSL};"

type PVarVal =
    { TEq: TOper
      Expr: PExpr }

    override self.ToString() = $"= {self.Expr}"

type PTypeAnno =
    { TColon: TOper
      Type: PType }

    override self.ToString() = $": {self.Type}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PLet =
    { TLet: TId
      Name: TId
      Opers: PLetOper []
      Split: TSplit }

    override self.ToString() =
        let opers = tryToStrMap self.Opers " " "" " "
        $"let {self.Name}{opers};"

type PLetOper =
    { Oper: TOper
      Expr: PExpr }

    override self.ToString() = $"{self.Oper} {self.Expr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PIf =
    { Label: PLabelDef Maybe
      TIf: TId

      Cond: PExpr
      Body: PIfBody }

    override self.ToString() =
        $"{self.Label.TryToStr}if {self.Cond} {self.Body}"

type PIfBody =
    | Expr of PIfThenExpr
    | Block of PBlock
    | Else of PElseBody

    override self.ToString() =
        match self with
        | Expr e -> string e
        | Block e -> string e
        | Else e -> string e

type PIfThenExpr =
    { TDo: TId
      Expr: PExpr
      Else: PElseBody Maybe
      Split: TSplit Maybe }

    override self.ToString() =
        $"do {self.Expr}{self.Else.TryToStrSL}{self.Split.TryToStr}"

type PIfThenBlock =
    { Block: PBlock
      Else: PElseBody Maybe }

    override self.ToString() = $"{self.Block}{self.Else.TryToStrSL}"

type PElseBody =
    | Expr of PElseThenExpr
    | Block of PElseThenBlock

    override self.ToString() =
        match self with
        | Expr e -> string e
        | Block e -> string e

type PElseThenExpr =
    { TElse: TId
      TDo: TId Maybe
      Expr: PExpr
      Split: TSplit }

    override self.ToString() =
        $"else{self.TDo.TryToStrSL} {self.Expr};"

type PElseThenBlock =
    { TElse: TId
      Block: PBlock }

    override self.ToString() = $"else {self.Block}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PCase =
    { TCase: TId
      Expr: PExpr
      Body: PCaseBody }

    override self.ToString() = $"case {self.Expr} {self.Body}"

type PCaseBody =
    | Flat of PCaseFlat
    | Block of PCaseBlock

type PCaseFlat =
    { Split: TSplit
      Items: PCaseItem [] }

    override self.ToString() =
        let v = tryToStrMap self.Items " " " " " "
        $";{v}"

type PCaseBlock =
    { Brackets: struct (Loc * Loc)
      Items: PCaseItem [] }

    override self.ToString() =
        let v = tryToStrMap self.Items " " " " " "
        $"{{{v}}}"

type PCaseItem =
    | Of of PCaseOf
    | Else of PElseBody

type PCaseOf =
    { TOf: TId
      Pat: PPat
      Body: PCaseOfBody }

    override self.ToString() = $"of {self.Pat}"

type PCaseOfBody =
    | Expr of PCaseOfThenExpr
    | Block of PCaseOfThenBlock

type PCaseOfThenExpr =
    { TDo: TId
      Expr: PExpr
      Split: TSplit }

    override self.ToString() = $"do {self.Expr};"

type PCaseOfThenBlock =
    { Block: PBlock }

    override self.ToString() = $"{self.Block}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PWhile =
    { Label: PLabelDef Maybe
      TWhile: TId

      TDo: TId Maybe
      Cond: PExpr
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $"{self.Label.TryToStr}while{self.TDo.TryToStrSL} {self.Cond} {self.Block}{self.With.TryToStrSL}"

type PFor =
    { Label: PLabelDef Maybe
      TFor: TId

      Pat: PPat
      TIn: TId
      Iter: PExpr
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $"{self.Label.TryToStr}for {self.Pat} in {self.Iter} {self.Block}{self.With.TryToStrSL}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PBreak =
    { TBreak: TId
      Label: PLabelUse Maybe
      Expr: PExpr Maybe }

    override self.ToString() =
        $"break{self.Label.TryToStr}{self.Expr.TryToStrSL}"

type PContinue =
    { TContinue: TId
      Label: PLabelUse Maybe }

    override self.ToString() = $"continue{self.Label.TryToStr}"

type PReturn =
    { TReturn: TId
      Label: PLabelUse Maybe
      Expr: PExpr Maybe }

    override self.ToString() =
        $"return{self.Label.TryToStr}{self.Expr.TryToStrSL}"

type PGoto =
    { TGoto: TId
      Label: PLabelUse Maybe }

    override self.ToString() = $"goto{self.Label.TryToStr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PThrow =
    { TThrow: TId
      Expr: PExpr }

    override self.ToString() = $"throw {self.Expr}"

type PTry =
    { TTry: TId
      Expr: PExpr }

    override self.ToString() = $"try {self.Expr}"

type PCatch =
    { TCatch: TId
      Pat: PPat Maybe
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $"catch{self.Pat.TryToStrSL} {self.Block}{self.With.TryToStrSL}"

type PFinally =
    { TFinally: TId
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $"finally {self.Block}{self.With.TryToStrSL}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PBlock =
    { Brackets: struct (Loc * Loc)
      Items: PItem [] }

    override self.ToString() =
        let b = tryToStrMap self.Items " " " " " "
        $"{{{b}}}"

type PLabelDef =
    { Name: TId
      At: TAt }

    override self.ToString() = $"{self.Name}@"

type PLabelUse =
    { At: TAt
      Name: TId }

    override self.ToString() = $"@{self.Name}"

type PCodeBlock =
    { Label: PLabelDef Maybe
      Colon: TOper
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $"{self.Label.TryToStr}:{self.Block}{self.With.TryToStr}"

type PItemLabel =
    { Label: PLabelDef
      Split: TSplit }

    override self.ToString() = $"{self.Label};"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PWith =
    { TWith: TId
      Target: PWithTarget }

    override self.ToString() = $"with{self.Target}"

type PWtihBlock =
    { Label: PLabelDef Maybe
      Block: PBlock }

    override self.ToString() = $"{self.Label.TryToStrSR}{self.Block}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PFn =
    { Label: PLabelDef Maybe
      TFn: TId

      Name: TId
      Params: PParams
      Ret: PRetType Maybe
      Affix: PFnAffix []
      Body: PBlock }

    override self.ToString() =
        let a = tryToStrMap self.Affix " " "" " "
        $"{self.Label.TryToStr}fn {self.Name}{self.Params}{self.Ret.TryToStrSL}{a} {self.Body}"

type PRetType =
    { TArrow: TOper
      Type: PType }

    override self.ToString() = $"-> {self.Type}"

type PParams =
    { Brackets: struct (Loc * Loc)
      Items: PParamItem [] }

    override self.ToString() =
        let v = tryToStrMap self.Items "" "" " "
        $"({v})"

type PParamItem =
    { Name: TId
      Type: PTypeAnno Maybe
      Val: PVarVal Maybe
      Comma: TComma Maybe }

    override self.ToString() =
        $"{self.Name}{self.Type.TryToStr}{self.Val.TryToStrSL}{self.Comma.TryToStr}"

type PFnAffix =
    | Co of TId
    | Inline of TId
    | Throws of TId
    | Tail of TId

////////////////////////////////////////////////////////////////////////////////////////////////////

type PCall =
    { Target: PExpr
      Args: PArgs }

    override self.ToString() = $"{self.Target}{self.Args}"

type PArgs =
    { Brackets: struct (Loc * Loc)
      Items: PArgItem [] }

    override self.ToString() =
        let v = tryToStrMap self.Items "" "" " "
        $"({v})"

type PArgItem =
    { Expr: PExpr
      Comma: TComma Maybe }

    override self.ToString() = $"{self.Expr}{self.Comma.TryToStr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeFn =
    { TFn: TId
      Params: PTypeTuple
      Ret: PRetType Maybe }

    override self.ToString() =
        $"fn {self.Params}{self.Ret.TryToStrSL}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeTuple =
    { Brackets: struct (Loc * Loc)
      Items: PTypeTupleItem [] }

    override self.ToString() =
        let v = tryToStrMap self.Items "" "" " "
        $"({v})"

type PTypeTupleItem =
    { Name: PTypeTupleItemName Maybe
      Type: PType
      Comma: TComma Maybe }

    override self.ToString() = $"{self.Name.TryToStrSR}{self.Type}"

type PTypeTupleItemName =
    { Name: TId
      TColon: TOper }

    override self.ToString() = $"{self.Name}:"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTailFn =
    { Target: PExpr
      Label: PLabelDef Maybe
      Block: PBlock }

    override self.ToString() =
        $"{self.Target} {self.Label.TryToStr}{self.Block}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PExpr =
    | Id of TId
    | If of PIf
    | Case of PCase
    | While of PWhile
    | For of PFor
    | Block of PCodeBlock
    | Break of PBreak
    | Continue of PContinue
    | Return of PReturn
    | Goto of PGoto
    | Throw of PThrow
    | Try of PTry
    | Call of PCall
    | TailFn of PTailFn

    override self.ToString() =
        match self with
        | Id i -> string i
        | If i -> string i
        | Case i -> string i
        | While i -> string i
        | For i -> string i
        | Block i -> string i
        | Break i -> string i
        | Continue i -> string i
        | Return i -> string i
        | Goto i -> string i
        | Throw i -> string i
        | Try i -> string i
        | Call i -> string i
        | TailFn i -> string i

////////////////////////////////////////////////////////////////////////////////////////////////////

type PItem =
    | Split of TSplit
    | If of PIf
    | Case of PCase
    | While of PWhile
    | For of PFor
    | Block of PCodeBlock
    | Expr of PExprItem
    | Ret of PExpr
    | Label of PItemLabel
    | Catch of PCatch
    | Finally of PFinally
    | Fn of PFn

    override self.ToString() =
        match self with
        | Split i -> string i
        | If i -> string i
        | Case i -> string i
        | While i -> string i
        | For i -> string i
        | Block i -> string i
        | Expr i -> string i
        | Ret i -> string i
        | Label i -> string i
        | Catch i -> string i
        | Finally i -> string i
        | Fn i -> string i

type PExprItem =
    { Expr: PExpr
      Split: TSplit }

    override self.ToString() = $"{self.Expr};"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PWithTarget =
    | Block of PWtihBlock
    | If of PIf
    | Case of PCase
    | While of PWhile
    | For of PFor
    | CodeBlock of PCodeBlock
    | Catch of PCatch
    | Finally of PFinally

    override self.ToString() =
        match self with
        | If i -> string i
        | Case i -> string i
        | While i -> string i
        | For i -> string i
        | Block i -> string i
        | Catch i -> string i
        | Finally i -> string i
        | CodeBlock i -> string i

////////////////////////////////////////////////////////////////////////////////////////////////////

type PPat =
    | Id of TId

    override self.ToString() =
        match self with
        | Id i -> string i

////////////////////////////////////////////////////////////////////////////////////////////////////

type PType =
    | Id of TId
    | Fn of PTypeFn

    override self.ToString() =
        match self with
        | Id i -> string i
        | Fn i -> string i
