using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharpmisc.DesignPatterns.DisposePattern
{
    // Any disposable class inherits from IDisposable interface
    public class DisposableClass : IDisposable
    {
        private bool disposed;

        // Implement the inherited method
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DisposableClass()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposed)
            {
                return;
            }

            if(disposing)
            {
                // free other managed objects that implement
                // IDisposable only
            }

            // release any unmanaged objects
            // set the object references to null
            disposed = true;
        }
    }

    public class DerivedDisposableClass : DisposableClass
    {
        private bool _disposed;

        // a finalizer is not necessary, as it is inherited from
        // the base class

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // free other managed objects that implement
                    // IDisposable only
                }

                // release any unmanaged objects
                // set object references to null

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
