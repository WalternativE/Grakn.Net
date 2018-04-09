# Grakn.Net

This is the WIP repo for a gRPC based [Grakn](https://grakn.ai/) access library for the .NET runtime.

In order to generate the message classes you will have to execute

```powershell
.\packages\build\Grpc.Tools\tools\windows_x64\protoc.exe -I./grakn-spec/proto --csharp_out src\Grakn.Net .\grakn-spec\proto\concept.proto
```

and

``` powershell
.\packages\build\Grpc.Tools\tools\windows_x64\protoc.exe -I./grakn-spec/proto --csharp_out src\Grakn.Net .\grakn-spec\proto\iterator.proto
```

respectevely.

In order to generate the `Grakn.cs` as well as the `GraknGrp.cs` files issue the following command in the root directory of this repository after restoring the nuget dependencies using `.paket\paket.exe restore`:

```powershell
.\packages\build\Grpc.Tools\tools\windows_x64\protoc.exe -I./grakn-spec/proto --csharp_out src\Grakn.Net --grpc_out src\Grakn.Net ./grakn-spec/proto/grakn.proto --plugin=protoc-gen-grpc=.\packages\build\Grpc.Tools\tools\windows_x64\grpc_csharp_plugin.exe
```

If you are not currently on Windows you'll need to change the bath to to `protoc` tool and the `grpc_csharp_plugin`. You'll also have to adapt the path delimiters to reflect the ones used in your local environment.