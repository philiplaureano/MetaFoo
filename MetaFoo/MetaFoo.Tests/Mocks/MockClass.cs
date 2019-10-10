using System.Collections.Generic;

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