#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
#r "packages/GitVersion/Lib/Net45/GitVersionCore.dll"
#r "packages/LibGit2Sharp/lib/net40/LibGit2Sharp.dll"
open GitVersion
open Fake
open Fake.NuGet.Install
open System.Collections.Generic
open System

#load "helpers.fsx"
open Helpers

let solution = ("../src/SourceBrowser.sln" |> FullNameFromHere)

let mutable semanticVersion : SemanticVersion = null
let mutable semanticVersionVariables : Dictionary<string, string> = null

Target "Restore" (fun _ ->
  CustomRestorePackage  solution
)


Target "Build" (fun _ ->
  let buildMode = getBuildParamOrDefault "buildMode" "Release"

  let setParams defaults =
        { defaults with
            Verbosity = Some(MSBuildVerbosity.Normal)
            Targets = ["Build"]
            Properties =
                [
                    "Configuration", buildMode
                ]
         }

  build setParams solution
)

Target "Versioning" (fun _ ->

  let log (s :string) = System.Console.WriteLine(s)
  let action = (System.Action<string> log)

  GitVersion.Logger.WriteInfo <- action
  GitVersion.Logger.WriteWarning <- action
  GitVersion.Logger.WriteError <- action
  let dir = (".." |> FullNameFromHere)
  let repo =
    RepositoryLoader.GetRepo(dir)


  let servers = GitVersion.BuildServerList.GetApplicableBuildServers(GitVersion.Authentication())

  for server in servers do
    server.PerformPreProcessingSteps (dir) |> ignore


  semanticVersion <- GitVersionFinder.GetSemanticVersion(repo)

  for server in servers do
    server.WriteIntegration(semanticVersion, action) |> ignore

  semanticVersionVariables <- VariableProvider.GetVariablesFor(semanticVersion)

  BulkReplaceAssemblyInfoVersions (".." |> FullNameFromHere) (fun f ->
                                              {f with
                                                  AssemblyVersion = semanticVersionVariables.["AssemblySemVer"]
                                                  AssemblyFileVersion = semanticVersionVariables.["AssemblySemVer"]
                                                  AssemblyInformationalVersion = semanticVersionVariables.["SemVer"]
                                                  })

)

Target "Pack" (fun _ ->
    CusomtNuGetPack ("../src/SourceBrowser.Generator/SourceBrowser.Generator.csproj" |> FullNameFromHere) (sprintf "%s-beta"semanticVersionVariables.["NuGetVersionV2"]) "Release"
)

Target "Test" (fun _ ->
    trace "test stuff..."

)


"Build"
   ==> "Test"

"Restore"
   ==> "Build"

"Versioning"
   ==> "Build"

"Build"
   ==> "Pack"

Run "Pack"
