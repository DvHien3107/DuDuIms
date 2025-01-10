using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.IEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.ViewControler
{
    public class InventoryViewService
    {
        public static async Task<string> ConvertToInvoice(O_Orders _order, WebDataModel db, bool updateTicket = true, string updateBy = "")
        {
            try
            {
                _order.InvoiceNumber = long.Parse(_order.OrdersCode);
                _order.Status = InvoiceStatus.Open.ToString();
                _order.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                db.Entry(_order).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                
                //Tao ticket voi ticket_type = "Deployment" --Ghi chu: 2019/11/28
                /*Code tao ticket or day*/
                //if (updateTicket)
                //{
                //    await TicketViewController.AutoTicketScenario.UpdateTicketFromSatellite(_order.OrdersCode, AppLB.UserContent.TICKET_TYPE.Sales, updateBy);
                //}
                // var l1 = TicketViewController.AutoTicketScenario.UpdateTicketFromSatellite(_order.CustomerCode, AppLB.UserContent.TICKET_TYPE.Sales);
                // var l2 = TicketViewController.AutoTicketScenario.NewTicketDeployment(_order.InvoiceNumber.ToString(), "");

                return _order.Status;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Loc inactive 20191128
        //public static bool AutoCreateBundle(long? id, bool remove = false)
        //{
        //    try
        //    {
        //        var db = new WebDataModel();
        //        P_Member cMem = AppLB.Authority.GetCurrentMember();

        //        var order = db.O_Orders.Find(id);
        //        if (!remove)
        //        {
        //            var bundle = new I_Bundle
        //            {
        //                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
        //                BundleCode = "B" + order.OrdersCode,
        //                OrderCode = order.OrdersCode,
        //                MerchantName = order.CustomerName,
        //                Status = "Preparation to ship",
        //                CreateAt = DateTime.Now,
        //                CreateBy = cMem.FullName,
        //                UpdateAt = DateTime.Now,
        //                UpdateBy = cMem.FullName

        //            };
        //            order.BundelStatus = "Preparation to ship";
        //            db.I_Bundle.Add(bundle);
        //            var order_product = db.Order_Products.Where(op => op.OrderCode == order.OrdersCode).ToList();
        //            var newid = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
        //            foreach (var item in order_product)
        //            {
        //                var b_device = new I_Bundle_Device
        //                {
        //                    Id = newid++,
        //                    Bundle_Id = bundle.Id,
        //                    ProductCode = item.ProductCode,
        //                    ProductName = item.ProductName,
        //                    Features = item.Feature ?? "",
        //                    Quantity = item.Quantity
        //                };
        //                db.I_Bundle_Device.Add(b_device);
        //            }
        //            db.SaveChanges();
        //        }
        //        else
        //        {
        //            var bundel = db.I_Bundle.Where(b => b.OrderCode == order.OrdersCode).FirstOrDefault();
        //            var list_b_device = db.I_Bundle_Device.Where(bd => bd.Bundle_Id == bundel.Id).ToList();
        //            foreach (var item in list_b_device)
        //            {
        //                if (!string.IsNullOrEmpty(item.SerialNumbers))

        //                    foreach (var s in item.SerialNumbers.Split(','))
        //                    {
        //                        db.O_Device.Where(d => d.SerialNumber == s).FirstOrDefault().Inventory = 0;
        //                    }
        //            }
        //            db.I_Bundle_Device.RemoveRange(list_b_device);
        //            db.I_Bundle.Remove(bundel);
        //            db.SaveChanges();
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {

        //        throw e;
        //    }
        //}
    }
}