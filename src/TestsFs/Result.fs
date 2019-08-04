module Result
open Xunit

let assertEqual(expected:Result<'a,'e>,actual:Result<'a,'e>)=
    match actual, expected with
    | Ok actual, Ok expected when actual = expected -> ()
    | Error actual, Error expected when actual = expected -> ()
    | _ ,_ -> Assert.True (false, sprintf "Expected %A should equal %A" expected actual)
