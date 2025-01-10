using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.ViewModel
{

    public class Ticket_request : DataTable_request
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public long? Severity { get; set; }
        public string Tags { get; set; }
        public string Cates { get; set; }
        
    }

    public class DeploymentTicket_request : DataTable_request
    {
        public long?[] Type { get; set; }
        public string[] Status { get; set; }
        public string Tab { get; set; }
        public string[] Priority { get; set; }
        public long?[] Severity { get; set; }
        public string[] Tags { get; set; }
        public string FilterBy { get; set; }
        public string[] CustomerCode { get; set; }
        public string[] LicenseCode { get; set; }
        public string[] MemberNumber { get; set; }
        public string[] Departments { get; set; }
        public string[] OpenBy { get; set; }
        public string SearchText { get; set; }
        public string CloseAt { get; set; }
        public string[] TypeDevelop { get; set; }
        public string Page { get; set; }
        public string ProjectId { get; set; }
        public string StageId { get; set; }
        public string VersionId { get; set; }
        public string GMT { get; set; }
        public string[] MemberTag { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

    }
    public class Order_Request 
    {
        public DateTime FromDate { get; set; }
        public DateTime Todate { get; set; }
        public string SearchText { get; set; }
        public string SearchBy { get; set; }
        public string Team { get; set; }
        public string Partner { get; set; }
        public string FromSystem { get; set; }
        public string SalesPerson { get; set; }
        public string Status { get; set; }
    }
    public class DataTable_request
    {
        public int draw { get; set; }
        public List<Column> columns { get; set; }
        public List<Order> order { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public Search search { get; set; }
        public class Column
        {
            public int data { get; set; }
            public string name { get; set; }
            public bool searchable { get; set; }
            public bool orderable { get; set; }
            public Search search { get; set; }
        }
        public class Search
        {
            public string value { get; set; }
            public bool regex { get; set; }
        }
        public class Order
        {
            public int column { get; set; }
            public string dir { get; set; }
        }
    }

}