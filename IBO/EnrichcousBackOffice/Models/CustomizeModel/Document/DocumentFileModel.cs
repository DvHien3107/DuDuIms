using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Document
{
    public class DocumentFileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Breadcrumb {get;set;}
        public string Description { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string Attachment { get; set; }
        public string UpdateBy { get; set; }
        public string CreateBy { get; set; }
        public string UpdateAt { get; set; }
        public string CreateAt { get; set; }
    }
}