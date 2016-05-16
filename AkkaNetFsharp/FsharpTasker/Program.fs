﻿open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing
open Akka.Cluster
open FsharpCommon

let config = ConfigurationFactory.Load()
let system = System.create "clusterexample" <| config


let router = spawnOpt system "router"  (actorOf (fun msg -> printfn "received '%s'" msg)) [SpawnOption.Router(FromConfig.Instance)]
printfn "%A" router.Path

[<EntryPoint>]
let main argv = 
    [1..20000] |> Seq.iter (fun x -> 
        async {
            router.Tell(Value 5)
            do! Async.Sleep 500
            let! response = router <? Respond
            printfn "value received %A" response
         } |> Async.RunSynchronously
    )
    System.Console.ReadKey() |> ignore
    0 