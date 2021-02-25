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
    { TIf: TId
      Label: PLabel Maybe
      Cond: PExpr
      Body: PIfBody }

    override self.ToString() =
        $"if{self.Label.TryToStr} {self.Cond} {self.Body}"

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
    { TWhile: TId
      Label: PLabel Maybe
      TDo: TId Maybe
      Cond: PExpr
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $"while{self.Label.TryToStr}{self.TDo.TryToStrSL} {self.Cond} {self.Block}{self.With.TryToStrSL}"

type PFor =
    { TFor: TId
      Label: PLabel Maybe
      Pat: PPat
      TIn: TId
      Iter: PExpr
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $"for{self.Label.TryToStr} {self.Pat} in {self.Iter} {self.Block}{self.With.TryToStrSL}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PBreak =
    { TBreak: TId
      Label: PLabel Maybe
      Expr: PExpr Maybe }

    override self.ToString() =
        $"break{self.Label.TryToStr}{self.Expr.TryToStrSL}"

type PContinue =
    { TContinue: TId
      Label: PLabel Maybe }

    override self.ToString() = $"continue{self.Label.TryToStr}"

type PReturn =
    { TReturn: TId
      Label: PLabel Maybe
      Expr: PExpr Maybe }

    override self.ToString() =
        $"return{self.Label.TryToStr}{self.Expr.TryToStrSL}"

type PGoto =
    { TGoto: TId
      Label: PLabel Maybe }

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

type PType =
    | Id of TId

    override self.ToString() =
        match self with
        | Id i -> i.ToString()

////////////////////////////////////////////////////////////////////////////////////////////////////

type PBlock =
    { Brackets: struct (Loc * Loc)
      Items: PItem [] }

    override self.ToString() =
        let b = tryToStrMap self.Items " " " " " "
        $"{{{b}}}"

type PLabel =
    { At: TAt
      Name: TId }

    override self.ToString() = $"@{self.Name}"

type PCodeBlock =
    { Colon: TOper
      Label: PLabel Maybe
      Block: PBlock
      With: PWith Maybe }

    override self.ToString() =
        $":{self.Label.TryToStr}{self.Block}{self.With.TryToStr}"

type PItemLabel =
    { Colon: TOper
      Label: PLabel
      Split: TSplit }

    override self.ToString() = $":{self.Label};"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PWith =
    { TWith: TId
      Target: PWithTarget }

    override self.ToString() = $"with{self.Target}"

type PWtihBlock =
    { Label: PLabel Maybe
      Block: PBlock }

    override self.ToString() = $"{self.Label.TryToStrSR}{self.Block}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PFn =
    { TFn: TId
      Label: PLabel Maybe
      Name: TId
      Params: PParams
      Type: PRetType Maybe
      Affix: PFnAffix []
      Body: PBlock }

    override self.ToString() =
        let a = tryToStrMap self.Affix " " "" " "
        $"fn{self.Label.TryToStr} {self.Name}{self.Params}{self.Type.TryToStrSL}{a} {self.Body}"

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
    { Pat: PPat
      Type: PTypeAnno Maybe
      Val: PVarVal Maybe
      Comma: TComma Maybe }

    override self.ToString() =
        $"{self.Pat}{self.Type.TryToStr}{self.Val.TryToStrSL}{self.Comma.TryToStr}"

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
