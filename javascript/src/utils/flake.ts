import { just, Maybe, nil, Option, Voidable } from "./maybe";

export interface Flake<T> {
    readonly [index: number]: Maybe<T>
}
export class Flake<T> {
    constructor(public readonly arr: T[], public readonly off: number = 0, public readonly len: number = arr.length - off) {
        return new Proxy(this, {
            get(target, p, receiver) {
                if (typeof p === 'symbol') return Reflect.get(target, p, receiver)
                const n = +p
                if (isNaN(n)) return Reflect.get(target, p, receiver)
                return target.get(n)
            }
        })
    }

    get isEmpty() {
        return this.len <= 0
    }

    get(i: number): Maybe<T> {
        if (i < 0 || i >= this.len) return nil
        return just(this.arr[i + this.off])
    }
    getn(i: number): Voidable<T> {
        return this.arr[i + this.off]
    }

    slice(): this
    slice(s: number): Flake<T>
    slice(s: number, e: number): Flake<T>
    slice(s?: Option<number>, e?: Option<number>): Flake<T> {
        if (s == null) return this
        if (e == null) return new Flake(this.arr, this.off + s, this.len - s)
        return new Flake(this.arr, this.off + s, e - s)
    }
    sliceTo(e: number): Flake<T> {
        return new Flake(this.arr, this.off, e)
    }

    get first(): Maybe<T> {
        if (this.isEmpty) return nil
        return just(this.arr[this.off])
    }

    get last(): Maybe<T> {
        if (this.isEmpty) return nil
        return just(this.arr[this.off + this.len - 1])
    }

    get tail(): Flake<T> {
        return this.slice(1)
    }

    rawIndex(i: number): number {
        return this.off + i
    }
    fromRawIndex(i: number): number {
        return i - this.off
    }

    toArray(): T[] {
        return this.arr.slice(this.off, this.len + this.off)
    }
    toString(): string {
        if (this.isEmpty) return '[]'
        return `[${this.toArray().join(', ')}]`
    }
    toJSON(): string {
        if (this.isEmpty) return '[]'
        return JSON.stringify(this.toArray())
    }
}
