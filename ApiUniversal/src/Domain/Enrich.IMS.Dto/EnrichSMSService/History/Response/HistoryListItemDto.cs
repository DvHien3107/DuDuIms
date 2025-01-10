using Enrich.Dto.Attributes;
using Newtonsoft.Json;
using System;

namespace Enrich.IMS.Dto.EnrichSMSService
{
    public class HistoryListItemDto
    {

        [GridField(Index = 1, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.Id")]
        public int Id { get; set; }

        [GridField(Index = 2, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.StoreCode")]
        public string StoreCode { get; set; }
                        
        public string StoreName { get; set; }


        [GridField(Index = 3, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.Description")]
        public string Description { get; set; }

        [GridField(Index = 4, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.ObjectId")]
        public int ObjectId { get; set; }

        [GridField(Index = 5, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.RemainingSMS")]
        public int RemainingSMS { get; set; }

        [GridField(Index = 6, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.UsedSegment")]
        public int UsedSegment { get; set; }

        [GridField(Index = 7, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.TotalSegment")]
        public int TotalSegment { get; set; }

        [GridField(Index = 8, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.Price")]
        public double Price { get; set; }

        [GridField(Index = 9, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.SentMessage")]
        public int SentMessage { get; set; }

        [GridField(Index = 10, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.Type")]
        public int Type { get; set; }

        [GridField(Index = 11, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.Caller")]
        public string Caller { get; set; }

        [GridField(Index = 12, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("History.CreatedDate")]
        public DateTime? CreatedDate { get; set; }
    }
}
