using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Project
{
    public class ProjectDetailModel
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<T_Project_Milestone> Versions { get; set; }
        public List<T_Project_Stage> Stages { get; set; }
    }
}