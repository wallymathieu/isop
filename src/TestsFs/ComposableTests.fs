module ComposableTests

open System
open Xunit
open FSharpPlus
open System.Text.RegularExpressions
open FSharpPlus
open FSharpPlus.Data
type CLIPart<'a> = 'a -> OptionT<Async<'a option>>
module CLIPart=
  let choose (options : CLIPart<'a> list) = fun x -> choice (List.map ((|>) x) options)
  let inline fail (_:'a) : OptionT<Async<'a option>> = OptionT <| async.Return None

let (|Parameter|_|) name : _-> string option =
  let regex = Regex(sprintf "--%s[:=](.+)" (Regex.Escape name))
  fun value ->  
    let m = regex.Match value
    if m.Success then Some ((nth 1 m.Groups).Value)
    else None

module Filters=
  let method (text : string) = OptionT << fun (x : string list) -> async.Return (match x with x'::xs when x'=text -> Some xs | _->  None)
  


type CmdArgs =
  { Dir: string; }
let defaultArgs = { Dir= "Dir"; }

let rec parseArgs b args =
  match args with
  | [] -> Ok b
  | Parameter "dir" dir :: xs -> parseArgs { b with Dir = dir } xs
  | "--dir" :: dir :: xs -> parseArgs { b with Dir = dir } xs
  | invalidArgs ->
    sprintf "error: invalid arguments %A" invalidArgs |> Error

let app=CLIPart.choose [ 
  Filters.method "fetch" >=> 
  (fun args-> monad {
    match parseArgs defaultArgs args with
    | Ok v->
        // do some async work
        return []
    | Error err->
      Console.Error.WriteLine err
      // print error
      return! CLIPart.fail []
  }) ]

let parse args=async {
  match! app args |> OptionT.run with
    | Some _ -> return 0
    | None -> return 1
}

[<Theory>]
[<InlineData("fetch --dir folder")>]
[<InlineData("fetch --dir=folder")>]
[<InlineData("fetch --dir:folder")>]
[<InlineData("--dir:folder fetch")>]
let ``Can parse`` (args:string) =
  let args = split [" "] args
  Result.assertEqual(Ok {Dir="folder"; }, parseArgs defaultArgs args)
