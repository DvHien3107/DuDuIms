using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Document
{
    public class DocumentCategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UpdateBy { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public DateTime? CreateAt { get; set; }
        public List<DocumentFileModel> DocumentFiles { get; set; }
    }
}