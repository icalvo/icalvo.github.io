---
layout: post
title: "Branching IEnumerables"
date: 2023-03-01
comments: true
categories: [software]
tags: [code,dotnet,csharp]
---

Let's imagine that we have an `IEnumerable<T>` source that represents a lengthy process, that generates a huge collection of data. This means that you don't want to iterate over that more than once, and also you don't want to materialize it completely in memory.

However, you want to do several processes with that collection that require the whole collection. For example, two different `GroupBy()` calls.

The solution is connecting the source with the consuming process with some consumer-producer solution. .NET provides us with a simple one, `BlockingCollection<T>`. So let's suppose we have a source and some process we want to run on it:

```csharp
IEnumerable<T> bigSource = ...;

void Process1(IEnumerable<T> source) { ... }
void Process2(IEnumerable<T> source) { ... }
```

Then we can solve our problem like this:

```csharp
var q1 = new BlockingCollection<T>();
var q2 = new BlockingCollection<T>();

// Start all consumers
var t1 = Task.Run(() => Process1(q1.GetConsumingEnumerable()));
var t2 = Task.Run(() => Process2(q2.GetConsumingEnumerable()));

// Feed the queues
foreach (var item in bigSource)
{
    q1.Add(item);
    q2.Add(item);
}

// Wait for all consumers
await Task.WhenAll(t1, t2);
```

That solves the problem but it's a bit verbose. Can we aim for a better API?

```csharp
var consumerTasks = bigSource
    .Branch(Process1)
    .Branch(Process2)
    .Start();

// Wait for all consumers
await consumerTasks;
```

If the processes are of the same kind (all async or not), we can also implement a shortcut:


```csharp
await bigSource.BranchStart(Process1, Process2);
```

We should support both `IEnumerable<T>` and `IAsyncEnumerable<T>` sources, and also body sync and async consumers.
OK, so let's start implementing. First we need an interface that provide the `Branch()` and `Start()` operation:

```csharp
public interface IBrancher<T>
{
    IBrancher<T> Branch(Action<IEnumerable<T>> action);
    IBrancher<T> Branch(Func<IEnumerable<T>, Task> action);
    Task Start();
}
```

T