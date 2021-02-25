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
        let v = self.Val.TryToStrMap(" ")
        $"var {self.Pat}{self.Type.TryToStr}{v};"

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
        let e = self.Else.TryToStrMap(" ")
        $"do {self.Expr}{e}{self.Split.TryToStr}"

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
      Expr: PExpr
      Split: TSplit }

    override self.ToString() =
        let d = self.TDo.TryToStrMap(" ")
        $"else{d} {self.Expr};"

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

type PType =
    | Id of TId

    override self.ToString() =
        match self with
        | Id i -> i.ToString()

////////////////////////////////////////////////////////////////////////////////////////////////////

type PExpr =
    | Id of TId
    | If of PIf
    | Case of PCase

    override self.ToString() =
        match self with
        | Id i -> string i
        | If i -> string i
        | Case i -> string i

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

////////////////////////////////////////////////////////////////////////////////////////////////////

type PItem = Split of TSplit

////////////////////////////////////////////////////////////////////////////////////////////////////

type PPat = Id of TId
