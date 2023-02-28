using DataAccess.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.MasterData
{
    public class Sites:Site
    {
        public string _id { get; set; }
    }
}
