using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MetaFoo.Tests.Mocks;
using Xunit;

namespace MetaFoo.Tests
{
    public class DynamicObjectTests
    {
        [Fact]
        public void ShouldBeAbleToAddNewMethodsFromDelegates()
        {
            var writtenLines = new List<string>();
            Action<string> writeLine = text => { writtenLines.Add(text); };

            dynamic foo = new Dynamic.DynamicObject();
            foo.WriteLine = writeLine;
            foo.WriteLine("Hello, World!");

            Assert.Single(writtenLines, text => text == "Hello, World!");
        }

        [Fact]
        public void ShouldBeAbleToAddNewMethodOverloadsFromDelegates()
        {
            var writtenLines = new List<string>();
            Action<string> writeLine1 = text => { writtenLines.Add(text); };
            Action<string, string> writeLine2 = (arg1, arg2) => { writtenLines.Add($"{arg1} {arg2}"); };

            dynamic foo = new Dynamic.DynamicObject();
            foo.WriteLine = writeLine1;
            foo.WriteLine = writeLine2;
            foo.WriteLine("Hello, World!");
            foo.WriteLine("Hi", "Everyone!");

            Assert.Contains(writtenLines, text => text == "Hello, World!");
            Assert.Contains(writtenLines, text => text == "Hi Everyone!");
        }

        [Fact]
        public void ShouldBeAbleToCastToADuckTypedInterface()
        {
            var items = new List<int>();
            Action doSomething = () =>
            {
                // If the action is successful, then it should populate this list
                items.Add(42);
            };

            dynamic foo = new Dynamic.DynamicObject();
            foo.DoSomething = doSomething;

            ISampleDuckInterface sampleDuckInterface = foo;
            sampleDuckInterface.DoSomething();

            Assert.Single(items, item => item == 42);
        }

        [Fact]
        public void ShouldBeAbleToCreateGetter()
        {
            Func<int> getValue = () => 42;

            dynamic foo = new Dynamic.DynamicObject();
            foo.get_Value = getValue;

            int result = foo.Value;
            Assert.Equal(42, result);
        }

        [Fact]
        public void ShouldBeAbleToCreateSetter()
        {
            var items = new List<string>();
            Action<string> setValue = arg0 => { items.Add(arg0); };

            dynamic foo = new Dynamic.DynamicObject();
            foo.set_Value = setValue;

            foo.Value = "42";
            Assert.Single(items, item => item == "42");
        }
    }
}