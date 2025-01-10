using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inner.Libs.Validation;
using Newtonsoft.Json;

namespace EnrichcousBackOffice.API.DTO
{
    public class Ticket
    {
        [JsonProperty("ticket_id")] public long? Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("description")] public string Content { get; set; }
        [JsonProperty("priority")] public string Priority { get; set; }

        /// <summary>
        /// Store id
        /// </summary>
        [Required]
        [JsonProperty("store_id")] public string StoreId { get; set; }
        
        /// <summary>
        /// Required : sales / tech / admin
        /// </summary>
        [Required]
        [StringRange(Allowable = new[] { "sales", "tech" , "admin"}, ErrorMessage = "Type must be either 'sales','tech' or 'admin'.")]
        [JsonProperty("type")]public string Type { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("attachments")] public List<string> Attachments { get; set; }
        [JsonProperty("feedbacks")] public List<Feedback> Feedbacks { get; set; }
    }

    public class Feedback
    {
        [Required]
        [JsonProperty("ticket_id")] public long? TicketId { get; set; }
        [JsonProperty("feedback_id")] public long? Id { get; set; }
        [Required]
        [JsonProperty("feedback_title")] public string Title { get; set; }
        [Required]
        [JsonProperty("feedback_content")] public string Content { get; set; }
        [JsonProperty("attachments")] public List<string> Attachments { get; set; }
    }
}