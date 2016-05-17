namespace FsharpCommon

open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing
open FsharpCommon
open Akka.Dispatch
open System.Threading

module WorkerActor =
    let workerActor (mailbox: Actor<_>) =        
        let rec loop() = 
            actor {
                let! msg = mailbox.Receive()
                match msg with
                | TaskMessage num ->
                    printfn "Actor: %s" (mailbox.Self.Path.ToString())
                    Akka.Dispatch.ActorTaskScheduler.RunTask(fun () ->
                          async {
                            printfn "Started = %d" num
                            do! Async.Sleep 300
                            } |> Async.StartAsTask :> Threading.Tasks.Task)

                    mailbox.Sender().Tell("Finished "+ num.ToString())
                
                return! loop()
            } 
        loop()