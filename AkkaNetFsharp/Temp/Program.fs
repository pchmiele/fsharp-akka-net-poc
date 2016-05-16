open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing
open Akka.Cluster
open FsharpCommon

let config = ConfigurationFactory.Load()
let system = System.create "clusterexample" <| config

[<EntryPoint>]
let main argv = 
    let router = select @"akka.tcp://clusterexample@127.0.0.1:4053/user/router" system  
    printfn "%A" router.Path

    [1..20000] |> Seq.iter (fun x -> 
        async {
            printfn "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
            router.Tell(Value 5)
            printfn "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"

            do! Async.Sleep 500
            printfn "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"

            let! response = router <? Respond

            printfn "value received %A" response
         } |> Async.RunSynchronously
    )
    System.Console.ReadKey() |> ignore
    0 