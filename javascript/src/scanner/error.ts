import { Pos } from "../utils";

export class ScannerError extends Error {
    constructor(public readonly kind: ScannerErrorKind) {
        super()
    }
}

export type ScannerErrorKind =
    | { t: 'multiple errors', errs: ScannerErrorKind[] }
    | { t: 'unknown symbol', pos: Pos }
    | { t: 'unexpected eof', pos: Pos }
    