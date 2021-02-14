using System;
using System.Collections.Generic;
using System.Threading;
using FakeItEasy;
using LightInject.Interception;

using MetaFoo.Core.Adapters;
using MetaFoo.Core.Dynamic;
using MetaFoo.Core.Reflection;
using MetaFoo.Tests.Mocks;
using Optional;
using Optional.Unsafe;
using Xunit;

namespace MetaFoo.Tests
{
    public class DuckTypingTests
    {
        [Fact(DisplayName = "We should be able to duck type an interface type to a class with the same method signatures")]
        public void ShouldRouteInterfaceMethodCallsToAnObjectWithACompatibleMethodSignature()
        {
            var foo = new SampleDuckType();

            var duck = foo.CreateDuck<ISampleDuckInterface>();
            duck.DoSomething();

            Assert.True(foo.WasCalled);
        }

        [Fact(DisplayName = "We should be able to route an interface method call to any class that implements the IIInterceptor interface")]
        public void ShouldRouteInterfaceCallToInterceptorInstance()
        {
            var fakeInterceptor = A.Fake<IInterceptor>();
            var duck = fakeInterceptor.CreateDuck<ISampleDuckInterface>();
            duck.DoSomething();

            A.CallTo(() => fakeInterceptor.Invoke(A<IInvocationInfo>._)).MustHaveHappened();
        }

        [Fact(DisplayName = "We should be able to route an interface method call to any class that implements the IMethodInvoker interface")]
        public void ShouldRouteInterfaceCallToMethodInvokerInstance()
        {
            var invoker = A.Fake<IMethodInvoker>();
            var duck = invoker.CreateDuck<ISampleDuckInterface>();
            duck.DoSomething();

            A.CallTo(() => invoker.Invoke(A<Option<string>>._, A<object[]>._)).MustHaveHappened();
        }

        [Fact(DisplayName =
            "We should be able to route calls to a single method on an interface back to a delegate if the delegate has a compatible signature with the given method")]
        public void ShouldRouteInterfaceCallToDelegateIfMethodSignatureIsCompatible()
        {
            var items = new List<object>();

            Action targetAction = () => { items.Add(42); };

            var duck = targetAction.CreateDuck<ISampleDuckInterface>("DoSomething");
            duck.DoSomething();

            Assert.NotEmpty(items);
        }

        [Fact(DisplayName = "We should be able to determine whether or not a MetaObject can fulfill an interface contract")]
        public void ShouldBeAbleToDetermineIfAMetaObjectCanFulfillAnInterfaceContract()
        {
            var wasMethodCalled = new ManualResetEvent(false);

            Action methodBody = () => wasMethodCalled.Set();

            // Add the new method that fulfills the ISampleDuckInterface contract
            var foo = new MetaObject();
            foo.AddMethod("DoSomething",methodBody);
            
            Assert.True(foo.LooksLike<ISampleDuckInterface>());
        }
    }
}