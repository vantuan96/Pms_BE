using System;
using System.Collections.Generic;
using System.Text;

namespace VM.Data.Queue
{
    public class MTSequence
    {
        private static object mutex = new object();
       
        public static int GetNextSequenceNumber()
        {
            lock (mutex)
            {
                int result = 0;
                result = SequenceNumber.GetNextSequenceNumber("MTSEQ", "MTSEQ.xml");
                return result;
            }
        }

        public static void ResetSequenceNumber()
        {
            lock (mutex)
            {
                SequenceNumber.ResetSequenceNumber("MTSEQ", "MTSEQ.xml");
            }
        }

    }
}
