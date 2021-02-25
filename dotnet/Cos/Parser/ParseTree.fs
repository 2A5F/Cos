namespace rec Volight.Cos.Parser

open Volight.Cos.Utils
open Volight.Cos.Utils.Utils
open Volight.Cos.SrcPos

////////////////////////////////////////////////////////////////////////////////////////////////////

type PVar =
    { TVar: TId
      Name: TId
      Type: PTypeAnno Maybe
      Val: PVarVal Maybe
      Split: TSplit }

    override self.ToString() =
        let v = self.Val.TryToStrMap(" ")
        $"var {self.Name}{self.Type.TryToStr}{v};"

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
      Cond: PExpr
      Body: PIfBody }
    override self.ToString() = "if {self.Cond}"

type PIfBody =
    | Expr of PIfThenExpr
    | Block of PIfThenBlock

type PIfThenExpr =
    { TDo: TId
      Label: PLabel Maybe
      Expr: PExpr }
    override self.ToString() = $"do{self.Label.TryToStr} {self.Expr}"

type PIfThenBlock =
    { TDo: TId
      Label: PLabel Maybe
      Block: PBlock }
    override self.ToString() = $"do{self.Label.TryToStr} {self.Block}"

type PIfElseExpr =
    { TElse: TId
      Label: PLabel Maybe
      Expr: PExpr }
    override self.ToString() = $"else do{self.Label.TryToStr} {self.Expr}"

type PIfElseBlock =
    { TElse: TId
      Label: PLabel Maybe
      Block: PBlock }
    override self.ToString() = $"else{self.Label.TryToStr} {self.Block}"


////////////////////////////////////////////////////////////////////////////////////////////////////

type PType =
    | Id of TId

    override self.ToString() =
        match self with
        | Id i -> i.ToString()

////////////////////////////////////////////////////////////////////////////////////////////////////

type PExpr =
    | Id of TId

    override self.ToString() =
        match self with
        | Id i -> i.ToString()

////////////////////////////////////////////////////////////////////////////////////////////////////

type PBlock =
    { Brackets: struct (Loc * Loc * BracketsType)
      Items: PItem [] }

    override self.ToString() = "todo"

type PLabel =
    { At: TAt
      Name: TId }

    override self.ToString() = $"@{self.Name}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PItem = Split of TSplit
