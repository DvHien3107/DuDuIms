using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class StagesModel
    {
        public T_Project_Stage Stage { get; set; }
        public List<string> VersionIds { get; set; }
    }
}