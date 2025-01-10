using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Orther
{
    public class Ninja
    {
        public string Key { get; set; }
        public Clan Clan { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
    }

    public class Clan
    {
        public string Name { get; set; }
    }
}
