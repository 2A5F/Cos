export interface GetPos {
    readonly pos: Pos
}

export type Pos = { readonly line: number; readonly column: number } & GetPos

export function pos(): Pos
export function pos(value: number): Pos
export function pos(line: number, column: number): Pos
export function pos(line: number = 0, column: number = line): Pos {
    return { line, column, get pos() { return this } }
}

export function isPos(v: unknown): v is Pos {
    return typeof v === 'object' && v !== null && 'line' in v && 'column' in v && typeof (v as any)['line'] === 'number' && typeof (v as any)['column'] === 'number'
}

export interface GetLoc {
    readonly loc: Loc
}

export type Loc = { readonly from: Pos; readonly to: Pos } & GetLoc

export function loc(): Loc
export function loc(value: number): Loc
export function loc(line: number, column: number): Loc
export function loc(lineFrom: number, columnFrom: number, lineTo: number, columnTo: number): Loc
export function loc(lineFrom: number = 0, columnFrom: number = lineFrom, lineTo: number = lineFrom, columnTo: number = columnFrom): Loc {
    return { from: pos(lineFrom, columnFrom), to: pos(lineTo, columnTo), get loc() { return this } }
}

export function locp(value: Pos): Loc
export function locp(from: Pos, to: Pos): Loc
export function locp(from: Pos = pos(), to: Pos = from): Loc {
    return { from, to, get loc() { return this } }
}

export function isLoc(v: unknown): v is Loc {
    return typeof v === 'object' && v !== null && 'from' in v && 'to' in v && isPos((v as any)['from']) && isPos((v as any)['to'])
}
