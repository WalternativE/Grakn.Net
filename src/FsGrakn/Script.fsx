// hack because fsi has problems with netstandard2.0
#r "../../packages/fsi/NETStandard.Library.NETFramework/build/net461/lib/netstandard.dll"
// end of hack

#r "./bin/Release/net461/Grakn.Net.dll"
#r "./bin/Release/net461/Grpc.Core.dll"
#r "./bin/Release/net461/Google.Protobuf.dll"
#r "./bin/Release/net461/System.Interactive.Async.dll"

#load "GraknUtil.fs"

open System.Threading

open Grpc.Core

let channel = Channel("127.0.0.1:48555", ChannelCredentials.Insecure)

open Ai.Grakn.Rpc.Generated

let graknClient = Grakn.GraknClient(channel)

type GraknTransaction = AsyncDuplexStreamingCall<TxRequest, TxResponse>

let getResponse (tx : GraknTransaction) = async {
    let! n = tx.ResponseStream.MoveNext(CancellationToken.None) |> Async.AwaitTask
    let resp =
        match n with
        | true ->
            Some tx.ResponseStream.Current
        | false ->
            None
    return resp
}

open FsGrakn.Util

type ResponseCase = TxResponse.ResponseOneofCase

let getNewTx (keySpace : string) =
    let tx = graknClient.Tx()
    let o = openRequest keySpace Read

    async {
        do! tx.RequestStream.WriteAsync o |> Async.AwaitTask
        let! response = getResponse tx
        
        let txToReturn = 
            match response with
            | Some r ->
                match r.ResponseCase with
                | ResponseCase.Done -> tx
                | _ -> failwith "Nope" // TODO think about error handling
            | None -> failwith "Nope"

        return txToReturn
    }

let resolveIterator (tx : GraknTransaction) (iteratorId : IteratorId) =
    async {
        let rec resolveIterator (results : TxResponse list) (itId : IteratorId) (resp : TxResponse) =
            match resp.ResponseCase with
            | ResponseCase.Done -> async { return results }
            | _ ->
                async {
                    let nr = nextRequest itId
                    do! tx.RequestStream.WriteAsync nr |> Async.AwaitTask
                    let! response = getResponse tx
                    match response with
                    | Some r ->
                        return! resolveIterator (r::results) itId r
                    | None -> return results
                }

        let nr = nextRequest iteratorId
        do! tx.RequestStream.WriteAsync nr |> Async.AwaitTask
        let! response = getResponse tx
        match response with
        | Some r ->
            return! resolveIterator [] iteratorId r
        | None -> return []
    }

let tx = getNewTx "academy" |> Async.RunSynchronously

let q = defaultExecQueryRequest "match $x isa company; limit 2; get;"

tx.RequestStream.WriteAsync(q)
|> Async.AwaitTask
|> Async.RunSynchronously

let response = getResponse tx |> Async.RunSynchronously

let results =
    match response with
    | Some r ->
        resolveIterator tx r.IteratorId |> Async.RunSynchronously
    | None -> []

channel.ShutdownAsync().Wait()