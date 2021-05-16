export type Ref<T> = { val: T }
export type Box<T> = { readonly val: T }

export function ref<T>(val: T): Ref<T> {
    return { val }
}

export function box<T>(val: T): Box<T> {
    return Object.defineProperty({}, 'val', { value: val })
}

export function isRef<T>(val: unknown): val is Ref<T> {
    let d: PropertyDescriptor | undefined
    return typeof val === 'object' && val !== null && 'val' in val && (d = Object.getOwnPropertyDescriptor(val, 'val'), d?.writable || (d?.set != null))
}
export function isBox<T>(val: unknown): val is Box<T> {
    return typeof val === 'object' && val !== null && 'val' in val
}
