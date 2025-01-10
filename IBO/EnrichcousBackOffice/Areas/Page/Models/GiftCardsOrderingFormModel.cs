using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Areas.Page.Models
{
    public class GiftCardsOrderingFormModel
    {
        public int Id { get; set; }
        public long CustomerId { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string BusinessName { get; set; }
        public string SalonAddress { get; set; }
        public string SalonAddress1 { get; set; }
        public string SalonAddress2 { get; set; }
        public string SalonCity { get; set; }
        public string SalonState { get; set; }
        public string ZipCode { get; set; }
        public string SalonHours { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string FrontDesign { get; set; }
        public string BackDesign { get; set; }
        public string FrontDesignFiles { get; set; }
        public string BackDesignFiles { get; set; }
        public string Note { get; set; }
        public List<SelectListItem> ListState { get; set; }
        public List<SelectListItem> ListProduct { get; set; }
        public List<SelectListItem> ListBackDesign { get; set; }
    }
}