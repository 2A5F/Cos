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
      Body: PFnBody Maybe
      Split: TSplit Maybe }

    override self.ToString() =
        let a = tryToStrMap self.Affix " " "" " "

        let part1 =
            $"{self.Label.TryToStr}fn {self.Name}{self.Params}{self.Ret.TryToStrSL}"

        let part2 =
            $"{a}{self.Body.TryToStrSL}{self.Split.TryToStr}"

        $"{part1}{part2}"

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

type PFnBody =
    | Expr of PFnBodyExpr
    | Block of PBlock

type PFnBodyExpr =
    { TDo: TId
      Expr: PExpr }

    override self.ToString() = $"do {self.Expr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PExprFn =
    { Label: PLabelDef Maybe
      TFn: TId
      Name: TId Maybe
      Params: PParams
      Ret: PRetType Maybe
      Affix: PFnAffix []
      Body: PFnBody }

    override self.ToString() =
        let a = tryToStrMap self.Affix " " "" " "
        $"{self.Label.TryToStr}fn {self.Name.TryToStr}{self.Params}{self.Ret.TryToStrSL}{a} {self.Body}"

type PExprBlockFn =
    { Label: PLabelDef Maybe
      TFn: TId
      Body: PBlockFnBody }

    override self.ToString() = $"{self.Label.TryToStr}fn {self.Body}"

type PBlockFnBody =
    { Brackets: struct (Loc * Loc)
      Sig: PBlockFnBodySig Maybe
      Items: PItem [] }

    override self.ToString() =
        let b = tryToStrMap self.Items " " " " " "
        $"{{{self.Sig.TryToStrSL}{b}}}"

type PBlockFnBodySig =
    { Name: TId Maybe
      Params: PParams
      Ret: PRetType Maybe
      Affix: PFnAffix []
      TDo: TId }

    override self.ToString() =
        let a = tryToStrMap self.Affix " " "" " "
        $"{self.Name.TryToStrSR}{self.Params}{self.Ret.TryToStrSL}{a} do"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTailFn =
    { Target: PExpr
      Dot: TOper Maybe
      Label: PLabelDef Maybe
      Block: PBlockFnBody }

    override self.ToString() =
        let dot =
            match self.Dot with
            | Just i -> string i
            | _ -> " "

        $"{self.Target}{dot}{self.Label.TryToStr}{self.Block}"

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

type PBool =
    | True of TId
    | False of TId

    override self.ToString() =
        match self with
        | True _ -> "true"
        | False _ -> "false"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PStr =
    { Left: Loc
      Right: Loc
      Items: PStrPart [] }

    override self.ToString() =
        let i = tryToStr self.Items
        $"\"{i}\""

type PStrPart =
    | Str of SubStr
    | Escape of TStrEscape
    | Block of PStrBlock

    override self.ToString() =
        match self with
        | Str s -> string s
        | Escape s -> string s.Raw
        | Block b -> string b

type PStrBlock =
    { TDollar: Loc
      Block: PBlock }

    override self.ToString() = $"${self.Block}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeStr =
    { Left: Loc
      Right: Loc
      Items: PStrPart [] }

    override self.ToString() =
        let i = tryToStr self.Items
        $"\"{i}\""

type PTypeStrPart =
    | Str of SubStr
    | Escape of TStrEscape
    | Block of PStrBlock

    override self.ToString() =
        match self with
        | Str s -> string s
        | Escape s -> string s.Raw
        | Block b -> string b

type PTypeStrBlock =
    { TDollar: Loc
      Brackets: struct (Loc * Loc)
      Type: PType }

    override self.ToString() = $"${{ {self.Type} }}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeObj =
    { Brackets: struct (Loc * Loc)
      Items: PTypeObjItem [] }

    override self.ToString() =
        let i = tryToStrMap self.Items " " " " " "
        $"{{{i}}}"

type PTypeObjItem =
    { Name: TId
      TColon: TOper
      Type: PType
      TComma: TComma Maybe }

    override self.ToString() =
        $"{self.Name}: {self.Type}{self.TComma.TryToStr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PObj =
    { Brackets: struct (Loc * Loc)
      Items: PObjItem [] }

    override self.ToString() =
        let i = tryToStrMap self.Items " " " " " "
        $"{{{i}}}"

type PObjItem =
    { Name: TId
      TEq: TOper
      Expr: PExpr
      TComma: TComma Maybe }

    override self.ToString() =
        $"{self.Name} = {self.Expr}{self.TComma.TryToStr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeArr =
    { Brackets: struct (Loc * Loc)
      Type: PType
      Len: PTypeArrLen Maybe }

    override self.ToString() = $"[{self.Type}{self.Len.TryToStr}]"

