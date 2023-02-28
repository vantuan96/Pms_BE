using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VM.ThreadingEx.Worker
{
    /// <summary>    
    /// Implements the base processing thread which does a repetitive processing in
    /// a cycle and can be started in a thread and stopped in more
    /// controlled manner than Thread class.
    /// Provides final exception reporting (if there is any).
    /// </summary>
    public abstract class ProcessingThread
    {
        /// <summary>
        /// The name for the thread as displayed in the debug file.
        /// </summary>
        private const String PROCESSING_THREAD_NAME = "ProcThread";

        /// <summary>        
        /// The processing thread is in initialisation phase.
        /// It's the phase after calling <code>ProcessingThread.Start</code> method
        /// but before entering while-loop in the <code>ProcessingThread.Run</code> method.        
        /// <see cref="ProcessingThread._ProcessingStatus"/>
        /// <see cref="ProcessingThread.IsInitialising()"/>        
        /// </summary>
        private const byte PROC_INITIALISING = 0;

        /// <summary>        
        /// The processing thread is in running phase.
        /// It's the phase when the thread is in the while-loop
        /// in the <code>ProcessingThread.Process</code> method or method called from the loop
        /// that method.        
        /// <see cref="ProcessingThread._ProcessingStatus"/> 
        /// <see cref="ProcessingThread.IsProcessing()"/>
        /// <see cref="ProcessingThread.Run()"/>        
        /// </summary>
        private const byte PROC_RECEIVING = 1;

        /// <summary>        
        /// The processing thread is finished.
        /// The finished phase is phase when the thread has exited the while-loop
        /// in the <code>ProcessingThread.Run</code> method. It is possible to run it again
        /// by calling <code>ProcessingThread.Start</code> method again.
        ///<see cref="ProcessingThread._ProcessingStatus"/>
        /// <see cref="ProcessingThread.IsFinished()"/>        
        /// </summary>
        private const byte PROC_FINISHED = 2;

        /// <summary>
        /// Object for monitoring the access to the <code>ProcessingThread.ProcessingStatus</code> variable.
        /// </summary>
        private object _ProcessingStatusLock = new object();

        /// <summary>        
        /// State variable indicating status of the thread. Don't confuse with
        /// <code>ProcessingThread._KeepProcessing</code> which stops the main loop, but
        /// can be set to <code>false</code> far before the end of the loop
        /// using <code>ProcessingThread.StopProcessing</code> method.
        /// <see cref="ProcessingThread._KeepProcessing"/>
        /// <see cref="ProcessingThread.Run()"/>
        /// <see cref="ProcessingThread.Start()"/>
        /// <see cref="ProcessingThread.Stop()"/>
        /// <see cref="ProcessingThread.SetProcessingStatus()"/>
        /// <see cref="ProcessingThread.IsInitialising()"/>
        /// <see cref="ProcessingThread.IsProcessing()"/>
        /// <see cref="ProcessingThread.IsFinished()"/>        
        /// </summary>
        private byte _ProcessingStatus = PROC_INITIALISING;

        /// <summary>
        /// The instancies of the class are indexed with this index.
        /// </summary>
        private int _ThreadIndex = 0;

        /// <summary>
        /// Tells the thread that it should continue processing.
        /// Controls while-cycle in the run method. Can be assigned directly from
        /// the code in case of exceptional situation as oppose to the controlled
        /// stopping of the thread using the <code>ProcessingThread.Stop()</code> method.
        /// <see cref="ProcessingThread.Run()"/>        
        /// </summary>
        private bool _KeepProcessing = true;

        /// <summary>
        /// Contains the last caught exception.
        /// As there is no means how to catch an exception thrown from the
        /// <code>ProcessingThread.Run()</code> method, for case that it's necessary to examine an
        /// exception thrown in <code>ProcessingThread.Run()</code> it is stored to this variable.<br>
        /// Descendants of <code>ProcessingThread</code> will use this variable
        /// to store the exception which will cause termination of the thread.
        /// It is also set by <code>ProcessingThread.Run()</code> method if it'll catch an
        /// exception when calling <code>ProcessingThread.Process</code>.<br>
        /// The exception is also set by the <code>ProcessingThread.StopProcessing()</code>
        /// method.
        /// <see cref="ProcessingThread.StopProcessing(Exception)"/>
        /// <see cref="ProcessingThread.SetTermException(Exception)"/>
        /// <see cref="ProcessingThread.GetTermException()"/>        
        /// </summary>
        private Exception _TermException = null;

        /// <summary>
        /// The thread which runs the code of this class.
        /// </summary>
        private Thread _ProcessingThread = null;

        private int _ThreadSleepTime = 30;
        public int ThreadSleepTime
        {
            get
            {
                return _ThreadSleepTime;
            }
            set
            {
                _ThreadSleepTime = value;
            }
        }

        private bool _ThreadSleeping = false;

        /// <summary>
        /// The method which is repeatedly called from the <code>ProcessingThread.Run()</code>
        /// method. This is supposed the hearth of the actual processing. The
        /// derived classes should implement their code in this method.
        /// </summary>
        public abstract void Process();

        /// <summary>
        /// Creates new thread and passes it <code>this</code> as
        /// the <code>Runnable</code> to run. Resets private variables to defaults.
        /// Starts the newly created thread.
        /// <see cref="Thread"/>        
        /// </summary>
        /// <param name="_name"></param>
        public virtual void Start(string _name)
        {
            //debug.enter(DUTL,this,"start()");
            if (!IsProcessing())
            {
                // i.e. is initialising or finished
                SetProcessingStatus(PROC_INITIALISING);
                _TermException = null;
                _KeepProcessing = true;

                //To name a delegate.
                ThreadStart ts = new ThreadStart(this.Run);
                _ProcessingThread = new Thread(ts);
                _ProcessingThread.Name = _name;
                _ProcessingThread.Start();
                while (IsInitialising())
                {
                    Sleep(30);
                    //Thread.yield(); // we're waiting for the proc thread to start
                }
            }
            //debug_.exit(DUTL,this);
        }

        /// <summary>
        /// Using default thread name
        /// </summary>
        public virtual void Start()
        {
            if (!IsProcessing())
            {
                // i.e. is initialising or finished
                SetProcessingStatus(PROC_INITIALISING);
                _TermException = null;
                _KeepProcessing = true;

                //To name a delegate.
                ThreadStart ts = new ThreadStart(this.Run);
                _ProcessingThread = new Thread(ts);
                _ProcessingThread.Name = GenerateDefaultIndexedThreadName();
                _ProcessingThread.Start();
                while (IsInitialising())
                {
                    Sleep(30);
                    //Thread.yield(); // we're waiting for the proc thread to start
                }
            }
        }

        /// <summary>
        /// Stops the receiving by setting flag <code>_KeepProcessing</code> to false.
        /// Waits until the receiving is really stopped.
        /// </summary>
        public virtual void Stop(int timeout)
        {
            if (IsProcessing())
            {
                StopProcessing(null);

                if (_ProcessingThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                {
                    _ProcessingThread.Interrupt();
                }
                DateTime dt = DateTime.Now;
                while (!IsFinished())
                {
                    TimeSpan tp = DateTime.Now - dt;
                    if (tp.TotalMilliseconds < timeout)
                    {
                        Sleep(10);
                    }
                    else
                    {
                        _ProcessingThread.Abort();
                    }
                    //Thread.yield(); // we're waiting for the proc thread to stop
                }
            }
        }

        /// <summary>
        /// Stops the receiving by setting flag <code>_KeepProcessing</code> to false.
        /// Waits until the receiving is really stopped.
        /// </summary>
        public virtual void Stop()
        {
            if (IsProcessing())
            {
                StopProcessing(null);

                if (_ProcessingThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                {
                    _ProcessingThread.Interrupt();
                }
                while (!IsFinished())
                {
                    Sleep(10);
                }
            }
        }


        /// <summary>
        /// Causes stoping of the while-loop in the <code>Run</code> method.
        /// Called from <code>Stop</code> method or can be used to terminate
        /// the processing thread in case of an exceptional situation while
        /// processing (from <code>Process</code> method.)
        /// </summary>
        /// <param name="e"></param>
        protected void StopProcessing(Exception e)
        {
            SetTermException(e);
            _KeepProcessing = false;
        }

        /// <summary>
        /// Calls <code>Process</code> in cycle until stopped.
        /// This method is called from <code>Thread</code>'s code as the code which
        /// has to be executed by the thread.         
        /// <see cref="Thread"/>
        /// </summary>
        public void Run()
        {
            try
            {
                SetProcessingStatus(PROC_RECEIVING);
                while (_KeepProcessing)
                {
                    try
                    {
                        Process();
                    }
                    catch (Exception e1)
                    {
                        //khong thoat khoi tien trinh khi gap exception
                        //System.Console.WriteLine("Exception in ProcessingThread" + e1.ToString());
                        //m_logger.Debug(e1.ToString());
                        _KeepProcessing = false;
                    }
                    //Like  Thread.yield() in Java???
                    Sleep(30);
                }
            }
            catch (Exception e)
            {
                SetTermException(e);
                //debug_.write("ProcessingThread.run() caught unhadled exception "+e);
                //event_.write(e,"ProcessingThread.run() unhadled exception");
            }
            finally
            {
                SetProcessingStatus(PROC_FINISHED);
                //debug_.exit(DUTL,this);
            }
        }

        /// <summary>
        /// Uses <code>GetDefaultThreadName</code> and <code>GetThreadIndex</code>
        /// to generate unique name for the thread. Called during initialisation
        /// of the thread.
        /// </summary>
        /// <returns></returns>
        public String GenerateDefaultIndexedThreadName()
        {
            return GetDefaultThreadName() + "-" + GetThreadIndex();
        }

        /// <summary>
        /// Should return the name for the thread. Derived classes are expected
        /// to return specific name here from this method.
        /// </summary>
        /// <returns></returns>
        public virtual String GetThreadName()
        {
            return _ProcessingThread.Name;
        }

        /// <summary>
        /// Should return the name for the thread. Derived classes are expected
        /// to return specific name here from this method.
        /// </summary>
        /// <returns></returns>
        public String GetDefaultThreadName()
        {
            return PROCESSING_THREAD_NAME;
        }

        /// <summary>
        /// In case there are multiple instancies of the class this generates
        /// and returns instance index.
        /// </summary>
        /// <returns></returns>
        public int GetThreadIndex()
        {
            return ++_ThreadIndex;
        }

        public int GetSystemCurrentThreadID()
        {
            return AppDomain.GetCurrentThreadId();
        }

        /// <summary>
        /// As there is no means how to catch an exception thrown from
        /// <code>Run</code> method, in case that it's necessary to throw an
        /// exception it's rather remembered by calling of this method.             
        /// </summary>
        /// <param name="e">the exception to remember</param>
        protected void SetTermException(Exception e) { _TermException = e; }

        /// <summary>
        /// Returns the last exception caught during processing.        
        /// </summary>
        /// <returns></returns>
        public Exception GetTermException()
        {
            return _TermException;
        }

        /// <summary>
        /// Sets the <code>_ProcessingStatus</code> to value provided.
        /// </summary>
        /// <param name="value"></param>
        private void SetProcessingStatus(byte value)
        {
            lock (_ProcessingStatusLock)
            {
                _ProcessingStatus = value;
            }
        }

        /// <summary>
        /// Returns if the <code>_ProcessingStatus</code> indicates that
        /// the receiving is in initialisation stage, i.e. the processing loop
        /// has not been entered yet.
        /// </summary>
        /// <returns></returns>
        private bool IsInitialising()
        {
            lock (_ProcessingStatusLock)
            {
                return _ProcessingStatus == PROC_INITIALISING;
            }
        }

        /// <summary>
        /// Returns if the <code>_ProcessingStatus</code> indicates that
        /// the receiving has started, but it didn't finished yet, i.e. the
        /// the thread is still in the processing loop.
        /// </summary>
        /// <returns></returns>
        public bool IsProcessing()
        {
            lock (_ProcessingStatusLock)
            {
                return _ProcessingStatus == PROC_RECEIVING;
            }
        }

        /// <summary>
        /// Returns if the <code>ThreadProcessing._ProcessingStatus</code> indicates that
        /// the receiving has finished, i.e. the processing loop
        /// has finished.
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            lock (_ProcessingStatusLock)
            {
                return _ProcessingStatus == PROC_FINISHED;
            }
        }

        public void Sleep(int timeout)
        {
            try
            {
                _ThreadSleeping = true;
                Thread.Sleep(timeout);//wait 1 minute
                _ThreadSleeping = false;
            }
            catch (ThreadInterruptedException e)
            {
                _ThreadSleeping = false;
                //Console.WriteLine(e.Message);
            }
        }

        public bool IsSleeping()
        {
            return _ThreadSleeping;
        }
    }
}
