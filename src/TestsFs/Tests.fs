module Tests

open System
open Xunit
open System.Linq
open FSharpPlus
open System.Text.RegularExpressions
[<Diagnostics.CodeAnalysis.SuppressMessage("*", "EnumCasesNames")>]
type Cmd=
  |fetch=0
  |batch=1
  |sum=2
  |writeLangCount=3
type CmdArgs =
  { Dir: string; Command: Cmd option}
let defaultArgs = { Dir= "Dir"; Command = None }
let (|Parameter|_|) name : _-> string option =
  let regex = Regex(sprintf "--%s[:=](.+)" (Regex.Escape name))
  fun value ->  
    let m = regex.Match value
    if m.Success then Some ((nth 1 m.Groups).Value)
    else None
let (|Cmd|_|) : _-> Cmd option = tryParse

let rec parseArgs b args =
  match args with
  | [] -> Ok b
  | Parameter "dir" dir :: xs -> parseArgs { b with Dir = dir } xs
  | "--dir" :: dir :: xs -> parseArgs { b with Dir = dir } xs
  | Cmd cmd :: xs-> parseArgs { b with Command = Some cmd } xs
  | invalidArgs ->
    sprintf "error: invalid arguments %A" invalidArgs |> Error
  
[<Theory>]
[<InlineData("fetch --dir folder")>]
[<InlineData("fetch --dir=folder")>]
[<InlineData("fetch --dir:folder")>]
let ``Can parse`` (args:string) =
  let args =  args.Split(" ") |> List.ofArray
  Assert.Equal(Ok {Dir="folder"; Command=Some Cmd.fetch}, parseArgs defaultArgs args)

