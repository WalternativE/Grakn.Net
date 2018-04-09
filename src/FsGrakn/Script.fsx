// hack because fsi has problems with netstandard2.0
#r "../../packages/fsi/NETStandard.Library.NETFramework/build/net461/lib/netstandard.dll"
// end of hack

#r "./bin/Release/net461/Grakn.Net.dll"
#r "./bin/Release/net461/Grpc.Core.dll"
#r "./bin/Release/net461/Google.Protobuf.dll"
#r "./bin/Release/net461/System.Interactive.Async.dll"

#load "GraknUtil.fs"

open System.Threading
open System.Threading.Tasks

open Grpc.Core

let channel = Channel("127.0.0.1:48555", ChannelCredentials.Insecure)

open Ai.Grakn.Rpc.Generated

let graknClient = Grakn.GraknClient(channel)

let tx = graknClient.Tx()

let listenToResponses () = async {
    let rec listenToResponses isNext = async {
        match isNext with
        | true ->
            let resp = tx.ResponseStream.Current
            let qr = resp.QueryResult
            let d = resp.Done
            let ii = resp.IteratorId
            printfn "QueryResult %A Done %A IteratorId %A" qr d ii
            let! n = tx.ResponseStream.MoveNext(CancellationToken.None) |> Async.AwaitTask
            return! listenToResponses n
        | false -> ()
    }
    let! n = tx.ResponseStream.MoveNext(CancellationToken.None) |> Async.AwaitTask
    return! listenToResponses n
}

open FsGrakn.Util

let o = openRequest "academy" Read
let q = defaultExecQueryRequest "match $x isa company; limit 2; get;"

let cts = CancellationTokenSource()
Async.StartAsTask(listenToResponses(), TaskCreationOptions.None, cts.Token)

tx.RequestStream.WriteAsync(o)
|> Async.AwaitTask
|> Async.RunSynchronously

tx.RequestStream.WriteAsync(q)
|> Async.AwaitTask
|> Async.RunSynchronously

cts.Cancel()
channel.ShutdownAsync().Wait()