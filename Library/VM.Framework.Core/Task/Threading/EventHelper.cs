using System;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Helps with events
    /// </summary>
    public static class EventHelper
    {
        #region Public Static Functions

        /// <summary>
        /// Raises an event
        /// </summary>
        /// <typeparam name="T">The type of the event args</typeparam>
        /// <param name="Delegate">The delegate</param>
        /// <param name="EventArgs">The event args</param>
        public static void Raise<T>(T EventArgs, Action<T> Delegate) where T : class
        {
            try
            {
                if (Delegate != null)
                {
                    Delegate(EventArgs);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Raises an event
        /// </summary>
        /// <typeparam name="T">The type of the event args</typeparam>
        /// <param name="Delegate">The delegate</param>
        /// <param name="Sender">The sender</param>
        /// <param name="EventArg">The event args</param>
        public static void Raise<T>(EventHandler<T> Delegate, object Sender, T EventArg) where T : System.EventArgs
        {
            try
            {
                if (Delegate != null)
                {
                    Delegate(Sender, EventArg);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Raises an event
        /// </summary>
        /// <typeparam name="T1">The event arg type</typeparam>
        /// <typeparam name="T2">The return type</typeparam>
        /// <param name="Delegate">The delegate</param>
        /// <param name="EventArgs">The event args</param>
        /// <returns>The value returned by the function</returns>
        public static T2 Raise<T1, T2>(T1 EventArgs, Func<T1, T2> Delegate) where T1 : class
        {
            try
            {
                if (Delegate != null)
                {
                    return Delegate(EventArgs);
                }
                return default(T2);
            }
            catch { throw; }
        }

        #endregion
    }
}
