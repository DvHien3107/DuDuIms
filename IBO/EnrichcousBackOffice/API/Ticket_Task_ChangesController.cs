using EnrichcousBackOffice.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EnrichcousBackOffice.API
{
    [RoutePrefix("api/tickettaskchanges")]
    public class Ticket_Task_ChangesController : ApiController
    {
        //public List<Ticket_Task_Info> Get(string memberNo)
        //{
        //    try
        //    {
        //        var list_ticket_task = new List<Ticket_Task_Info>();
        //        //SqlDependency.Start(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        //        var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        //        connection.Open();

        //        //SqlCommand command_Ticket = new SqlCommand(@"SELECT [Id],[Name],[TypeId],[AssignedToMemberNumber],[CreateAt] 
        //        //FROM [dbo].[T_SupportTicket]
        //        //WHERE[AssignedToMemberNumber] like '%" + memberNo + "%' and[DateClosed] IS NULL", connection);


        //        string sqlQuery_ticket = @"SELECT [Id],[TicketId],[CloseDate],[OpenDate],[AssignedMember_Numbers],[StatusId],[StageId]
        //                                      FROM [dbo].[T_TicketStage_Status]
        //                                      WHERE [Active] = 1 AND [AssignedMember_Numbers] like '%" + memberNo + "%'";
        //        SqlCommand command_Ticket = new SqlCommand(sqlQuery_ticket, connection);

        //        SqlCommand command_Task = new SqlCommand(@"SELECT [Id],[Name],[AssignedToMemberNumber],[CreateAt]
        //        FROM [dbo].[Ts_Task]
        //        WHERE ([AssignedToMemberNumber] like '%" + memberNo + "%' or [CreateByMemberNumber] like '%" + memberNo + "') and ([Complete] != 'True' or [Complete] IS NULL) and ParentTaskId IS NULL", connection);


        //        // Make sure the command object does not already have
        //        // a notification object associated with it.
        //        command_Ticket.Notification = null;
        //        command_Task.Notification = null;


        //        #region Ticket

        //        SqlDependency dependency_Ticket = new SqlDependency(command_Ticket);
        //        dependency_Ticket.OnChange += new OnChangeEventHandler(Dependency_OnChange);

        //        if (connection.State == ConnectionState.Closed)
        //            connection.Open();

        //        var reader_Ticket = command_Ticket.ExecuteReader();

        //        //list_ticket_task = reader_Ticket.Cast<IDataRecord>()
        //        //            .Select(x => new Ticket_Task_Info
        //        //            {
        //        //                Id = (long)x["Id"],
        //        //                Name = (string)x["Name"],
        //        //                Type = "Ticket",
        //        //                CreateAt = (DateTime)x["CreateAt"],
        //        //                TicketTypeId = (long)x["TypeId"]
        //        //            }).ToList();

        //        var db = new WebDataModel();
        //        var listTicket_Stage = reader_Ticket.Cast<IDataRecord>()
        //                    .Select(x => new
        //                    {
        //                        TicketId = (long)x["TicketId"],
        //                        StatusId = x["StatusId"]?.ToString(),
        //                        StageId = x["StageId"]?.ToString()
        //                    }).ToList();
        //        var listTicket_origin = listTicket_Stage.GroupBy(x => x.TicketId).Select(c => c.Key).ToList();
        //        listTicket_origin = listTicket_origin.Where(t =>
        //                        listTicket_Stage.Where(c => c.TicketId == t && db.T_TicketStatus.Where(ts => ts.Type == "closed").Any(ts => ts.Id.ToString() == c.StatusId)).Count() == 0 &&
        //                        listTicket_Stage.Where(c => c.TicketId == t && db.T_Project_Stage_Members.Any(ps => ps.StageId == c.StageId && ps.MemberNumber == memberNo)).Count() > 0
        //                        ).ToList();
        //        listTicket_origin.ForEach(t =>
        //        {
        //            var ticket = db.T_SupportTicket.FirstOrDefault(s => s.Id == t && s.Visible == true);
        //            if (ticket != null)
        //                list_ticket_task.Add(new Ticket_Task_Info
        //                {
        //                    Id = ticket.Id,
        //                    Name = ticket.Name,
        //                    Type = "Ticket",
        //                    CreateAt = ticket.CreateAt,
        //                    TicketTypeId = ticket.TypeId
        //                });
        //        });

        //        connection.Close();
        //        #endregion


        //        //#region Task

        //        //SqlDependency dependency_Task = new SqlDependency(command_Task);
        //        //dependency_Task.OnChange += new OnChangeEventHandler(Dependency_OnChange);

        //        //if (connection.State == ConnectionState.Closed)
        //        //    connection.Open();

        //        //var reader_Task = command_Task.ExecuteReader();

        //        //var list_Task = reader_Task.Cast<IDataRecord>()
        //        //            .Select(x => new Ticket_Task_Info
        //        //            {
        //        //                Id = (long)x["Id"],
        //        //                Name = (string)x["Name"],
        //        //                Type = "Task",
        //        //                CreateAt = (DateTime?)x["CreateAt"],
        //        //            }).ToList();

        //        //foreach (var item in list_Task)
        //        //{
        //        //    list_ticket_task.Add(item);
        //        //}

        //        //#endregion

        //        list_ticket_task = list_ticket_task.OrderByDescending(x => x.CreateAt).ToList();

        //        return list_ticket_task;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

        //private void Dependency_OnChange(object sender, SqlNotificationEventArgs e)
        //{
        //    IHubContext context = GlobalHost.ConnectionManager.GetHubContext<BroadcastHub>();
        //    context.Clients.All.displayTicketTask();
        //}

        //public class Ticket_Task_Info
        //{
        //    public long? Id { get; set; }
        //    public string Name { get; set; }
        //    public string Type { get; set; }
        //    public DateTime? CreateAt { get; set; }
        //    public long? TicketTypeId { get; set; }
        //}
    }
}
