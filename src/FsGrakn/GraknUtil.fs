module FsGrakn.Util

open Ai.Grakn.Rpc.Generated
open Grpc.Core

type GraknTransaction = AsyncDuplexStreamingCall<TxRequest, TxResponse>

type ResponseCase = TxResponse.ResponseOneofCase
type QueryResponseCase = QueryResult.QueryResultOneofCase

type TransactionType =
    | Read
    | Write
    | Batch

type GraknQuery =
    | Inferring of string
    | NonInferring of string

type GraknResponse =
    | Done
    | ResultGraph of TxResponse list
    | ErrorResponse of string

let getChannel (credentialConfig : ChannelCredentials) (port : string) (host :string) =
    let hostPort = sprintf "%s:%s" host port
    Channel(hostPort, credentialConfig)

let getDefaultChannel =
    getChannel ChannelCredentials.Insecure "48555"

let openRequest (keyspace : string) (txType : TransactionType) =
    let tt =
        match txType with
        | Read -> TxType.Read
        | Write -> TxType.Write
        | Batch -> TxType.Batch

    let ks = Keyspace(Value = keyspace)
    TxRequest(Open = Open (Keyspace = ks, TxType = tt))

let commitRequest () =
    TxRequest(Commit = Commit())

let execQueryRequest (inferring : bool) (query : string) =
    let query = Query(Value = query)
    let infer = Infer(Value = inferring)
    TxRequest(ExecQuery = ExecQuery(Query = query, Infer = infer))

let defaultExecQueryRequest = execQueryRequest true

let nextRequest (iteratorId : IteratorId) =
    let nxt = Next (IteratorId = iteratorId)
    TxRequest (Next = nxt)