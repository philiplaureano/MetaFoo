using System.Collections.Generic;
using Xunit.Sdk;

namespace MetaFoo.Tests.Mocks
{
    public class MockClass
    {
        public void AddItem(List<int> items)
        {
            items.Add(1);
        }
    }
}