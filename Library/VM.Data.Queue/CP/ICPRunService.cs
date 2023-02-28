//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace GAPIT.MKT.Data
//{
//    /// <summary>
//    /// Define the general service of a content provider
//    /// </summary>
//    public interface ICPRunService
//    {
//        /// <summary>
//        /// Check if the MO message received will process by this service
//        /// </summary>
//        /// <param name="sServiceID">The short code</param>
//        /// <param name="sCommandCode">The keyword</param>
//        /// <param name="pg">The CP service to check</param>
//        /// <returns>true if CP service will process this MO</returns>
//        bool Check(string sServiceID, string sCommandCode, CPService pg);

//        /// <summary>
//        /// The CP service will process this MO message when check result = true
//        /// </summary>
//        /// <param name="msg">The MO message wants to process</param>
//        void Process(MessageData msg);

//        /// <summary>
//        /// Stop processing action in this CP service
//        /// </summary>
//        void Stop();

//        /// <summary>
//        /// Start handling the CP service
//        /// </summary>
//        /// <param name="pg">CP service wants to handle</param>
//        void Start(CPService pg);
//    }
//}
