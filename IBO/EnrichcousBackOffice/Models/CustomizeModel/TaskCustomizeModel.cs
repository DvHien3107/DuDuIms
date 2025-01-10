using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class P_Member_image
    {
        public long Id { get; set; }
        public string Picture { get; set; }
        public string FullName { get; set; }
    }
    public class Ts_Task_session
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool? Complete { get; set; }
        public List<UploadMoreFile> Files { get; set; }
    }
}