
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;

namespace BonZeb
{
    public sealed class BonZebDataDisposable : ICancelable, IDisposable
    {
        IDisposable resource;

        public BonZebDataDisposable(StreamWriter writer, IDisposable disposable)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            Writer = writer;
            resource = disposable;
        }

        public StreamWriter Writer { get; private set; }

        public bool IsDisposed
        {
            get { return resource == null; }
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref resource, null);
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}