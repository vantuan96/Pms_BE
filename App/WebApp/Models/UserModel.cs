using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string FullShortName { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string Mobile { get; set; }
    }
}