using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;
using MetaFoo.Core.Adapters;
using MetaFoo.Tests.Mocks;
using Xunit;
using MetaFoo.Core.Dynamic;
using MetaFoo.Core.Reflection;

namespace MetaFoo.Tests
{
    public class DynamicObjectTests
    {
        [Fact(DisplayName = "We should be able to create new methods using Action<T> delegates")]
        public void ShouldBeAbleToAddNewMethodsFromDelegates()
        {
            var writtenLines = new List<string>();
            Action<string> writeLine = text => { writtenLines.Add(text); };

            dynamic foo = new MetaObject();
            foo.WriteLine = writeLine;
            foo.WriteLine("Hello, World!");

            Assert.Single(writtenLines, text => text == "Hello, World!");
        }

        [Fact(DisplayName =
            "We should be able to add and distinguish between two new method overloads with the same method name")]
        public void ShouldBeAbleToAddNewMethodOverloadsFromDelegates()
        {
            var writtenLines = new List<string>();
            Action<string> writeLine1 = text => { writtenLines.Add(text); };
            Action<string, string> writeLine2 = (arg1, arg2) => { writtenLines.Add($"{arg1} {arg2}"); };

            dynamic foo = new MetaObject();
            foo.WriteLine = writeLine1;
            foo.WriteLine = writeLine2;
            foo.WriteLine("Hello, World!");
            foo.WriteLine("Hi", "Everyone!");

            Assert.Contains(writtenLines, text => text == "Hello, World!");
            Assert.Contains(writtenLines, text => text == "Hi Everyone!");
        }

        [Fact(DisplayName = "We should be able to cast a DynamicObject to any given interface")]
        public void ShouldBeAbleToCastToADuckTypedInterface()
        {
            var items = new List<int>();
            Action doSomething = () =>
            {
                // If the action is successful, then it should populate this list
                items.Add(42);
            };

            dynamic foo = new MetaObject();
            foo.DoSomething = doSomething;

            ISampleDuckInterface sampleDuckInterface = foo;
            sampleDuckInterface.DoSomething();

            Assert.Single(items, item => item == 42);
        }

        [Fact(DisplayName = "We should be able to create property getters using DynamicObjects")]
        public void ShouldBeAbleToCreateGetter()
        {
            Func<int> getValue = () => 42;

            dynamic foo = new MetaObject();
            foo.get_Value = getValue;

            int result = foo.Value;
            Assert.Equal(42, result);
        }

        [Fact(DisplayName = "We should be able to create property setters using DynamicObjects")]
        public void ShouldBeAbleToCreateSetter()
        {
            var items = new List<string>();
            Action<string> setValue = arg0 => { items.Add(arg0); };

            dynamic foo = new MetaObject();
            foo.set_Value = setValue;

            foo.Value = "42";
            Assert.Single(items, item => item == "42");
        }

        [Fact(DisplayName =
            "We should be able to redirect both property getter and setter calls on an interface back to the DynamicObject")]
        public void ShouldBeAbleToDuckTypeToAnInterfaceWithProperties()
        {
            var wasGetterCalled = new ManualResetEvent(false);
            var wasSetterCalled = new ManualResetEvent(false);

            var valueStack = new ConcurrentStack<int>();
            Action<int> setValue = newValue =>
            {
                Assert.Empty(valueStack);
                wasSetterCalled.Set();

                valueStack.Push(newValue);
            };

            Func<int> getValue = () =>
            {
                Assert.Single(valueStack);
                wasGetterCalled.Set();

                Assert.True(valueStack.TryPeek(out var result));
                return result;
            };

            var metaObject = new MetaObject();

            dynamic foo = metaObject;
            foo.set_Value = setValue;
            foo.get_Value = getValue;

            var duckType = metaObject.CreateDuck<ISampleInterfaceWithProperties>();
            var expectedValue = 42;
            duckType.Value = expectedValue;
            Assert.Equal(expectedValue, duckType.Value);

            Assert.True(wasGetterCalled.WaitOne());
            Assert.True(wasSetterCalled.WaitOne());
        }

        [Fact(DisplayName = "We should be able to add new methods just by using the MetaObject.AddMethod() method")]
        public void ShouldBeAbleToAddMethodWithoutUsingDynamicKeyword()
        {
            var wasMethodCalled = new ManualResetEvent(false);

            // Use the Action delegate as the method body
            Action methodBody = () => wasMethodCalled.Set();

            var metaObject = new MetaObject();
            metaObject.AddMethod("DoSomething", methodBody);

            dynamic foo = metaObject;
            foo.DoSomething();

            Assert.True(wasMethodCalled.WaitOne());
        }

        [Fact(DisplayName =
            "We should be able to pass the instance of the MetaObject to any newly added methods to make it easier for them to manage their own state")]
        public void ShouldBeAbleToPassTheDynamicObjectInstanceToAddedMethodsAsAnOptionalParameter()
        {
            var wasMethodCalled = new ManualResetEvent(false);

            // Use the Action delegate as the method body
            Action<DynamicObject> methodBody = _ => wasMethodCalled.Set();

            var metaObject = new MetaObject();
            metaObject.AddMethod("DoSomething", methodBody);

            dynamic foo = metaObject;
            foo.DoSomething();

            Assert.True(wasMethodCalled.WaitOne(TimeSpan.FromMilliseconds(100)));
        }

        [Fact(DisplayName =
            "We should be able to call CreateDuck<T> from the MetaObject and create duck typed interface instances")]
        public void ShouldBeAbleToCreateDuckTypesFromTheMetaObjectItself()
        {
            var wasMethodCalled = new ManualResetEvent(false);
            var metaObject = new MetaObject();
            metaObject.AddMethod<Func<int>>("get_Value", () =>
            {
                wasMethodCalled.Set();
                return 42;
            });
            
            var duck = metaObject.CreateDuck<ISampleInterfaceWithProperties>();
            Assert.Equal(42, duck.Value);
            
            Assert.True(wasMethodCalled.WaitOne(TimeSpan.FromMilliseconds(100)));
        }
    }
}