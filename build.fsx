// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "./packages/build/FAKE/tools/FakeLib.dll"

open Fake
open System

// --------------------------------------------------------------------------------------
// Build variables
// --------------------------------------------------------------------------------------

let dotnetcliVersion = "2.1.105"
let mutable dotnetExePath = "dotnet"

open Fake.Git

let appTitle = "FsGrakn"
let appDescription = ".NET driver for Grakn.AI knowledge graph"
let author = "Gregor Beyerle - gregor.beyerle@gmail.com"
let version = "0.0.1.0-alpha"
let commitHash = Information.getCurrentHash ()

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------

let run' timeout cmd args dir =
    if execProcess (fun info ->
        info.FileName <- cmd
        if not (String.IsNullOrWhiteSpace dir) then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) timeout |> not then
        failwithf "Error while running '%s' with args: %s" cmd args

let run = run' System.TimeSpan.MaxValue

let runDotnet workingDir args =
    let result =
        ExecProcess (fun info ->
            info.FileName <- dotnetExePath
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------

// TODO will have a look at CI/CD tools - if they all have containers with dotnet cli
// I'll just make it a requirment and delete manual tool handling
Target "InstallDotNetCLI" (fun _ ->
    dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "Restore" (fun _ ->
    DotNetCli.Restore id
)

Target "Clean" (fun _ ->
    DotNetCli.RunCommand id "clean"
)

open Fake.AssemblyInfoFile

Target "CreateAssemblyInfo" (fun _ ->
    CreateFSharpAssemblyInfo "./src/FsGrakn/Properties/AssemblyInfo.fs"
        [ Attribute.Title appTitle
          Attribute.Version version
          Attribute.FileVersion version
          Attribute.Description appDescription
          Attribute.InternalsVisibleTo "FsGrakn.Test"
          Attribute.Product appTitle
          Attribute.Guid "5341159e-5f87-40aa-a5e1-0d7dea3d4fe3"
          Attribute.Copyright author
          Attribute.Metadata("githash", commitHash) ]
)

Target "Build" (fun _ ->
    DotNetCli.Build id
)

Target "Test" (fun _ ->
    DotNetCli.Test (fun p ->
        { p with WorkingDir = "test/FsGrakn.Test"}
    )
)

// --------------------------------------------------------------------------------------
// Build order
// --------------------------------------------------------------------------------------

"Clean"
    ==> "Restore"
    ==> "CreateAssemblyInfo"
    ==> "Build"
    ==> "Test"

RunTargetOrDefault "Build"
