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
      Else: PElseBody Maybe }

    override self.ToString() =
        let e = self.Else.TryToStrMap(" ")
        $"do {self.Expr}{e}"

type PIfThenBlock =
    { Block: PBlock
      Else: PElseBody Maybe }

    override self.ToString() =
        let e = self.Else.TryToStrMap(" ")
        $"{self.Block}{e}"

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
      Expr: PExpr }

    override self.ToString() =
        let d = self.TDo.TryToStrMap(" ")
        $"else{d} {self.Expr}"

type PElseThenBlock =
    { TElse: TId
      Block: PBlock }

    override self.ToString() = $"else {self.Block}"

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
