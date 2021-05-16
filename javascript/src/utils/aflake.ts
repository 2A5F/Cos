import { Flake } from "./flake";
import { just, Maybe, nil, Option, Voidable } from "./maybe";

export interface ASource<T> extends AsyncIterator<T[]> { }

export type AssociatedAArray<A, B> = [AArray<A>, B[]]

export class AArray<T> extends Array<T> {
    constructor(public readonly source: ASource<T>) { super() }

    #queue: [needlen: number, res: () => void][] = []
    #done = false

    readonly #dotask = async () => {
        if (this.#done) return this.#queue.forEach(([, res]) => res())
        for (const [need, res] of this.#queue) {
            if (this.#done) { res(); continue }
            while (need > this.length) {
                const ir = await this.source.next()
                if (ir.done) {
                    this.#done = true
                    if (ir.value instanceof Array) this.push(...ir.value)
                    break
                } else {
                    this.push(...ir.value)
                }
            }
            res()
        }
    }

    // 要求长度少 n 个，除非没有数据了
    need(n: number = 1) {
        if (this.#done || n <= 0) return
        return new Promise<void>(res => {
            this.#queue.push([n, res])
            this.#dotask()
        })
    }
    // 要求获取新数据至少 n 个，除非没有数据了
    needMore(n: number = 1) {
        if (this.#done || n <= 0) return
        return new Promise<void>(res => {
            this.#queue.push([this.length + n, res])
            this.#dotask()
        })
    }
}

export interface AFlake<T> {
    [i: number]: Promise<Maybe<T>>
}
export class AFlake<T> {
    constructor(public readonly arr: AArray<T>, public readonly off: number = 0, public readonly len?: number) {
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
        if (this.len == null) return this.arr.length <= this.off
        return this.len <= 0
    }
    get nowLen() {
        if (this.len == null) return this.arr.length - this.off
        return this.len
    }

    async get(i: number): Promise<Maybe<T>> {
        if (i < 0) return nil
        if (this.len == null) {
            if (i >= this.nowLen) await this.arr.need(this.off + i + 1)
            if (i >= this.nowLen) return nil
            return just(this.arr[i + this.off])
        } else {
            if (i >= this.len) return nil
            return just(this.arr[i + this.off])
        }
    }
    async getn(i: number): Promise<Voidable<T>> {
        if (this.len == null) {
            if (i >= this.nowLen) await this.arr.need(this.off + i + 1)
            return this.arr[i + this.off]
        } else {
            return this.arr[i + this.off]
        }
    }

    slice(): this
    slice(s: number): AFlake<T>
    slice(s: number, e: number): AFlake<T>
    slice(s?: Option<number>, e?: Option<number>): AFlake<T> {
        if (s == null) return this
        if (e == null) return new AFlake(this.arr, this.off + s, this.len != null ? this.len - s : void 0)
        return new AFlake(this.arr, this.off + s, e - s)
    }
    sliceTo(e: number): AFlake<T> {
        return new AFlake(this.arr, this.off, e)
    }
    sliceFlake(s: number, e: number): Flake<T> {
        return new Flake(this.arr, this.off + s, e - s)
    }
    sliceToFlake(e: number): Flake<T> {
        return new Flake(this.arr, this.off, e)
    }

    get first(): Promise<Maybe<T>> {
        return (async () => {
            if (this.len == null) {
                if (this.isEmpty) await this.arr.need(this.off + 1)
            }
            if (this.isEmpty) return nil
            return just(this.arr[this.off])
        })()
    }

    get last(): Promise<Maybe<T>> {
        return (async () => {
            if (this.len == null) {
                if (this.isEmpty) await this.arr.need(Infinity)
                if (this.isEmpty) return nil
                return just(this.arr[this.arr.length - 1])
            } else {
                if (this.isEmpty) return nil
                return just(this.arr[this.off + this.len - 1])
            }
        })()
    }

    get tail(): AFlake<T> {
        return this.slice(1)
    }

    rawIndex(i: number): number {
        return this.off + i
    }
    fromRawIndex(i: number): number {
        return i - this.off
    }

    toArray(): T[] {
        if (this.len == null) return this.arr.slice(this.off)
        return this.arr.slice(this.off, this.len + this.off)
    }
    async toArrayEnd(): Promise<T[]> {
        if (this.len == null) {
            await this.arr.need(Infinity)
            return this.arr.slice(this.off)
        }
        await this.arr.need(this.len + this.off)
        return this.arr.slice(this.off, this.len + this.off)
    }
    toString(): string {
        if (this.isEmpty) return '[]'
        return `[${this.toArray().join(', ')}]`
    }
    async toStringEnd(): Promise<string> {
        if (this.isEmpty) return '[]'
        return `[${(await this.toArrayEnd()).join(', ')}]`
    }
    toJSON(): string {
        if (this.isEmpty) return '[]'
        return JSON.stringify(this.toArray())
    }
}