type PTypeArrLen =
    { TSplit: TSplit
      Len: TNum }

    override self.ToString() = $"; {self.Len}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PArr =
    { Brackets: struct (Loc * Loc)
      Items: PArrItem [] }

    override self.ToString() =
        let i = tryToStrMap self.Items "" "" " "
        $"[{i}]"

type PArrItem =
    { Expr: PExpr
      TComma: TComma Maybe }

    override self.ToString() = $"{self.Expr}{self.TComma.TryToStr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTuple =
    { Brackets: struct (Loc * Loc)
      Items: PTupleItem [] }

    override self.ToString() =
        let i = tryToStrMap self.Items "" "" " "
        $"({i})"

type PTupleItem =
    { Expr: PExpr
      TComma: TComma Maybe }

    override self.ToString() = $"{self.Expr}{self.TComma.TryToStr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeNever =
    { TNever: TOper }

    override _.ToString() = "!"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeRange =
    { Left: PType
      TRange: TOper
      Right: PType }

    override self.ToString() = $"{self.Left}{self.TRange}{self.Right}"

type PRange =
    { Left: PExpr
      TRange: TOper
      Right: PExpr }

    override self.ToString() = $"{self.Left}{self.TRange}{self.Right}"


type PTypeRangeTo =
    { TRange: TOper
      Right: PType }

    override self.ToString() = $"{self.TRange}{self.Right}"

type PRangeTo =
    { TRange: TOper
      Right: PExpr }

    override self.ToString() = $"{self.TRange}{self.Right}"

type PTypeRangeFrom =
    { Left: PType
      TRange: TOper }

    override self.ToString() = $"{self.Left}{self.TRange}"

type PRangeFrom =
    { Left: PExpr
      TRange: TOper }

    override self.ToString() = $"{self.Left}{self.TRange}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeIn =
    { TIn: TId
      Type: PType }

    override self.ToString() = $"in {self.Type}"

type PInOper =
    { Left: PExpr
      TIn: TId
      Right: PExpr }

    override self.ToString() = $"{self.Left} in {self.Right}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeOr =
    { Items: PTypeOrItem [] }

    override self.ToString() = tryToStrMap self.Items "" "" " "

type PTypeOrItem =
    { TOr: TOper Maybe
      Type: PType }

    override self.ToString() = $"{self.TOr.TryToStrSR}{self.Type}"

type PTypeAnd =
    { Items: PTypeAndItem [] }

    override self.ToString() = tryToStrMap self.Items "" "" " "

type PTypeAndItem =
    { TAnd: TOper Maybe
      Type: PType }

    override self.ToString() = $"{self.TAnd.TryToStrSR}{self.Type}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeNullable =
    { TQ: TOper
      Type: PType }

    override self.ToString() = $"?{self.Type}"

type PTypeOptional =
    { Type: PType
      TQ: TOper }

    override self.ToString() = $"{self.Type}?"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeGeneric =
    { Target: PType
      Generic: PGenericUse }

    override self.ToString() = $"{self.Target}{self.Generic}"

type PGeneric =
    { Target: PExpr
      Generic: PGenericUse }

    override self.ToString() = $"{self.Target}{self.Generic}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PTypeFor =
    { TFor: PFor
      Generic: PGenericUse
      Type: PType Maybe }

    override self.ToString() =
        $"for{self.Generic}{self.Type.TryToStrSL}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PDef =
    { TDef: TId
      Name: TId
      Generic: PGenericDef Maybe
      Body: PDefBody }

    override self.ToString() =
        $"def {self.Name}{self.Generic.TryToStr} {self.Body}"

type PDefBody =
    | Alias of PDefAlias
    | Data of PDefData
    | Kind of PDefKind
    | Enum of PDefEnum
    | Need of PDefNeed

    override self.ToString() =
        match self with
        | Alias i -> string i
        | Data i -> string i
        | Kind i -> string i
        | Enum i -> string i
        | Need i -> string i

type PDefAlias =
    { TEq: TOper
      Type: PType
      TSplit: TSplit }

    override self.ToString() = $"= {self.Type};"

type PDefNeed =
    { TNeed: TId
      Constraint: PTypeConstraint Maybe
      TSplit: TSplit }

    override self.ToString() = $"need{self.Constraint.TryToStrSL};"

type PDefData =
    { TData: TId
      Params: PParams Maybe
      Constraint: PTypeConstraint Maybe
      Block: PDefDataBlock }

    override self.ToString() =
        $"data{self.Params.TryToStr}{self.Constraint.TryToStrSL} {self.Block}"

type PDefDataBlock =
    { Brackets: struct (Loc * Loc)
      Items: PMember [] }

    override self.ToString() =
        let v = tryToStrMap self.Items " " " " " "
        $"{{{v}}}"

