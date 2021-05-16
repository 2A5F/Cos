export function use<T, R = T>(v: T, f: (v: T) => R): R {
    return f(v)
}
export function also<T>(v: T, f: (v: T) => void): T {
    f(v)
    return v
}
