open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing
open Akka.Cluster
open FsharpCommon

let system = System.create "clusterexample" <| ConfigurationFactory.Load()

[<EntryPoint>]
let main argv = 
    System.Console.ReadKey() |> ignore
    0 