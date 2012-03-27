using System;

namespace Project1.Core.Infrastructure
{
    class Scope : IDisposable
    {
        private readonly Action _dispose;

        public Scope(Action begin, Action dispose)
        {
            _dispose = dispose;
            begin();
        }

        public void Dispose()
        {
            _dispose();
        }
    }
}