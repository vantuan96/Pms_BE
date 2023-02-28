using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Determines the frequency at which a task is run
    /// </summary>
    public enum RunTime
    {
        Once,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
