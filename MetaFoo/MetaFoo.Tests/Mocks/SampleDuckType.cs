namespace MetaFoo.Tests.Mocks
{
    public class SampleDuckType
    {
        public void DoSomething()
        {
            WasCalled = true;
        }

        public bool WasCalled { get; set; }
    }
}