using System;
using System.Diagnostics;

namespace LoadTesting
{
    class ProcessRunner : IDisposable
    {
        private readonly Process _process;
        private bool disposedValue;

        public ProcessRunner(string command, string argsuments)
        {
            _process = Process.Start(command, argsuments);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _process.Kill();
                    _process.WaitForExit();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}