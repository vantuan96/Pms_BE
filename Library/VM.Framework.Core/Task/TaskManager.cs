#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Manager for the task scheduler
    /// </summary>
    public class TaskManager:Singleton<TaskManager>
    {
        #region Constructor
        
        /// <summary>
        /// Constructor
        /// </summary>
        protected TaskManager()
            : base()
        {
            try
            {
                Config = ConfigManager.Instance.GetConfigFile<Configuration>("TaskScheduler");
                Workers = new List<Worker>();
                for (int x = 0; x < Config.NumberOfThreads; ++x)
                {
                    Worker TempWorker = new Worker("");
                    TempWorker.Finished = new EventHandler<OnEndEventArgs>(Finished);
                    TempWorker.Started = new EventHandler<OnStartEventArgs>(Started);
                    TempWorker.Exception = new EventHandler<OnErrorEventArgs>(Error);
                    Workers.Add(TempWorker);
                }
            }
            catch { throw; }
        }

        #endregion

        #region Public Functions
        public void Start(string TaskAssemblyLocation)
        {
            Start(TaskAssemblyLocation,null);
        }
        /// <summary>
        /// Starts the task manager
        /// </summary>
        /// <param name="TaskAssemblyLocation">Location of the task assembly</param>
        /// <param name="Log">Log using (if null, it uses its own default)</param>
        public void Start(string TaskAssemblyLocation, ILog Log)
        {
            try
            {
                TaskManager.Log = GetLog(Log);
                Assembly TaskAssembly = Assembly.LoadFile(TaskAssemblyLocation);
                AddTasks(TaskAssembly);
                StartWorkers();
            }
            catch { throw; }
        }


        public void Start(Assembly TaskAssembly)
        {
            Start(TaskAssembly,null);
        }
        /// <summary>
        /// Starts the task manager
        /// </summary>
        /// <param name="TaskAssembly">The task assembly</param>
        /// <param name="Log">Log using (if null, it uses its own default)</param>
        public void Start(Assembly TaskAssembly, ILog Log )
        {
            try
            {
                TaskManager.Log = GetLog(Log);
                AddTasks(TaskAssembly);
                StartWorkers();
            }
            catch { throw; }
        }


        public void Start(List<Assembly> TaskAssemblies)
        {
            Start(TaskAssemblies,null);
        }

        /// <summary>
        /// Starts the task manager
        /// </summary>
        /// <param name="TaskAssemblies">The task assemblies</param>
        /// <param name="Log">Log using (if null, it uses its own default)</param>
        public void Start(List<Assembly> TaskAssemblies,ILog Log )
        {
            try
            {
                TaskManager.Log = GetLog(Log);
                for (int x = 0; x < TaskAssemblies.Count; ++x)
                {
                    AddTasks(TaskAssemblies[x]);
                }
                StartWorkers();
            }
            catch { throw; }
        }

        /// <summary>
        /// Stops the tasks
        /// </summary>
        public void Stop()
        {
            try
            {
                for (int x = 0; x < Config.NumberOfThreads; ++x)
                {
                    Workers[x].Stop();
                }
            }
            catch { throw; }
        }

        #endregion

        #region Private Functions

        private ILog GetLog(ILog Log)
        {
            try
            {
                if (Log == null)
                {
                    Log = LogManager.Instance.GetLog("EchoNet");
                }
                return Log;
            }
            catch { throw; }
        }

        private void AddTasks(Assembly TaskAssembly)
        {
            try
            {
                List<Type> TaskTypes = Reflection.GetTypes(TaskAssembly, typeof(Task).FullName);
                for (int x = 0; x < TaskTypes.Count; )
                {
                    for (int y = 0; y < Workers.Count && x < TaskTypes.Count; ++y, ++x)
                    {
                        Task TempTask = (Task)TaskTypes[x].Assembly.CreateInstance(TaskTypes[x].FullName);
                        TempTask.Setup(TaskTypes[x].Name);
                        Workers[y].AddTask(TempTask);
                    }
                }
            }
            catch { throw; }
        }

        private void StartWorkers()
        {
            try
            {
                for (int x = 0; x < Config.NumberOfThreads; ++x)
                {
                    Workers[x].Start();
                }
            }
            catch { throw; }
        }

        #endregion

        #region Private Properties

        private List<Worker> Workers { get; set; }
        private Configuration Config { get; set; }
        private static ILog Log { get; set; }

        #endregion

        #region Event Handlers

        internal static void Finished(object sender, OnEndEventArgs e)
        {
            try
            {
                lock (Log)
                {
                    Log.LogMessage("Worker finished", MessageType.General);
                }
            }
            catch { throw; }
        }

        internal static void Started(object sender, OnStartEventArgs e)
        {
            try
            {
                lock (Log)
                {
                    Log.LogMessage("Worker started", MessageType.General);
                }
            }
            catch { throw; }
        }

        internal static void Error(object sender, OnErrorEventArgs e)
        {
            try
            {
                Exception Temp = (Exception)e.Content;
                lock (Log)
                {
                    Log.LogMessage("Worker had the following error : {0}", MessageType.Error, Temp.ToString());
                }
            }
            catch { throw; }
        }

        #endregion
    }
}
