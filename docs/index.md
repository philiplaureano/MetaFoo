# MetaFoo.Core - A Library for Metaprogramming in .NET

## Overview

MetaFoo is the spiritual successor to [LinFu](https://www.codeproject.com/Articles/20884/Introducing-the-LinFu-Framework-Part-I-LinFu-Dynam), which was a library that I wrote in 2007 that added language extensions to C# just by using the reflection capabilities within the .NET Common Language Runtime.

It is a library that does a few of the things that I have learned with metaprogramming in .NET over the past decade, and puts it into one neat little Nuget package.

In a nutshell, it lets C# behave more like a dynamically typed language, such as JavaScript, all without having to leave the strong typing roots of C#. 

(**Note**: This library is meant to share a few of the things that I have picked up over the years, and it is not meant to be comprehensive, or enterprise ready in any sense. It's safe and open source, but use it at your own risk 😁)

## Features

- Dynamic Typing / MetaObjects (aka DynamicObjects on steroids)
  - Adding new methods to Dynamic Objects at runtime
    - This includes adding multiple overloads for the same method by treating dynamic objects as expando objects
    - This also includes using the MetaObject.AddMethod(...) method to add new methods at runtime, which means using the dynamic keyword is optional
    - Each newly added method body (aka delegate/func/action) can access the MetaObject 'self' parameter as the first parameter, which allows all method bodies to access the state of the object that it is currently executing against
  - Adding new property getters and setters to Dynamic Objects at runtime
    - Using Action&lt;T&gt; for setters, and Func&lt;T&gt; for getters
  - Duck typing/casting to an interface that is backed by MetaFoo's dynamic MetaObject 
    - Using the MetaObject.LooksLike&lt;T&gt;() method to test for similarity between the current meta object and the type you want it to look like
    - Using MetaObject.CreateDuck&lt;T&gt;() to cast it to an interface instance that 'looks like' the given type in question (provided that you add the requisite methods to match the target type signature)
 
- Late Binding / Reflection
  - Invoking static methods at runtime using only the type, method name, and runtime arguments
  - Invoking instance methods at runtime
  
- Duck Typing
  - Mapping interfaces to concrete classes with the same interface "shape"
  - Duck typing an interface to a dynamic interface proxy
  - Mapping interface types to malleable and expandable [Dynamic Objects](https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.dynamicobject?view=net-5.0)
  - Mapping a single method call from an interface type to a single backing Func<T> or Action<T> or Action delegate
  - **Planned**: Appending extension classes to an existing Meta Object, and using those extension methods to fulfill the runtime interface contract for the duck type. (This is similar to [how Go implements interfaces](https://golangbyexample.com/interface-in-golang/) )
  
- **Planned**: IL Rewriting
  - Method Call Redirection
  - Method Call Interception

- **Planned**: Reverse engineering a compiled type into a MetaObject so that it can be rewritten and modified like a dynamic type

## Installing the MetaFoo NuGet Package 
### Prerequisites
- MetaFoo:
  - [Runs on .NET Standard 2.0 compatible binaries](https://dotnet.microsoft.com/platform/dotnet-standard)
  - [Requires .NET 5 to build the source code](https://dotnet.microsoft.com/download/dotnet/5.0)
- You can find the repository for MetaFoo [here](https://github.com/philiplaureano/MetaFoo), as well as [the unit tests](https://github.com/philiplaureano/MetaFoo/tree/master/MetaFoo/MetaFoo.Tests) that show how to use its existing features. 

You can [download the package here](https://www.nuget.org/packages/Laureano.MetaFoo.Core/) from NuGet.

## License
 MetaFoo is [licensed under the MIT License](https://opensource.org/licenses/MIT). It comes with no warranties expressed or implied, whatsoever.
 
## Questions, Comments, or Feedback?
- Feel free to [follow me](http://twitter.com/philiplaureano) on Twitter.
