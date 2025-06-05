namespace MockDataSystem
{
    
    public class MockService
    {
        private readonly IMockGenerator _generator;

        public MockService(IMockGenerator generator = null)
        {
            _generator = generator ?? new MockGenerator();
        }

        public T CreateMock<T>() where T : new()
        {
            return _generator.Generate<T>();
        }
    }
}