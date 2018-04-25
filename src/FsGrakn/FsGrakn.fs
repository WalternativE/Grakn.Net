namespace FsGrakn

module Client =

    open Util
    open System.Threading
    open Ai.Grakn.Rpc.Generated
    open Grpc.Core
    open System

    type T (channel : Channel) =
        let client = Grakn.GraknClient(channel)

        member this.Client = client

        interface IDisposable with
            member this.Dispose () =
                channel.ShutdownAsync().Wait()

    let private getResponse (tx : GraknTransaction) = async {
        let! n = tx.ResponseStream.MoveNext(CancellationToken.None) |> Async.AwaitTask
        let resp =
            match n with
            | true ->
                Some tx.ResponseStream.Current
            | false ->
                None
        return resp
    }

    let internal sendRequest (tx : GraknTransaction) (request : TxRequest) =
        async {
            do! tx.RequestStream.WriteAsync request |> Async.AwaitTask
            return! getResponse tx
        }

    /// <summary>Creates the client wrapper around a new open channel for a given host. You might want only one of these per program.</summary>
    let getClient (host : string) =
        let channel = getDefaultChannel host
        new T(channel)

    /// <summary>Creates a new transaction - the main unit of operation. Make sure to only bind this with use bindings</summary>
    let getTx (graknClient : T) (keySpace : string) =
        let tx = graknClient.Client.Tx()
        let o = openRequest keySpace Read

        async {
            let! response = sendRequest tx o
            
            let txToReturn = 
                match response with
                | Some r ->
                    match r.ResponseCase with
                    | ResponseCase.Done -> tx
                    | _ -> failwith "Nope" // TODO think about error handling
                | None -> failwith "Nope"

            return txToReturn
        }

    let private convertAnswer (answer : Answer) =
        () 

    let private convert (responses : TxResponse list) =
        responses
        |> List.map (fun res ->
            match res.ResponseCase with
            | ResponseCase.QueryResult -> 
                match res.QueryResult.QueryResultCase with
                | QueryResponseCase.Answer -> convertAnswer res.QueryResult.Answer
                | _ -> () )

    let private resolveIterator (tx : GraknTransaction) (iteratorId : IteratorId) =
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
                return!
                    resolveIterator [] iteratorId r
            | None -> return []
        }

    let executeQuery (tx : GraknTransaction) (query : GraknQuery) =
        let q =
            match query with
            | Inferring q -> defaultExecQueryRequest q
            | NonInferring q -> execQueryRequest false q

        async {
            let! response = sendRequest tx q
            
            let resolvedResponse =
                response
                |> Option.map (fun r ->
                    match r.ResponseCase with
                    | ResponseCase.Done -> async { return GraknResponse.Done }
                    | ResponseCase.IteratorId ->
                        async {
                            let! iterationResult = resolveIterator tx r.IteratorId
                            return iterationResult |> GraknResponse.ResultGraph
                        }
                    | ResponseCase.QueryResult ->
                        async { return GraknResponse.ResultGraph [ r ] }
                    | _ -> async { return GraknResponse.ErrorResponse "Unexpected result" })
                

            match resolvedResponse with
            | Some r ->
                return! r
            | None -> return "No result available" |> ErrorResponse
        }