import { Box } from "./ref"

export type Nullale<T> = T | null
export type Voidable<T> = T | undefined
export type Option<T> = T | null | undefined

export type MaybeMethod<T> = {
    isJust: boolean
    isNil: boolean
    unwrap: T
}
export type Just<T> = Box<T> & MaybeMethod<T>
export type Nil = { nil: true } & MaybeMethod<any>
export type Maybe<T> = (Just<T> | { nil: true }) & MaybeMethod<T>
export const nil: Nil = {
    nil: true,
    get isJust() { return false },
    get isNil() { return true },
    unwrap: void 0
}

export function just<T>(val: T): Just<T> {
    return {
        val,
        get isJust() { return true },
        get isNil() { return false },
        get unwrap() { return val }
    }
}
export function isJust<T>(val: unknown): val is Just<T> {
    return isMayb(val) && 'val' in val
}
export function isNil(val: unknown): val is Nil {
    return isMayb(val) && 'nil' in val
}
export function isMayb<T>(val: unknown): val is Maybe<T> {
    return typeof val === 'object' && val !== null
}
export function asJust<T>(val: Maybe<T>): Just<T> {
    return val as any
}
