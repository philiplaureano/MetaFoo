using System;
using System.Collections.Generic;

namespace MetaFoo.Tests.Mocks
{
    public static class MockClassExtensions
    {
        public static void Clear(this MockClass mock, List<int> items)
        {
            if (mock == null) 
                throw new ArgumentNullException(nameof(mock));
            
            items.Clear();
        }
    }
}