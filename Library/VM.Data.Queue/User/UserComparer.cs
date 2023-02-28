using System;
using System.Collections.Generic;
using System.Text;

namespace VM.Data.Queue
{
    public class UserComparer: System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            int answer = 0;

            User u1 = x as User;
            User u2 = y as User;

            if (u1 == null && u2 == null)
            {
                return 0;
            }
            else if (u1 == null)
            {
                return -1;
            }
            else if (u2 == null)
            {
                return 1;
            }

            answer = u1.UserName.CompareTo(u2.UserName);

            return answer;
        }
    }
}
