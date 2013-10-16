using System;

namespace MefExtensions
{
    public class LazyFactory<T, TMetadata>
    {
        private readonly Func<T> CreateObjectFunc;

        public LazyFactory(Func<T> createObjectFunc, TMetadata metadata)
        {
            Metadata = metadata;
            CreateObjectFunc = createObjectFunc;
        }

        public TMetadata Metadata { get; private set; }

        public T Create()
        {
            return CreateObjectFunc();
        }
    }
}