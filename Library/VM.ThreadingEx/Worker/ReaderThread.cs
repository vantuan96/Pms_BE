using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VM.ThreadingEx.Worker
{
    public abstract class ReaderThread
    {
        private Thread _ProcessingThread = null;
        /// <summary>
        /// Using default thread name
        /// </summary>
        public virtual void Start()
        {
            ThreadStart ts = new ThreadStart(this.Process);
            _ProcessingThread = new Thread(ts);
            _ProcessingThread.Start();
        }

        public virtual void Stop()
        {
            if (_ProcessingThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
            {
                _ProcessingThread.Interrupt();
            }
        }

        public abstract void Process();
    }
}
