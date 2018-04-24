// hack because fsi has problems with netstandard2.0
#r "../../packages/fsi/NETStandard.Library.NETFramework/build/net461/lib/netstandard.dll"
// end of hack

#r "./bin/Release/net461/Grakn.Net.dll"
#r "./bin/Release/net461/Grpc.Core.dll"
#r "./bin/Release/net461/Google.Protobuf.dll"
#r "./bin/Release/net461/System.Interactive.Async.dll"

#load "GraknUtil.fs"
#load "FsGrakn.fs"

open FsGrakn.Util
open Ai.Grakn.Rpc.Generated
open FsGrakn.Client

let channel = getDefaultChannel "127.0.0.1"

let graknClient = Grakn.GraknClient(channel)

let tx = getTx graknClient "academy" |> Async.RunSynchronously

let q = Inferring "match $x isa company; limit 10; get;"

let res =
    executeQuery tx q |> Async.RunSynchronously

channel.ShutdownAsync().Wait()