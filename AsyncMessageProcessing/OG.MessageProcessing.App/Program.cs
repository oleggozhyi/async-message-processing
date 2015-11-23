﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OG.MessageProcessing.Persisting;
using OG.MessageProcessing.Processing;
using OG.MessageProcessing.Utils;

namespace OG.MessageProcessing.App
{

    class Program
    {
        static void Main(string[] args)
        {
            new TestCase
            {
                Times = 11,
                MessagesCount = 3000,
                Options = new AsyncMessageProcessorOptions
                {
                    LoopStrategy = MessageLoopStrategy.SequentialStrategy,
                }
            }.Run();
        }
    }

    public class TestCase
    { 
        public int Times { get; set; } = 10;
        public bool LargeContent { get; set; } = false;
        public int MessagesCount { get; set; } = 3000;
        public int IntervalMs { get; set; } = 0;
        public AsyncMessageProcessorOptions Options { get; set; } = AsyncMessageProcessorOptions.Default;
        public string ContentFormat { get; set; } = FileMessagePersisterSettings.Default.ContentFormat;

        private void Startup()
        {
            if (Directory.Exists(FileMessagePersisterSettings.Default.RootDirectory))
                Directory.Delete(FileMessagePersisterSettings.Default.RootDirectory, true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private string TestCaseName(int count)
        {
            string size = LargeContent ?"big" : "small";
            return $"Processing {MessagesCount} {size} items with {IntervalMs}ms interval - Run #{count}";
        }

        private void Measure(Action a, int count)
        {
            string testCaseNane = TestCaseName(count);
            Console.WriteLine($"Starting test case <{testCaseNane}>");
            var sw = Stopwatch.StartNew();
            a();
            sw.Stop();
            Console.WriteLine($"Completed test case <{testCaseNane}>");
            Console.WriteLine($"Elapsed = {sw.Elapsed}\n\n");
        }

        public void Run()
        {
            Console.WriteLine("========================================================================");
            for (int i = 1; i <= Times; i++)
            {
                Start(i);
            }
            Console.WriteLine("========================================================================\n");
        }

        private void Start(int count)
        {
            Startup();

            var settings = new FileMessagePersisterSettings();
            if (LargeContent)
                settings.ContentFormat = BigFormat;

            var handler = new FileMessagePersister(settings);
            var processor = new AsyncMessageProcessor(handler, Options);

            Measure(() =>
            {
                for (int i = 0; i < MessagesCount; i++)
                {
                    processor.Add($"msg{i}");
                    if (IntervalMs > 0)
                        Thread.Sleep(IntervalMs);
                }

                processor.WaitFor(_ =>
                        Directory.GetFiles(settings.RootDirectory, "*.log", SearchOption.AllDirectories).Length == MessagesCount, 
                                           msPollInterval: 100, msTimeout:60000, noLogs:true)
                                 .Wait();

            }, count);
        }

    #region blob
        public const string BigFormat =
            @"{Data}{Timestamp}
Functional languages are very different from standard imperative languages, and can be quite tricky to get the hang of initially. This page offers some tips on how to learn F# effectively.

Approach learning F# as a beginner

If you have experience in languages such as C# and Java, you have probably found that you can get a pretty good understanding of source code written in other similar languages, even if you aren't familiar with the keywords or the libraries. This is because all imperative languages use the same way of thinking, and experience in one language can be easily transferred to another.

If you are like many people, your standard approach to learning a new programming language is to find out how to implement concepts you are already familiar with. You might ask ""how do I assign a variable?"" or ""how do I do a loop?"", and with these answers be able to do some basic programming quite quickly.

When learning F#, you should not try to bring your old imperative concepts with you. In a pure functional language there are no variables, there are no loops, and there are no objects!

Yes, F# is a hybrid language and does support these concepts. But you will learn much faster if you start with a beginners mind.

Change the way you think

It is important to understand that functional programming is not just a stylistic difference; it is a completely different way of thinking about programming, in the way that truly object-oriented programming (in Smalltalk say) is also a different way of thinking from a traditional imperative language such as C.

F# does allow non-functional styles, and it is tempting to retain the habits you already are familiar with. You could just use F# in a non-functional way without really changing your mindset, and not realize what you are missing. To get the most out of F#, and to be fluent and comfortable with functional programming in general, it is critical that you think functionally, not imperatively.

By far the most important thing you can do is to take the time and effort to understand exactly how F# works, especially the core concepts involving functions and the type system. So please read and reread the series ""thinking functionally"" and ""understanding F# types"", play with the examples, and get comfortable with the ideas before you try to start doing serious coding. If you don’t understand how functions and types work, then you will have a hard time being productive.

Dos and Don'ts

Here is a list of dos and don'ts that will encourage you to think functionally. These will be hard at first, but just like learning a foreign language, you have to dive in and force yourself to speak like the locals.
•Don't use the mutable keyword at all as a beginner. Coding complex functions without the crutch of mutable state will really force you to understand the functional paradigm.
•Don't use for loops or if-then-else. Use pattern matching for testing booleans and recursing through lists.
•Don't use ""dot notation"". Instead of ""dotting into"" objects, try to use functions for everything. That is, write String.length ""hello"" rather than ""hello"".Length. It might seem like extra work, but this way of working is essential when using pipes and higher order functions like List.map. And don't write your own methods either! See this post for details.
•As a corollary, don't create classes. Use only the pure F# types such as tuples, records and unions.
•Don't use the debugger. If you have relied on the debugger to find and fix incorrect code, you will get a nasty shock. In F#, you will probably not get that far, because the compiler is so much stricter in many ways. And of course, there is no tool to “debug” the compiler and step through its processing. The best tool for debugging compiler errors is your brain, and F# forces you to use it!

On the other hand:
•Do create lots of ""little types"", especially union types. They are lightweight and easy, and their use will help document your domain model and ensure correctness.
•Do understand the list and seq types and their associated library modules. Functions like List.fold and List.map are very powerful. Once you understand how to use them, you will be well on your way to understanding higher order functions in general.
•Once you understand the collection modules, try to avoid recursion. Recursion can be error prone, and it can be hard to make sure that it is properly tail-recursive. When you use List.fold, you can never have that problem.
•Do use pipe (|>) and composition (>>) as much as you can. This style is much more idiomatic than nested function calls like f(g(x))
•Do understand how partial application works, and try to become comfortable with point-free (tacit) style.
•Do develop code incrementally, using the interactive window to test code fragments. If you blindly create lots of code and then try to compile it all at once, you may end up with many painful and hard-to-debug compilation errors.

Troubleshooting

There are a number of extremely common errors that beginners make, and if you are frustrated about getting your code to compile, please read the ""troubleshooting F#"" page.
Functional languages are very different from standard imperative languages, and can be quite tricky to get the hang of initially. This page offers some tips on how to learn F# effectively.

Approach learning F# as a beginner

If you have experience in languages such as C# and Java, you have probably found that you can get a pretty good understanding of source code written in other similar languages, even if you aren't familiar with the keywords or the libraries. This is because all imperative languages use the same way of thinking, and experience in one language can be easily transferred to another.

If you are like many people, your standard approach to learning a new programming language is to find out how to implement concepts you are already familiar with. You might ask ""how do I assign a variable?"" or ""how do I do a loop?"", and with these answers be able to do some basic programming quite quickly.

When learning F#, you should not try to bring your old imperative concepts with you. In a pure functional language there are no variables, there are no loops, and there are no objects!

Yes, F# is a hybrid language and does support these concepts. But you will learn much faster if you start with a beginners mind.

Change the way you think

It is important to understand that functional programming is not just a stylistic difference; it is a completely different way of thinking about programming, in the way that truly object-oriented programming (in Smalltalk say) is also a different way of thinking from a traditional imperative language such as C.

F# does allow non-functional styles, and it is tempting to retain the habits you already are familiar with. You could just use F# in a non-functional way without really changing your mindset, and not realize what you are missing. To get the most out of F#, and to be fluent and comfortable with functional programming in general, it is critical that you think functionally, not imperatively.

By far the most important thing you can do is to take the time and effort to understand exactly how F# works, especially the core concepts involving functions and the type system. So please read and reread the series ""thinking functionally"" and ""understanding F# types"", play with the examples, and get comfortable with the ideas before you try to start doing serious coding. If you don’t understand how functions and types work, then you will have a hard time being productive.

Dos and Don'ts

Here is a list of dos and don'ts that will encourage you to think functionally. These will be hard at first, but just like learning a foreign language, you have to dive in and force yourself to speak like the locals.
•Don't use the mutable keyword at all as a beginner. Coding complex functions without the crutch of mutable state will really force you to understand the functional paradigm.
•Don't use for loops or if-then-else. Use pattern matching for testing booleans and recursing through lists.
•Don't use ""dot notation"". Instead of ""dotting into"" objects, try to use functions for everything. That is, write String.length ""hello"" rather than ""hello"".Length. It might seem like extra work, but this way of working is essential when using pipes and higher order functions like List.map. And don't write your own methods either! See this post for details.
•As a corollary, don't create classes. Use only the pure F# types such as tuples, records and unions.
•Don't use the debugger. If you have relied on the debugger to find and fix incorrect code, you will get a nasty shock. In F#, you will probably not get that far, because the compiler is so much stricter in many ways. And of course, there is no tool to “debug” the compiler and step through its processing. The best tool for debugging compiler errors is your brain, and F# forces you to use it!

On the other hand:
•Do create lots of ""little types"", especially union types. They are lightweight and easy, and their use will help document your domain model and ensure correctness.
•Do understand the list and seq types and their associated library modules. Functions like List.fold and List.map are very powerful. Once you understand how to use them, you will be well on your way to understanding higher order functions in general.
•Once you understand the collection modules, try to avoid recursion. Recursion can be error prone, and it can be hard to make sure that it is properly tail-recursive. When you use List.fold, you can never have that problem.
•Do use pipe (|>) and composition (>>) as much as you can. This style is much more idiomatic than nested function calls like f(g(x))
•Do understand how partial application works, and try to become comfortable with point-free (tacit) style.
•Do develop code incrementally, using the interactive window to test code fragments. If you blindly create lots of code and then try to compile it all at once, you may end up with many painful and hard-to-debug compilation errors.

Troubleshooting

There are a number of extremely common errors that beginners make, and if you are frustrated about getting your code to compile, please read the ""troubleshooting F#"" page.
Functional languages are very different from standard imperative languages, and can be quite tricky to get the hang of initially. This page offers some tips on how to learn F# effectively.

Approach learning F# as a beginner

If you have experience in languages such as C# and Java, you have probably found that you can get a pretty good understanding of source code written in other similar languages, even if you aren't familiar with the keywords or the libraries. This is because all imperative languages use the same way of thinking, and experience in one language can be easily transferred to another.

If you are like many people, your standard approach to learning a new programming language is to find out how to implement concepts you are already familiar with. You might ask ""how do I assign a variable?"" or ""how do I do a loop?"", and with these answers be able to do some basic programming quite quickly.

When learning F#, you should not try to bring your old imperative concepts with you. In a pure functional language there are no variables, there are no loops, and there are no objects!

Yes, F# is a hybrid language and does support these concepts. But you will learn much faster if you start with a beginners mind.

Change the way you think

It is important to understand that functional programming is not just a stylistic difference; it is a completely different way of thinking about programming, in the way that truly object-oriented programming (in Smalltalk say) is also a different way of thinking from a traditional imperative language such as C.

F# does allow non-functional styles, and it is tempting to retain the habits you already are familiar with. You could just use F# in a non-functional way without really changing your mindset, and not realize what you are missing. To get the most out of F#, and to be fluent and comfortable with functional programming in general, it is critical that you think functionally, not imperatively.

By far the most important thing you can do is to take the time and effort to understand exactly how F# works, especially the core concepts involving functions and the type system. So please read and reread the series ""thinking functionally"" and ""understanding F# types"", play with the examples, and get comfortable with the ideas before you try to start doing serious coding. If you don’t understand how functions and types work, then you will have a hard time being productive.

Dos and Don'ts

Here is a list of dos and don'ts that will encourage you to think functionally. These will be hard at first, but just like learning a foreign language, you have to dive in and force yourself to speak like the locals.
•Don't use the mutable keyword at all as a beginner. Coding complex functions without the crutch of mutable state will really force you to understand the functional paradigm.
•Don't use for loops or if-then-else. Use pattern matching for testing booleans and recursing through lists.
•Don't use ""dot notation"". Instead of ""dotting into"" objects, try to use functions for everything. That is, write String.length ""hello"" rather than ""hello"".Length. It might seem like extra work, but this way of working is essential when using pipes and higher order functions like List.map. And don't write your own methods either! See this post for details.
•As a corollary, don't create classes. Use only the pure F# types such as tuples, records and unions.
•Don't use the debugger. If you have relied on the debugger to find and fix incorrect code, you will get a nasty shock. In F#, you will probably not get that far, because the compiler is so much stricter in many ways. And of course, there is no tool to “debug” the compiler and step through its processing. The best tool for debugging compiler errors is your brain, and F# forces you to use it!

On the other hand:
•Do create lots of ""little types"", especially union types. They are lightweight and easy, and their use will help document your domain model and ensure correctness.
•Do understand the list and seq types and their associated library modules. Functions like List.fold and List.map are very powerful. Once you understand how to use them, you will be well on your way to understanding higher order functions in general.
•Once you understand the collection modules, try to avoid recursion. Recursion can be error prone, and it can be hard to make sure that it is properly tail-recursive. When you use List.fold, you can never have that problem.
•Do use pipe (|>) and composition (>>) as much as you can. This style is much more idiomatic than nested function calls like f(g(x))
•Do understand how partial application works, and try to become comfortable with point-free (tacit) style.
•Do develop code incrementally, using the interactive window to test code fragments. If you blindly create lots of code and then try to compile it all at once, you may end up with many painful and hard-to-debug compilation errors.

Troubleshooting

There are a number of extremely common errors that beginners make, and if you are frustrated about getting your code to compile, please read the ""troubleshooting F#"" page.
Functional languages are very different from standard imperative languages, and can be quite tricky to get the hang of initially. This page offers some tips on how to learn F# effectively.

Approach learning F# as a beginner

If you have experience in languages such as C# and Java, you have probably found that you can get a pretty good understanding of source code written in other similar languages, even if you aren't familiar with the keywords or the libraries. This is because all imperative languages use the same way of thinking, and experience in one language can be easily transferred to another.

If you are like many people, your standard approach to learning a new programming language is to find out how to implement concepts you are already familiar with. You might ask ""how do I assign a variable?"" or ""how do I do a loop?"", and with these answers be able to do some basic programming quite quickly.

When learning F#, you should not try to bring your old imperative concepts with you. In a pure functional language there are no variables, there are no loops, and there are no objects!

Yes, F# is a hybrid language and does support these concepts. But you will learn much faster if you start with a beginners mind.

Change the way you think

It is important to understand that functional programming is not just a stylistic difference; it is a completely different way of thinking about programming, in the way that truly object-oriented programming (in Smalltalk say) is also a different way of thinking from a traditional imperative language such as C.

F# does allow non-functional styles, and it is tempting to retain the habits you already are familiar with. You could just use F# in a non-functional way without really changing your mindset, and not realize what you are missing. To get the most out of F#, and to be fluent and comfortable with functional programming in general, it is critical that you think functionally, not imperatively.

By far the most important thing you can do is to take the time and effort to understand exactly how F# works, especially the core concepts involving functions and the type system. So please read and reread the series ""thinking functionally"" and ""understanding F# types"", play with the examples, and get comfortable with the ideas before you try to start doing serious coding. If you don’t understand how functions and types work, then you will have a hard time being productive.

Dos and Don'ts

Here is a list of dos and don'ts that will encourage you to think functionally. These will be hard at first, but just like learning a foreign language, you have to dive in and force yourself to speak like the locals.
•Don't use the mutable keyword at all as a beginner. Coding complex functions without the crutch of mutable state will really force you to understand the functional paradigm.
•Don't use for loops or if-then-else. Use pattern matching for testing booleans and recursing through lists.
•Don't use ""dot notation"". Instead of ""dotting into"" objects, try to use functions for everything. That is, write String.length ""hello"" rather than ""hello"".Length. It might seem like extra work, but this way of working is essential when using pipes and higher order functions like List.map. And don't write your own methods either! See this post for details.
•As a corollary, don't create classes. Use only the pure F# types such as tuples, records and unions.
•Don't use the debugger. If you have relied on the debugger to find and fix incorrect code, you will get a nasty shock. In F#, you will probably not get that far, because the compiler is so much stricter in many ways. And of course, there is no tool to “debug” the compiler and step through its processing. The best tool for debugging compiler errors is your brain, and F# forces you to use it!

On the other hand:
•Do create lots of ""little types"", especially union types. They are lightweight and easy, and their use will help document your domain model and ensure correctness.
•Do understand the list and seq types and their associated library modules. Functions like List.fold and List.map are very powerful. Once you understand how to use them, you will be well on your way to understanding higher order functions in general.
•Once you understand the collection modules, try to avoid recursion. Recursion can be error prone, and it can be hard to make sure that it is properly tail-recursive. When you use List.fold, you can never have that problem.
•Do use pipe (|>) and composition (>>) as much as you can. This style is much more idiomatic than nested function calls like f(g(x))
•Do understand how partial application works, and try to become comfortable with point-free (tacit) style.
•Do develop code incrementally, using the interactive window to test code fragments. If you blindly create lots of code and then try to compile it all at once, you may end up with many painful and hard-to-debug compilation errors.

Troubleshooting

There are a number of extremely common errors that beginners make, and if you are frustrated about getting your code to compile, please read the ""troubleshooting F#"" page.
Functional languages are very different from standard imperative languages, and can be quite tricky to get the hang of initially. This page offers some tips on how to learn F# effectively.

Approach learning F# as a beginner

If you have experience in languages such as C# and Java, you have probably found that you can get a pretty good understanding of source code written in other similar languages, even if you aren't familiar with the keywords or the libraries. This is because all imperative languages use the same way of thinking, and experience in one language can be easily transferred to another.

If you are like many people, your standard approach to learning a new programming language is to find out how to implement concepts you are already familiar with. You might ask ""how do I assign a variable?"" or ""how do I do a loop?"", and with these answers be able to do some basic programming quite quickly.

When learning F#, you should not try to bring your old imperative concepts with you. In a pure functional language there are no variables, there are no loops, and there are no objects!

Yes, F# is a hybrid language and does support these concepts. But you will learn much faster if you start with a beginners mind.

Change the way you think

It is important to understand that functional programming is not just a stylistic difference; it is a completely different way of thinking about programming, in the way that truly object-oriented programming (in Smalltalk say) is also a different way of thinking from a traditional imperative language such as C.

F# does allow non-functional styles, and it is tempting to retain the habits you already are familiar with. You could just use F# in a non-functional way without really changing your mindset, and not realize what you are missing. To get the most out of F#, and to be fluent and comfortable with functional programming in general, it is critical that you think functionally, not imperatively.

By far the most important thing you can do is to take the time and effort to understand exactly how F# works, especially the core concepts involving functions and the type system. So please read and reread the series ""thinking functionally"" and ""understanding F# types"", play with the examples, and get comfortable with the ideas before you try to start doing serious coding. If you don’t understand how functions and types work, then you will have a hard time being productive.

Dos and Don'ts

Here is a list of dos and don'ts that will encourage you to think functionally. These will be hard at first, but just like learning a foreign language, you have to dive in and force yourself to speak like the locals.
•Don't use the mutable keyword at all as a beginner. Coding complex functions without the crutch of mutable state will really force you to understand the functional paradigm.
•Don't use for loops or if-then-else. Use pattern matching for testing booleans and recursing through lists.
•Don't use ""dot notation"". Instead of ""dotting into"" objects, try to use functions for everything. That is, write String.length ""hello"" rather than ""hello"".Length. It might seem like extra work, but this way of working is essential when using pipes and higher order functions like List.map. And don't write your own methods either! See this post for details.
•As a corollary, don't create classes. Use only the pure F# types such as tuples, records and unions.
•Don't use the debugger. If you have relied on the debugger to find and fix incorrect code, you will get a nasty shock. In F#, you will probably not get that far, because the compiler is so much stricter in many ways. And of course, there is no tool to “debug” the compiler and step through its processing. The best tool for debugging compiler errors is your brain, and F# forces you to use it!

On the other hand:
•Do create lots of ""little types"", especially union types. They are lightweight and easy, and their use will help document your domain model and ensure correctness.
•Do understand the list and seq types and their associated library modules. Functions like List.fold and List.map are very powerful. Once you understand how to use them, you will be well on your way to understanding higher order functions in general.
•Once you understand the collection modules, try to avoid recursion. Recursion can be error prone, and it can be hard to make sure that it is properly tail-recursive. When you use List.fold, you can never have that problem.
•Do use pipe (|>) and composition (>>) as much as you can. This style is much more idiomatic than nested function calls like f(g(x))
•Do understand how partial application works, and try to become comfortable with point-free (tacit) style.
•Do develop code incrementally, using the interactive window to test code fragments. If you blindly create lots of code and then try to compile it all at once, you may end up with many painful and hard-to-debug compilation errors.

Troubleshooting

There are a number of extremely common errors that beginners make, and if you are frustrated about getting your code to compile, please read the ""troubleshooting F#"" page.
Functional languages are very different from standard imperative languages, and can be quite tricky to get the hang of initially. This page offers some tips on how to learn F# effectively.
";
#endregion
    }
}
