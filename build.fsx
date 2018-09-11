#r "paket:
source nuget/dotnetcore
source https://api.nuget.org/v3/index.json
nuget Fake.Core.Target
nuget Fake.Tools.Git
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Cli //"

#load "./.fake/build.fsx/intellisense.fsx"

open Fake.DotNet
open Fake.Core
open Fake.Tools

// --------------------------------------------------------------------------------------
// Build variables
// --------------------------------------------------------------------------------------


let appTitle = "FsGrakn"
let appDescription = ".NET driver for Grakn.AI knowledge graph"
let author = "Gregor Beyerle - gregor.beyerle@gmail.com"
let version = "0.0.1.0-alpha"
let commitHash = Git.Information.getCurrentHash ()

let solutionName = "Grakn.Net.sln"

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------

let install = lazy DotNet.install DotNet.Versions.FromGlobalJson

let inline dotnetWorkDir wd =
    DotNet.Options.lift install.Value
    >> DotNet.Options.withWorkingDirectory wd

let inline dotnetSimple arg = DotNet.Options.lift install.Value arg

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------

Target.create "Restore" (fun _ ->
    DotNet.restore dotnetSimple solutionName
)

Target.create "Clean" (fun _ ->
    let procResult =
        DotNet.exec dotnetSimple "clean" solutionName

    if procResult.ExitCode <> 0 then failwith "Cleaning the solution failed"
)

Target.create "CreateAssemblyInfo" (fun _ ->
    AssemblyInfoFile.createFSharp "./src/FsGrakn/Properties/AssemblyInfo.fs"
        [ AssemblyInfo.Title appTitle
          AssemblyInfo.Version version
          AssemblyInfo.FileVersion version
          AssemblyInfo.Description appDescription
          AssemblyInfo.InternalsVisibleTo "FsGrakn.Test"
          AssemblyInfo.Product appTitle
          AssemblyInfo.Guid "5341159e-5f87-40aa-a5e1-0d7dea3d4fe3"
          AssemblyInfo.Copyright author
          AssemblyInfo.Metadata("githash", commitHash) ]
)

Target.create "Build" (fun _ ->
    DotNet.build dotnetSimple solutionName
)

Target.create "Test" (fun _ ->
    DotNet.test (dotnetWorkDir "test") "FsGrakn.Test"
)

// --------------------------------------------------------------------------------------
// Build order
// --------------------------------------------------------------------------------------

open Fake.Core.TargetOperators

"Clean"
    ==> "Restore"
    ==> "CreateAssemblyInfo"
    ==> "Build"
    ==> "Test"

Target.runOrDefault "Build"