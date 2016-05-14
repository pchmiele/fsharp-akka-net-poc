//System.IO.Directory.SetCurrentDirectory(@"C:\Users\pchmiele\Documents\Visual Studio 2013\Projects\ConsoleApplication4\ConsoleApplication4") ;;

#if INTERACTIVE
#r @"bin\Debug\Akka.dll"
#r @"bin\Debug\Akka.FSharp.dll"
#endif

open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing

type Message =
    | Value of int
    | Respond

let workerFun (mailbox: Actor<_>) = 
    let state = ref 0
    
    let rec loop() = 
        actor {
            let! msg = mailbox.Receive()
            match msg with
            | Value num when num > 0 ->
                state := num
            | Value num ->
                logErrorf mailbox "Received an error-prone value %d" num
            | Respond -> mailbox.Sender().Tell(mailbox.Self.Path.ToString() + " " + (!state).ToString())
            return! loop()            
        } 
    loop()

let strategy = 
    Strategy.OneForOne (fun x -> 
        match x with
        | :? ArithmeticException -> Directive.Restart
        | :? ArgumentException -> Directive.Stop
        | _ -> Directive.Escalate)
        
let system = System.create "SupervisorSystem" <| ConfigurationFactory.Default()
let worker1 = spawnOpt system "Worker1" workerFun [SupervisorStrategy(strategy)]
let worker2 = spawnOpt system "Worker2" workerFun [SupervisorStrategy(strategy)]
let worker3 = spawnOpt system "Worker3" workerFun [SupervisorStrategy(strategy)]

let supervisor = system.ActorOf(Props.Empty.WithRouter(RoundRobinGroup("/user/Worker1", "/user/Worker2", "/user/Worker3")), "router")

[<EntryPoint>]
let main argv = 
    [1..20] |> Seq.iter (fun x -> 
        async {
            let value = if x % 5 <> 0 then x else 0
            supervisor.Tell(Value value)
            do! Async.Sleep 500
            let! r = supervisor <? Respond
            printfn "value received %A" r
        } |> Async.RunSynchronously
    )
    System.Console.ReadKey() |> ignore
    0 // return an integer exit code
