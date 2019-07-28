module Tests

open System
open Xunit
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
let (|Cmd|_|) : _-> Cmd option = 
    fun x -> if Regex.IsMatch(x, "\d.*") then None
             else 
             tryParse x

let rec parseArgs b args =
  match args with
  | [] -> Ok b
  | Parameter "dir" dir :: xs -> parseArgs { b with Dir = dir } xs
  | "--dir" :: dir :: xs -> parseArgs { b with Dir = dir } xs
  | Cmd cmd :: xs-> parseArgs { b with Command = Some cmd } xs
  | invalidArgs ->
    sprintf "error: invalid arguments %A" invalidArgs |> Error
  
let assertEqual(expected:Result<'a,'e>,actual:Result<'a,'e>)=
    match actual, expected with
    | Ok actual, Ok expected when actual = expected -> ()
    | Error actual, Error expected when actual = expected -> ()
    | _ ,_ -> Assert.True (false, sprintf "Expected %A should equal %A" expected actual)

[<Theory>]
[<InlineData("fetch --dir folder")>]
[<InlineData("fetch --dir=folder")>]
[<InlineData("fetch --dir:folder")>]
[<InlineData("--dir:folder fetch")>]
let ``Can parse`` (args:string) =
  let args = split [" "] args
  assertEqual(Ok {Dir="folder"; Command=Some Cmd.fetch}, parseArgs defaultArgs args)

[<Theory>]
[<InlineData("fetchx", "[\"fetchx\"]")>]
[<InlineData("fetch --dirfolder", "[\"--dirfolder\"]")>]
[<InlineData("100", "[\"100\"]")>]
[<InlineData("1", "[\"1\"]")>]
let ``Cannot parse`` (args:string, err) =
  let args = split [" "] args
  assertEqual(Error (sprintf "error: invalid arguments %s" err), parseArgs defaultArgs args)

