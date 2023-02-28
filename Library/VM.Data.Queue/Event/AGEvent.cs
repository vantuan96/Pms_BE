using System;
using System.Collections.Generic;
using System.Text;

namespace VM.Data.Queue
{
    /// <summary>
    /// Delegate pointers, these are used for calling events, and async methods.
    /// </summary>

    /// <summary>
    /// Delegate for displaying the log message on Agent Interface
    /// </summary>
    public delegate void ShowLogMessage(string sMessage);


    /// <summary>
    /// Delegate for displaying the number of message received
    /// </summary>
    public delegate void CountHandleMessage(string msgType, int count);

    /// <summary>
    /// Delegate for displaying the message info has been sent
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="msgInfo"></param>
    public delegate void ShowMessageInfo(string msgType, string msgInfo);
}
