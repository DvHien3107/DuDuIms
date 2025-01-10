using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class T_FileRelatedModel
    {
        public int Id { get; set; }
        public string Note { get; set; }

        public long? TicketId { get; set; }
        public List<UploadMoreFile> FilesRelated {get;set;}
    }
}