open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing
open Akka.Cluster
open FsharpCommon

let system = System.create "clusterexample" <| ConfigurationFactory.Load()

let workerFun (mailbox: Actor<_>) = 
    let state = ref 0
    
    let rec loop() = 
        actor {
            let! msg = mailbox.Receive()
            printfn "Received: %A" (mailbox.Self.Path.ToString())

            match msg with
            | Value num when num > 0 ->
                state := num
            | Value num ->
                logErrorf mailbox "Received an error-prone value %d" num
            | Respond -> mailbox.Sender().Tell(mailbox.Self.Path.ToString() + " " + (!state).ToString())
            return! loop()
        } 
    loop()

let worker1 = spawn system "worker" workerFun 

[<EntryPoint>]
let main argv = 
    worker1.Tell(Value(2))
    System.Console.ReadKey() |> ignore
    0 