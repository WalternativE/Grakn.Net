// hack because fsi has problems with netstandard2.0
#r "../../packages/fsi/NETStandard.Library.NETFramework/build/net461/lib/netstandard.dll"
// end of hack

#r "./bin/Release/net461/Grakn.Net.dll"
#r "./bin/Release/net461/Grpc.Core.dll"
#r "./bin/Release/net461/Google.Protobuf.dll"
#r "./bin/Release/net461/System.Interactive.Async.dll"

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
            printfn "%A" tx.ResponseStream.Current
            let! n = tx.ResponseStream.MoveNext(CancellationToken.None) |> Async.AwaitTask
            return! listenToResponses n
        | false -> ()
    }
    let! n = tx.ResponseStream.MoveNext(CancellationToken.None) |> Async.AwaitTask
    return! listenToResponses n
}

let q = Query()
q.Value <- "match $x isa company; limit 2; get;"
let eq = ExecQuery()
eq.Query <- q
let i = Infer()
i.Value <- true
eq.Infer <- i

let txRequest = TxRequest()
txRequest.ExecQuery <- eq

let openRequest = TxRequest()
let o = Open()
let ks = Keyspace();
ks.Value <- "training"
o.Keyspace <- ks
openRequest.Open <- o

let cts = CancellationTokenSource()
Async.StartAsTask(listenToResponses(), TaskCreationOptions.None, cts.Token)

tx.RequestStream.WriteAsync(openRequest)
|> Async.AwaitTask
|> Async.RunSynchronously

tx.RequestStream.WriteAsync(txRequest)
|> Async.AwaitTask
|> Async.RunSynchronously

cts.Cancel()
channel.ShutdownAsync().Wait()