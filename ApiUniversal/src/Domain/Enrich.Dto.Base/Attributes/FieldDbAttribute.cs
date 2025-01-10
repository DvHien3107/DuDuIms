using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Attributes
{
    /// <summary>
    /// Using for map column of dto with sql server (general function base on itself)
    /// </summary>
    public class FieldDbAttribute : Attribute
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public int Size { get; set; }

        public bool AllowNull { get; set; }

        public string Table { get; set; }

        public PropertyInfo Prop { get; set; }

        public FieldDbAttribute(string name = "", string type = "", int size = 0, bool allowNull = true, string table = "")
        {
            Name = name;
            Table = table;
            Type = type;
            Size = size;
            AllowNull = allowNull;
        }
    }
}
