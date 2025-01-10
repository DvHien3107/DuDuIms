using EnrichcousBackOffice.Models;
using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Services
{
    public class DeploymentService : IDisposable
    {
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        public void ClearDeployment(string OrderCode)
        {
            List<string> UnassignedInvs = new List<string>();
            using (WebDataModel db = new WebDataModel())
            {

                var orderProduct = db.Order_Products.Where(o => o.OrderCode == OrderCode).ToList();
                foreach (var o in orderProduct)
                {
                    if (!string.IsNullOrEmpty(o.InvNumbers))
                    {
                        UnassignedInvs.AddRange(o.InvNumbers?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList());
                    }
                };
                db.SaveChanges();
            }
            UnassignedInvs.ForEach(inv =>
            {
                unAssignedDevice(inv);
            });
        }
        public bool unAssignedDevice(string InvNumber)
        {
            if (string.IsNullOrEmpty(InvNumber))
            {
                return true;
            }
            try
            {

                using (WebDataModel db = new WebDataModel())
                {
                    var device = db.O_Device.Where(d => d.InvNumber == InvNumber).FirstOrDefault();
                    if (device == null)
                    {

                        var _orderDevice = db.Order_Products.Where(o => o.InvNumbers.Contains(InvNumber)).FirstOrDefault();
                        if (_orderDevice != null)
                        {
                            var _list_Invs = (_orderDevice.InvNumbers ?? "").Split(',').ToList();
                            _list_Invs.Remove(InvNumber);
                            //var _list_Sers = (_orderDevice.SerNumbers ?? "").Split(',').ToList();
                            //_list_Sers.Remove(device.SerialNumber);

                            _orderDevice.InvNumbers = string.Join(",", _list_Invs);
                            _orderDevice.CusNumbers = string.Empty;
                            //_orderDevice.SerNumbers = string.Join(",", _list_Sers);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        device.Inventory = 1;
                        //device.AssignedOrderCode = ""; AssignedOrderCode không còn dùng, sẽ được xóa

                        var orderDevice = db.Order_Products.Where(o => o.InvNumbers.Contains(device.InvNumber)).FirstOrDefault();
                        var list_Invs = (orderDevice.InvNumbers ?? string.Empty).Split(',').ToList();
                        list_Invs.Remove(device.InvNumber);
                        var list_Sers = (orderDevice.SerNumbers ?? string.Empty).Split(',').ToList();
                        list_Sers.Remove(device.SerialNumber);
                        orderDevice.InvNumbers = string.Join(",", list_Invs);
                        orderDevice.SerNumbers = string.Join(",", list_Sers);
                        orderDevice.CusNumbers = string.Empty;


                        string his_log = "Device has been <b>returned warehouse</b>." + "<br/>By " + cMem.FullName + " - At " + DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm") + " (UTC).<br/>";
                        device.Description = his_log + "-------<br/>" + (device.Description ?? "");

                       // var deployment = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                        //var ticket = db.T_SupportTicket.Where(t => t.OrderCode == orderDevice.OrderCode && t.TypeId == deployment).FirstOrDefault();
                        //var fb = new T_TicketFeedback
                        //{
                        //    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                        //    TicketId = ticket.Id,
                        //    Feedback = his_log,
                        //    CreateByNumber = cMem.MemberNumber,
                        //    CreateByName = cMem.FullName,
                        //    CreateAt = DateTime.UtcNow,
                        //    FeedbackTitle = "Device has been returned warehouse.",
                        //    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                        //    GlobalStatus = "private"
                        //};
                        //db.T_TicketFeedback.Add(fb);
                        db.Entry(device).State = EntityState.Modified;
                        db.Entry(orderDevice).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
                return false;
            }

        }
        public void AssignedDevice(string InvNumber, string orderCode)
        {
            if (string.IsNullOrEmpty(InvNumber))
            {
                return;
            }
            try
            {
                using (WebDataModel db = new WebDataModel())
                {
                    var device = db.O_Device.Where(d => d.InvNumber == InvNumber).FirstOrDefault();
                    device.Inventory = 0;
                    //device.AssignedOrderCode = orderCode;
                    var his_log = "Device has been <b>assigned to Invoice #" + orderCode + "</b>. <br/>By " + cMem.FullName + " - At " + DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm") + " (UTC).<br/>";
                    device.Description = his_log + "-------<br/>" + (device.Description ?? "");
                    var orderDevice = db.Order_Products.Where(o => o.OrderCode == orderCode && o.ModelCode == device.ModelCode).FirstOrDefault();
                    var Invs_list = (orderDevice.InvNumbers ?? string.Empty).Split(',').ToList();
                    Invs_list.Add(device.InvNumber);
                    var Sers_list = (orderDevice.SerNumbers ?? string.Empty).Split(',').ToList();
                    Sers_list.Add(device.SerialNumber);
                    orderDevice.InvNumbers = string.Join(",", Invs_list);
                    orderDevice.SerNumbers = string.Join(",", Sers_list);
                    orderDevice.CusNumbers = string.Empty;
                    //var deployment = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                    //var ticket = db.T_SupportTicket.Where(t => t.OrderCode == orderDevice.OrderCode && t.TypeId == deployment).FirstOrDefault();
                    //var fb = new T_TicketFeedback
                    //{
                    //    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                    //    TicketId = ticket.Id,
                    //    Feedback = "Delivery status: <span class='text-warning'>Pending</span>",
                    //    CreateByNumber = cMem.MemberNumber,
                    //    CreateByName = cMem.FullName,
                    //    CreateAt = DateTime.UtcNow,
                    //    FeedbackTitle = "The delivery device has been updated",
                    //    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                    //    GlobalStatus = "private"
                    //};
                    //db.T_TicketFeedback.Add(fb);
                    db.Entry(orderDevice).State = EntityState.Modified;
                    db.Entry(device).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
            }

        }

        public void unAssignedCustomDevice(string OrderCode, string ModelCode)
        {
            if (string.IsNullOrEmpty(OrderCode) || string.IsNullOrEmpty(ModelCode))
            {
                return;
            }
            try
            {

                using (WebDataModel db = new WebDataModel())
                {
                    var orderDevice = db.Order_Products.FirstOrDefault(o => o.ModelCode == ModelCode && o.OrderCode == OrderCode);
                    orderDevice.CusNumbers = string.Empty;

                    //string his_log = "Custom device has been <b>unassign</b>." + "<br/>By " + cMem.FullName + " - At " + DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm") + " (UTC).<br/>";

                    //var deployment = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                    //var ticket = db.T_SupportTicket.Where(t => t.OrderCode == orderDevice.OrderCode && t.TypeId == deployment).FirstOrDefault();
                    //var fb = new T_TicketFeedback
                    //{
                    //    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                    //    TicketId = ticket.Id,
                    //    Feedback = his_log,
                    //    CreateByNumber = cMem.MemberNumber,
                    //    CreateByName = cMem.FullName,
                    //    CreateAt = DateTime.UtcNow,
                    //    FeedbackTitle = "Device has been unassign.",
                    //    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                    //    GlobalStatus = "private"
                    //};
                    //db.T_TicketFeedback.Add(fb);
                    db.Entry(orderDevice).State = EntityState.Modified;
                    db.SaveChanges();

                }
            }
            catch (Exception e)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
            }

        }
        public void AssignedCustomDevice(string OrderCode, string CusNumber, string ModelCode)
        {
            if (string.IsNullOrEmpty(CusNumber))
            {
                return;
            }
            try
            {
                using (WebDataModel db = new WebDataModel())
                {
                    //var his_log = "Custom device has been <b>assigned to Invoice #" + OrderCode + "</b>. <br/>By " + cMem.FullName + " - At " + DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm") + " (UTC).<br/>";

                    var orderDevice = db.Order_Products.Where(o => o.OrderCode == OrderCode && o.ModelCode == ModelCode).FirstOrDefault();
                    orderDevice.CusNumbers = CusNumber;
                    //var deployment = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                    //var ticket = db.T_SupportTicket.Where(t => t.OrderCode == orderDevice.OrderCode && t.TypeId == deployment).FirstOrDefault();
                    //var fb = new T_TicketFeedback
                    //{
                    //    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                    //    TicketId = ticket.Id,
                    //    Feedback = "Delivery status: <span class='text-warning'>Pending</span>",
                    //    CreateByNumber = cMem.MemberNumber,
                    //    CreateByName = cMem.FullName,
                    //    CreateAt = DateTime.UtcNow,
                    //    FeedbackTitle = "The delivery device has been updated",
                    //    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                    //    GlobalStatus = "private"
                    //};
                    //db.T_TicketFeedback.Add(fb);
                    db.Entry(orderDevice).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
            }

        }
        public void Dispose()
        {
        }
    }
}