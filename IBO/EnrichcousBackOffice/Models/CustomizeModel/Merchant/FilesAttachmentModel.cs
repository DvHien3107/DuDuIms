using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Merchant
{
    public class FilesAttachmentModel
    {
        public T_SupportTicket Ticket { get; set; }
        public List<UploadMoreFile> TicketUploadFiles { get; set; }
        public List<UploadMoreFile> FilesFeedback { get; set; }
        public List<T_TicketFeedback> Feedback { get; set; }
        public List<T_FileRelated> T_FilesRelated { get; set; }
        public List<UploadMoreFile> FilesRelatedUploads { get; set; }
    }

    public struct P_ChangeTab_files
    {
        public string TK { get; set; }
        public string UP { get; set; }
        public string UpFb { get; set; }
        public string FB { get; set; }
        public string FR { get; set; }
        public string filesRelated { get; set; }
    }
}