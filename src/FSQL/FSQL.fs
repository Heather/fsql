module Fsql

open System
open System.IO
open System.Text

let fsql (lines : seq<string>) = 
    String.Join(Environment.NewLine,
        [for line in lines do
            let nsplitted = [for s in line.Split(' ','\r','\n','\t',';') do
                                if s <> "" && not <| String.IsNullOrEmpty(s) then
                                    yield s]
            let newline = ref line
            if nsplitted.Length > 1 then
                for word in nsplitted do
                    let repl =
                        match word.ToLower() with
                        | "!" -> "select"
                        | "~" -> "from"
                        | "let" -> "set"
                        | "var" -> "declare"
                        | _ -> ""
                    if not <| String.IsNullOrEmpty(repl)
                        then newline:= (!newline).Replace(word, repl)
            yield !newline ])
