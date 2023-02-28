using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.AdminModels
{
    public class MenuModel:Menu
    {
        public List<Menu> SubMenus { get; set; }
    }
    public class Menu
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Order { get; set; }
        public string Url { get; set; }
        public string UrlTarget { get; set; }
    }
}
