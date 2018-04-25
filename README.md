# Grakn.Net

**This repo is extremely experimental. There is no guarantee that anything works and it might even kill your Grakn server. Do not use in production!**

This is the WIP repo for a gRPC based [Grakn](https://grakn.ai/) access library for the .NET runtime.

Currently all development happens in this [fsx script](https://github.com/WalternativE/Grakn.Net/blob/master/src/FsGrakn/Script.fsx) until I figured out the best way to proceed with the API development.

Things I'm currently thinking about are:

- ~~Concurrency model for working with a Grakn transaction (maybe MailBox Processors, maybe Hopac, maybe Rx.NET, maybe TaskBuilder.fs or just async computation expressions all the way)~~ Went with async computation expressions to stay with the core library
- DSL for message creation and combination
- Higher Levle DSL for working with the Knowledge graph
- F#/C# interfaces for all .NET clients
- CI/CD - testing pretty much always needs a running Grakn instance
- packaging

## Requirements

If you work on Windows make sure you have .NET framework >= 4.6.1 installed.

On MacOS or Linux you'll need a recent Mono version (>= 5.0 should be fine).

Regardless of your operating system make sure that you have the dotnet sdk (including the dotnet cli) installed (version >= 2 should be ok).

## Building/Developing

To restore the dependencies and build the projects execute either `.\build.cmd` or `./build.sh`.

If you work on Windows Visual Studio 2017 is currently supported. The prefered development environment is Visual Studio Code with the Ionide and Omnisharp plugins installed.

## Generating gRPC messages and client
### FYI: You can skip this if you don't want to update the existing files from the specs

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