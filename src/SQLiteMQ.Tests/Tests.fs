﻿module Tests

open FsUnit
open MessageQueue
open NUnit.Framework
open System

type Cat = 
  { Name : string }

type Dog = 
  { Name : string }

[<Test>]
let ``it enqueues and dequeues``() = 
  use mq = MessageQueue.create InMemory
  let cat = { Cat.Name = "Bubbles" }
  cat |> mq.Enqueue
  mq.Dequeue<Cat>() |> ignore
  mq.Dequeue<Cat>() |> should equal None

[<Test>]
let ``it dequeues FIFO``() = 
  use mq = MessageQueue.create InMemory
  let cats = [ 1..10 ] |> List.map (fun i -> { Cat.Name = sprintf "%i" i })
  cats |> Seq.iter mq.Enqueue
  for cat in cats do
    mq.Dequeue<Cat>()
    |> should equal
    <| Some cat

[<Test>]
let ``it queues different types``() = 
  use mq = MessageQueue.create InMemory
  let cat = { Cat.Name = "Bubbles" }
  let dog = { Dog.Name = "Rufus" }
  cat |> mq.Enqueue
  dog |> mq.Enqueue
  mq.Dequeue<Dog>()
  |> should equal
  <| Some dog
  mq.Dequeue<Cat>()
  |> should equal
  <| Some cat

[<Test>]
let ``it can clear the queue of a type``() = 
  use mq = MessageQueue.create InMemory
  let cats = [ 1..10 ] |> List.map (fun i -> { Cat.Name = sprintf "%i" i })
  cats |> Seq.iter mq.Enqueue
  let dog = { Dog.Name = "Rufus" }
  dog |> mq.Enqueue
  mq.Clear<Cat>() |> should equal 10
  mq.Dequeue<Cat>() |> should equal None
  mq.Dequeue<Dog>()
  |> should equal
  <| Some dog

[<Test>]
let ``it can clear the queue of all types``() = 
  use mq = MessageQueue.create InMemory
  { Cat.Name = "Bubbles" } |> mq.Enqueue
  { Dog.Name = "Rufus" } |> mq.Enqueue
  mq.ClearAll() |> should equal 2
  mq.Dequeue<Cat>() |> should equal None
  mq.Dequeue<Dog>() |> should equal None

[<Test>]
let ``it can return a sequence of a type``() = 
  use mq = MessageQueue.create InMemory
  let cats = [ 1..10 ] |> List.map (fun i -> { Cat.Name = sprintf "%i" i })
  cats |> Seq.iter mq.Enqueue
  mq.DequeueAll<Cat>() |> should equal cats
  mq.Dequeue<Cat>() |> should equal None

[<Test>]
let ``it persists the queue``() = 
  let createMq _ = 
    Environment.CurrentDirectory
    |> File
    |> MessageQueue.create
  
  use mq = createMq()
  let cat = { Cat.Name = "Bubbles" }
  cat |> mq.Enqueue
  use mq = createMq()
  let cat = { Cat.Name = "Bubbles" }
  mq.Dequeue<Cat>()
  |> should equal
  <| Some cat

[<Test>]
let ``it peeks``() = 
  use mq = MessageQueue.create InMemory
  let cat = { Cat.Name = "Bubbles" }
  cat |> mq.Enqueue
  mq.Peek<Cat>()
  |> should equal
  <| Some cat
  mq.Peek<Cat>()
  |> should equal
  <| Some cat

[<Test>]
let ``it deletes``() = 
  use mq = MessageQueue.create InMemory
  let cat = { Cat.Name = "Bubbles" }
  cat |> mq.Enqueue
  mq.Delete<Cat>()
  mq.Dequeue<Cat>() |> should equal None