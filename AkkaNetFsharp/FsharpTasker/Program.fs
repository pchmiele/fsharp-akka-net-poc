open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing
open Akka.Cluster
open FsharpCommon


let system = System.create "clusterexample" <| ConfigurationFactory.Load()
let router = spawne system "router" <@ WorkerActor.workerActor @> <| [SpawnOption.Router(FromConfig.Instance)] 

[<EntryPoint>]
let main argv = 
    async {
        do! Async.Sleep 5000
    } |> Async.RunSynchronously

    [1..1000] |> Seq.iter(fun x -> 
        async {
            let! response = router <? (Message.TaskMessage x)
            printfn "%A" response
        } |> Async.RunSynchronously
    )
    System.Console.ReadKey() |> ignore
    0