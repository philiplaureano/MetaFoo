using System;
using System.Collections.Generic;
using FakeItEasy;
using LightInject.Interception;
using MetaFoo.Adapters;
using MetaFoo.Reflection;
using MetaFoo.Tests.Mocks;
using Optional;
using Optional.Unsafe;
using Xunit;

namespace MetaFoo.Tests
{
    public class DuckTypingTests
    {
        [Fact]
        public void ShouldRouteInterfaceMethodCallsToAnObjectWithACompatibleMethodSignature()
        {
            var foo = new SampleDuckType();

            var duck = foo.CreateDuck<ISampleDuckInterface>();
            duck.DoSomething();

            Assert.True(foo.WasCalled);
        }

        [Fact]
        public void ShouldRouteInterfaceCallToInterceptorInstance()
        {
            var fakeInterceptor = A.Fake<IInterceptor>();
            var duck = fakeInterceptor.CreateDuck<ISampleDuckInterface>();
            duck.DoSomething();

            A.CallTo(() => fakeInterceptor.Invoke(A<IInvocationInfo>._)).MustHaveHappened();
        }

        [Fact]
        public void ShouldRouteInterfaceCallToMethodInvokerInstance()
        {
            var invoker = A.Fake<IMethodInvoker>();
            var duck = invoker.CreateDuck<ISampleDuckInterface>();
            duck.DoSomething();

            A.CallTo(() => invoker.Invoke(A<string>._, A<object[]>._)).MustHaveHappened();
        }

        [Fact]
        public void ShouldRouteInterfaceCallToDelegateIfMethodSignatureIsCompatible()
        {
            var items = new List<object>();

            Action targetAction = () => { items.Add(42); };

            var duck = targetAction.CreateDuck<ISampleDuckInterface>("DoSomething");
            duck.DoSomething();

            Assert.NotEmpty(items);
        }
    }
}