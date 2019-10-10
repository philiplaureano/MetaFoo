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
        [Fact]
        public void ShouldBeAbleToInvokeAnInstanceMethodBasedOnArguments()
        {
            var items = new List<int>();
            var instance = new MockClass();
            var invoker = new MethodInvoker(instance);
            invoker.Invoke(nameof(MockClass.AddItem), items);
            Assert.NotEmpty(items);
        }

        [Fact]
        public void ShouldBeAbleToInvokeAStaticMethodBasedOnArguments()
        {
            var writer = new StringWriter();
            Console.SetOut(writer);

            typeof(Console).InvokeStatic("WriteLine", "Hello, World!");

            Assert.NotEmpty(writer.ToString());
        }

        [Fact]
        public void ShouldBeAbleToAddExtensionMethodsFromASingleTypeAndTreatThemAsAdditionalInstanceMethods()
        {
            var items = new List<int>() {1, 2, 3};
            var instance = new MockClass();
            var invoker = new MethodInvoker(instance);
            
            invoker.AddExtensionMethodsFrom(typeof(MockClassExtensions));
            invoker.Invoke("Clear", items);
            
            Assert.Empty(items);
        }
        
        [Fact]        
        public void ShouldBeAbleToAddExtensionMethodsFromAnAssemblyAndTreatThemAsAdditionalInstanceMethods()
        {
            var items = new List<int>() {1, 2, 3};
            var instance = new MockClass();
            var invoker = new MethodInvoker(instance);
            
            invoker.AddExtensionMethodsFrom(typeof(MockClassExtensions).Assembly);
            invoker.Invoke("Clear", items);
            
            Assert.Empty(items);
        }
    }
}