import { Loc } from "../utils";

export type Token = TId | TSplit | TComma | TBlock | TOper | TAt | TDArrow | TSArrow | TNum | TChar | TStr

export type TId = Readonly<{
    t: 'id'
    /** id 字符串 */
    id: string
    /** > 0 就是关键词 */
    key: number
    loc: Loc
}>

export type TSplit = Readonly<{
    t: ';'
    loc: Loc
}>

export type TComma = Readonly<{
    t: ','
    loc: Loc
}>

export type BracketTypes = '{}' | '[]' | '()'

export type TBlock = Readonly<{
    t: 'block'
    type: BracketTypes
    left: Loc
    right: Loc
    loc: Loc
    items: Token[]
}>

/** % ! + - * / ^ | & > < . = ? : ~ */
export type TOper = {
    t: 'oper'
    oper: string
    loc: Loc
}

export type TAt = {
    t: '@'
    loc: Loc
}

export type TDArrow = {
    t: '=>'
    loc: Loc
}

export type TSArrow = {
    t: '->'
    loc: Loc
}

export type TNum = {
    t: 'num'
    num: string
    prefix: string
    suffix: string
    loc: Loc
}

export type TChar = {
    t: 'char'
    char: string
    raw: string
    loc: Loc
}

export type TStr = {
    t: 'str'
    quote: '"' | "'"
    left: Loc
    right: Loc
    items: TStrPart[]
}

export type TStrPart = string | TStrEscape | TStrBlock

export type TStrStr = {
    t: 'strstr'
    str: string
    loc: Loc
}

export type TStrEscape = {
    t: '\\'
    str: string
    raw: string
    loc: Loc
}

export type TStrBlock = {
    t: '$'
    /** $ */
    dollar: Loc
    block: TBlock
    loc: Loc
}

