import { isJust, isNil, just, Loc, Maybe, nil, Pos, use, Voidable, AArray, AFlake, AssociatedAArray, locp, Flake } from "../utils";
import { ScannerErrorKind } from "./error";
import { TId, Token } from "./Token";

const scanRet = Symbol()
export async function* scan([arr, pos]: AssociatedAArray<string, Pos>) {
    let res: Voidable<(token: Token) => Promise<void>>
    const ctx: Ctx = {
        arr, pos,
        errs: [],
        yield(token) {
            return new Promise<void>(async back => {
                if (res == null) await new Promise<void>(res => queueMicrotask(res))
                if (res == null) throw 'cos scanner internal error'
                await res(token)
                back()
            })
        },
        loc(code: Flake<any>): Loc {
            return locp(pos[code.off], pos[code.off + code.len])
        },
    }
    const code = new AFlake(arr)
    const p = new Promise<typeof scanRet>(async res => { await root(ctx, code); res(scanRet) })
    for (; ;) {
        let back: Voidable<() => void>
        const y = new Promise<Token>(r => {
            res = (token) => {
                res = void 0
                r(token)
                return new Promise<void>(r => { back = r })
            }
        })
        const r = await Promise.race([p, y])
        if (r === scanRet) return
        yield r
        back?.()
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

type Code = AFlake<string>

type Ctx = {
    readonly arr: AArray<string>
    readonly pos: Pos[]
    readonly errs: ScannerErrorKind[]
    yield(token: Token): Promise<void>
    loc(code: Flake<any>): Loc
}

////////////////////////////////////////////////////////////////////////////////////////////////////

async function root(ctx: Ctx, code: Code) {
    if (code.isEmpty) return
    const r = await blockItem(ctx, code)

}

////////////////////////////////////////////////////////////////////////////////////////////////////

async function blockItem(ctx: Ctx, code: Code): Promise<Maybe<Code>> {
    const first = await code.first
    if (isNil(first)) return nil
    let r: Maybe<Code>
    if (r = await space(ctx, code.tail), isJust(r)) return r
    if (r = await id(ctx, code), isJust(r)) return r
    return nil
}

////////////////////////////////////////////////////////////////////////////////////////////////////

const _isSpace = /\s/u
function isSpace(c: string) {
    return _isSpace.test(c)
}

async function space(_ctx: Ctx, code: Code): Promise<Maybe<Code>> {
    if (!use(await code.first, f => isJust(f) && isSpace(f.val))) return nil
    for (let i = 0; ; i++) {
        const e = await code[i]
        if (isJust(e) && isSpace(e.val)) continue
        return just(code.slice(i))
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

const word = /\p{L}/u
const digit = /\d/u

function isIdFirst(c: string) {
    if (c === '_' || c === '$' || word.test(c)) return true
    return false
}

function isIdBody(c: string) {
    if (isIdBody(c) || digit.test(c)) return true
    return false
}

async function id(ctx: Ctx, code: Code): Promise<Maybe<Code>> {
    if (!use(await code.first, f => isJust(f) && isIdFirst(f.val))) return nil
    for (let i = 1; ; i++) {
        const e = await code[i]
        if (isJust(e) && isIdBody(e.val)) continue
        const str = code.sliceTo(i).toString()
        let loc = ctx.loc(code.sliceToFlake(i))
        // todo key
        let t: TId = { t: 'id', id: str, key: 0, loc }
        await ctx.yield(t)
        return just(code.slice(i))
    }
}
