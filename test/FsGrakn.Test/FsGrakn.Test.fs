module FsGrakn.Test

open Xunit
open FsGrakn.Util
open FsGrakn.Client

(*
    Be aware that you should always have a Grakn instance on localhost running for these tests
    I'm thinking about some smart way to do a nicer setup for the db - but it's quite a heavy system
*)

[<Literal>]
let defaultKeyspace = "academy"

[<Literal>]
let defaultHost = "127.0.0.1"

[<Fact>]
let ``Given any runtime this test should always pass`` () =
    // this is a stupid test - but given that sometimes full framework, mono and .net core
    // act up it's good to have it as a smoke test
    Assert.True(true)

[<Fact>]
let ``Given an open request to a connection a done should be returned`` () =
    use client = getClient defaultHost
    use tx = client.Client.Tx()
    
    let o = openRequest defaultKeyspace Read

    let res = 
        async {
            return! sendRequest tx o
        } |> Async.RunSynchronously
    
    let isItADone =
        match res with
        | Some r ->
            match r.ResponseCase with
            | ResponseCase.Done -> true
            | _ -> false
        | None -> false

    Assert.True(isItADone)