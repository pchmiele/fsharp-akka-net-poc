open System
open Akka
open Akka.Actor
open Akka.FSharp
open Akka.Configuration
open Akka.Routing
open Akka.Cluster
open FsharpCommon

let strategy = 
    Strategy.OneForOne (fun x -> 
        match x with
        | :? ArithmeticException -> Directive.Restart
        | :? ArgumentException -> Directive.Stop
        | _ -> Directive.Escalate)
        
let system = System.create "clusterexample" <| ConfigurationFactory.Load()

let aref =  
    spawn system "tasker"
    <| fun mailbox ->
        let cluster = Cluster.Get (mailbox.Context.System)
        cluster.Subscribe (mailbox.Self, [| typeof<ClusterEvent.MemberUp> |])
        mailbox.Defer <| fun () -> cluster.Unsubscribe (mailbox.Self)
        let rec loop () = 
            actor {
                let! (msg: obj) = mailbox.Receive ()
                match msg with
                // wait for member up message from seed
                | :? ClusterEvent.MemberUp as up when up.Member.HasRole "seed" -> 
                    let sref = select (up.Member.Address.ToString() + "/user/listener") mailbox
                    sref <! "Hello"
                | _ -> printfn "Received: %A" msg
                return! loop () }
        loop ()

[<EntryPoint>]
let main argv = 
//    [1..20] |> Seq.iter (fun x -> 
//        async {
//            let value = if x % 5 <> 0 then x else 0
//            aref.Tell(Value value)
//            do! Async.Sleep 500
//            let! r = aref <? Respond
//            printfn "value received %A" r
//        } |> Async.RunSynchronously
//    )
    System.Console.ReadKey() |> ignore
    0 // return an integer exit code