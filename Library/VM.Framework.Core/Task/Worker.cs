#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Worker class (individual thread class)
    /// </summary>
    public class Worker : Worker<bool, string>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Params">Not used</param>
        public Worker(string Params)
            : base(Params)
        {
            try
            {
                Tasks = new List<Task>();
            }
            catch { throw; }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Adds a task to 
        /// </summary>
        /// <param name="Task">Task to add</param>
        public void AddTask(Task Task)
        {
            try
            {
                lock (Tasks)
                {
                    Tasks.Add(Task);
                }
            }
            catch { throw; }
        }

        #endregion

        #region Overridden Functions

        protected override bool Work(string Params)
        {
            try
            {
                while (true)
                {
                    if (Stopping)
                        return true;
                    lock (Tasks)
                    {
                        for (int x = 0; x < Tasks.Count; ++x)
                        {
                            if (Tasks[x].NextRunTime < DateTime.Now)
                            {
                                Tasks[x].DoWork();
                                Tasks[x].UpdateTime(true);
                            }
                        }
                    }
                    Sleep(1000);
                }
            }
            catch { throw; }
        }

        #endregion

        #region Private Properties

        private List<Task> Tasks { get; set; }

        #endregion
    }
}
