#load ".fake/build.fsx/intellisense.fsx"
open Fake.DotNet
open Fake.IO
open Fake.Core
open System.Threading
open Fake.Core.TargetOperators
open Fake.IO.Globbing.Operators


let appPath = "./src/saturn-boilerplate/" |> Path.getFullName
let projectPath = Path.combine appPath "saturn-boilerplate.fsproj"
let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson()

Target.create "Clean" ignore

Target.create "InstallDotNetCore" (fun _ ->
  DotNet.install (fun o -> { o with Version = DotNet.CliVersion.Version dotnetcliVersion }) |> ignore
)


Target.create "Restore" (fun _ ->
    DotNet.restore id projectPath
)

Target.create "Build" (fun _ ->
    DotNet.build id projectPath
)

Target.create "Run" (fun _ ->
  let server = async {
    DotNet.exec (fun p -> { p with WorkingDirectory = appPath } ) "watch" "run" |> ignore
  }
  let browser = async {
    Thread.Sleep 5000
    Process.start (fun i -> { i with FileName = "http://localhost:8085" }) |> ignore
  }

  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

"Clean"
  ==> "InstallDotNetCore"
  ==> "Build"

"Clean"
  ==> "Restore"
  ==> "Run"

Target.runOrDefault "Build"