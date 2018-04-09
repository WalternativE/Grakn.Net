module FsGrakn.Util

open Ai.Grakn.Rpc.Generated

type TransactionType =
    | Read
    | Write
    | Batch

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