type PDefKind =
    { TKind: TId
      Constraint: PTypeConstraint Maybe
      Block: PDefDataBlock }

    override self.ToString() =
        $"kind{self.Constraint.TryToStrSL} {self.Block}"

type PDefEnum =
    { TEnum: TId
      Constraint: PTypeConstraint Maybe
      Block: PDefEnumBlock }

    override self.ToString() =
        $"enum{self.Constraint.TryToStrSL} {self.Block}"

type PDefEnumBlock =
    { Brackets: struct (Loc * Loc)
      Items: PDefEnumItems []
      Member: PDefEnumMemberPart Maybe }

    override self.ToString() =
        let v = tryToStrMap self.Items " " " " " "
        $"{{{v}{self.Member.TryToStr}}}"

type PDefEnumItems =
    { Item: PDefEnumItem
      TComma: TComma Maybe }

    override self.ToString() = $"{self.Item}{self.TComma.TryToStr}"

type PDefEnumItem =
    | Id of TId
    | Tuple of PDefEnumItemTuple
    | Obj of PDefEnumItemObj

    override self.ToString() =
        match self with
        | Id i -> string i
        | Tuple i -> string i
        | Obj i -> string i

type PDefEnumItemTuple =
    { Name: TId
      Tuple: PTypeTuple }

    override self.ToString() = $"{self.Name}{self.Tuple}"

type PDefEnumItemObj =
    { Name: TId
      Obj: PTypeObj }

    override self.ToString() = $"{self.Name} {self.Obj}"

type PDefEnumMemberPart =
    { TSplit: TSplit
      Items: PMember [] }
    override self.ToString() =
        let v = tryToStrMap self.Items " " " " " "
        $";{v}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PGenericDef =
    { Brackets: struct (Loc * Loc)
      Items: PGenericDefItem [] }

    override self.ToString() =
        let i = tryToStrMap self.Items "" "" " "
        $"[{i}]"

type PGenericDefItem =
    { Name: TId
      Constraint: PTypeConstraint Maybe
      TComma: TComma Maybe }

    override self.ToString() =
        $"{self.Name}{self.Constraint.TryToStr}{self.TComma.TryToStr}"

type PTypeConstraint =
    { TColon: TOper
      Type: PType }

    override self.ToString() = $": {self.Type}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PGenericUse =
    { Brackets: struct (Loc * Loc)
      Items: PGenericUseItem [] }
    override self.ToString() =
        let i = tryToStrMap self.Items "" "" " "
        $"[{i}]"

type PGenericUseItem =
    { Type: PType
      TComma: TComma Maybe }

    override self.ToString() = $"{self.Type}{self.TComma.TryToStr}"

////////////////////////////////////////////////////////////////////////////////////////////////////

type PType =
    | Id of TId
    | Fn of PTypeFn
    | Bool of PBool
    | Num of TNum
    | Str of PTypeStr
    | Obj of PTypeObj
    | Arr of PTypeArr
    | Tuple of PTypeTuple
    | Never of PTypeNever
    | Range of PTypeRange
    | RangeTo of PTypeRangeTo
    | RangeFrom of PTypeRangeFrom
    | In of PTypeIn
    | Or of PTypeOr
    | And of PTypeAnd
    | Nullable of PTypeNullable
    | Optional of PTypeOptional
    | Generic of PTypeGeneric

    override self.ToString() =
        match self with
        | Id i -> string i
        | Fn i -> string i
        | Bool i -> string i
        | Num i -> string i
        | Str i -> string i
        | Obj i -> string i
        | Arr i -> string i
        | Tuple i -> string i
        | Never i -> string i
        | Range i -> string i
        | RangeTo i -> string i
        | RangeFrom i -> string i
        | In i -> string i
        | Or i -> string i
        | And i -> string i
        | Nullable i -> string i
        | Optional i -> string i
        | Generic i -> string i

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
    | Fn of PExprFn
    | BlockFn of PExprBlockFn
    | TailFn of PTailFn
    | Bool of PBool
    | Num of TNum
    | Str of PStr
    | Obj of PObj
    | Arr of PArr
    | Tuple of PTuple
    | Range of PRange
    | RangeTo of PRangeTo
    | RangeFrom of PRangeFrom
    | In of PInOper
    | Generic of PGeneric

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
        | Fn i -> string i
        | BlockFn i -> string i
        | TailFn i -> string i
        | Bool i -> string i
        | Num i -> string i
        | Str i -> string i
        | Obj i -> string i
        | Arr i -> string i
        | Tuple i -> string i
        | Range i -> string i
        | RangeTo i -> string i
        | RangeFrom i -> string i
        | In i -> string i
        | Generic i -> string i

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
    | Def of PDef

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
        | Def i -> string i

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

type PMember =
    | Field of PVar
    | Method of PFn
    | Def of PDef
