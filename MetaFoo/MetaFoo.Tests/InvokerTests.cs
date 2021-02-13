using System;
using System.Collections.Generic;
using System.IO;
using MetaFoo.Reflection;
using MetaFoo.Tests.Mocks;
using Xunit;

namespace MetaFoo.Tests
{
    public class InvokerTests
    {
        [Fact(DisplayName = "We should be able to invoke an instance method based the given arguments and the method name")]
        public void ShouldBeAbleToInvokeAnInstanceMethodBasedOnArguments()
        {
            var items = new List<int>();
            var instance = new MockClass();
            var invoker = new MethodInvoker(instance);
            invoker.Invoke(nameof(MockClass.AddItem), items);
            Assert.NotEmpty(items);
        }

        [Fact(DisplayName = "We should be able to invoke a static method based on the given arguments and the method name")]
        public void ShouldBeAbleToInvokeAStaticMethodBasedOnArguments()
        {
            var writer = new StringWriter();
            Console.SetOut(writer);

            typeof(Console).InvokeStatic("WriteLine", "Hello, World!");

            Assert.NotEmpty(writer.ToString());
        }

        [Fact(DisplayName = "We should be able to add extension methods (from a single extension type) as normal methods to an existing invoker and treat those extension methods like they were always a part of the original class")]
        public void ShouldBeAbleToAddExtensionMethodsFromASingleTypeAndTreatThemAsAdditionalInstanceMethods()
        {
            var items = new List<int>() {1, 2, 3};
            var instance = new MockClass();
            var invoker = new MethodInvoker(instance);

            invoker.AddExtensionMethodsFrom(typeof(MockClassExtensions));
            invoker.Invoke("Clear", items);

            Assert.Empty(items);
        }

        [Fact(DisplayName = "We should be able to add extension methods (from an entire assembly) as normal methods to an existing invoker and treat those extension methods like they were always a part of the original class")]
        public void ShouldBeAbleToAddExtensionMethodsFromAnAssemblyAndTreatThemAsAdditionalInstanceMethods()
        {
            var items = new List<int>() {1, 2, 3};
            var instance = new MockClass();
            var invoker = new MethodInvoker(instance);

            invoker.AddExtensionMethodsFrom(typeof(MockClassExtensions).Assembly);
            invoker.Invoke("Clear", items);

            Assert.Empty(items);
        }

        [Fact(DisplayName = "We should return Option.None<object> to indicate that a static method call returned nothing as a result of its execution")]
        public void ShouldReturnNoneWhenCallingAStaticMethodThatReturnsVoid()
        {
            var writer = new StringWriter();
            Console.SetOut(writer);

            var returnValue = typeof(Console).InvokeStatic("WriteLine", "Hello, World!");

            Assert.NotEmpty(writer.ToString());
            Assert.False(returnValue.HasValue);
        }

        [Fact(DisplayName = "We should return Option.None<object> to indicate that an instance method call returned nothing as a result of its execution")]
        public void ShouldReturnNoneWhenCallingAnInstanceMethodThatReturnsVoid()
        {
            var items = new List<int>();
            var instance = new MockClass();
            var invoker = new MethodInvoker(instance);
            var returnValue = invoker.Invoke(nameof(MockClass.AddItem), items);
            
            Assert.NotEmpty(items);
            Assert.False(returnValue.HasValue);
        }
    }
}