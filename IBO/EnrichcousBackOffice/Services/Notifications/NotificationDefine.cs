using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Services.Notifications
{
    public static class NotificationTemplateDefine
    {
        // ticket
        public const string TicketAdd = "Ticket.Add";
        public const string TicketUpdate = "Ticket.Update";
        public const string TicketClose = "Ticket.Close";
        public const string TicketTag = "Ticket.Tag";
        public const string TicketDeadline = "Ticket.Deadline";
        public const string TicketReminders = "Ticket.TicketReminders";
        public const string Task_NewTaskTicket = "Task.NewTaskTicket";
        public const string Task_UpdateTaskTicket = "Task.UpdateTaskTicket";
        //order
        public const string OrderAdd = "Order.Add";
        public const string OrderUpdate = "Order.Update";
        public const string OrderClose = "Order.Close";
        public const string OrderAssign = "Order.Assign";
        //sales lead
        public const string SalesLeadAssign = "SalesLead.Assign";
       
    }
    public static class NotificationCategoryDefine
    {
        // ticket
        public  const string Ticket = "Ticket";
        public const string  TicketReminder = "TicketReminder";
        public  const string Order = "Order";
        public  const string SalesLead = "SalesLead";
        public const string Task = "Task";
    }
    public static class NotificationActionDefine
    {
        // ticket
        public const string AddNew = "AddNew";
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Delete = "Delete";
    }
}