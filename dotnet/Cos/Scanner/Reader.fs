module Volight.Cos.Parser.Reader

open Volight.Cos.SrcPos

let read (code: 'a) = 
    let mutable r = false
    let mutable line = 0u
    let mutable column = 0u
    seq {
        yield! seq { 
            for c in code do
            yield { Line = line; Column = column }
            match c with
            | '\r' ->
                line <- line + 1u;
                column <- 0u;
                r <- true;
            | '\n' ->
                if not r then
                    line <- line + 1u;
                    column <- 0u
                r <- false;
            | _ ->
                column <- column + 1u;
                r <- false;
        }
        yield { Line = line; Column = column }
    }
    
