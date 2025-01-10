using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class KBController : Controller
    {
        // GET: KB
        public ActionResult Index()
        {
            var db = new WebDataModel();
            ViewBag.TicketCategory = (from cate in db.T_Project_Milestone
                                      where cate.Type == "category"
                                      select new T_TicketCategory_custom
                                      {
                                          name = cate.Name,
                                          id = cate.Id,
                                          parent = null,
                                          //Đếm số lượng ticket thuộc cate
                                          count = db.T_SupportTicket.Count(m => m.CategoryId == cate.Id && m.KB == true/*|| db.T_TicketCategory.Where(t => t.ParentCategoryId == cate.Id).Any(a => a.Id==m.CategoryId)*/)
                                      }).ToList();
            ViewBag.TicketNoCate = db.T_SupportTicket.Count(m => m.CategoryId == null && m.KB == true);

            var TicketList = (from ticket in db.T_SupportTicket
                              where ticket.KB == true
                              select new T_SupportTicketKB_custom
                              {
                                  Id = ticket.Id,
                                  name = ticket.Name,
                                  decs = ticket.Description.Substring(0, 30),
                                  categoryId = ticket.CategoryId
                              }).OrderBy(t => t.name).ToList();
            return View(TicketList);
        }

        public ActionResult KnowledgeBaseDetail(long? Id)
        {
            try
            {

                var db = new WebDataModel();
                var Ticket = db.T_SupportTicket.Find(Id);
                if (Ticket == null)
                    throw new Exception("Ticket is not found");
                ViewBag.TicketAttachFiles = db.UploadMoreFiles.Where(u => u.TableId == Id).ToList();
                ViewBag.Avatar = db.P_Member.Where(p => p.MemberNumber == Ticket.CreateByNumber).FirstOrDefault()?.Picture ?? "";
                ViewBag.ListFeedback = (from fb in db.T_TicketFeedback
                                        where fb.TicketId == Id && !string.IsNullOrEmpty(fb.Feedback)
                                        select new TicketFeedback_Custom
                                        {
                                            Id = fb.Id,
                                            Feedback = fb.Feedback,
                                            CreateBy = fb.CreateByName,
                                            CreateAt = fb.CreateAt,
                                            Avatar = db.P_Member.Where(p => p.MemberNumber == fb.CreateByNumber).FirstOrDefault().Picture ?? "",
                                            AttachFile = db.UploadMoreFiles.Where(u => u.TableId == fb.Id).ToList(),
                                            Attachment = fb.Attachments
                                        })
                                        .OrderByDescending(o => o.CreateAt)
                                        .ToList();


                return View(Ticket);
            }
            catch (Exception ex)
            {
                return RedirectToAction("err404", "home");
            }

        }
        public ActionResult SearchKB(string SearchText)
        {
            var db = new WebDataModel();
            var TicketList = (from ticket in db.T_SupportTicket
                              where (ticket.KB == true && (ticket.Name.ToLower().Contains(SearchText.ToLower()) ||
                                                            ticket.Description.ToLower().Contains(SearchText.ToLower()) ||
                                                            ticket.CreateByName.ToLower().Contains(SearchText.ToLower()) ||
                                                            ticket.Id.ToString().Contains(SearchText)))
                              select new T_SupportTicketKB_custom
                              {
                                  Id = ticket.Id,
                                  name = ticket.Name,
                                  categoryId = ticket.CategoryId
                              }).OrderBy(t => t.name).ToList();
            ViewBag.TicketNoCate = db.T_SupportTicket.Count(m =>string.IsNullOrEmpty(m.CategoryId) && m.KB == true && (m.Name.ToLower().Contains(SearchText.ToLower()) ||
                                                                                                        m.Description.ToLower().Contains(SearchText.ToLower()) ||
                                                                                                        m.CreateByName.ToLower().Contains(SearchText.ToLower()) ||
                                                                                                        m.Id.ToString().Contains(SearchText)));

            ViewBag.TicketCategory = (from cate in db.T_Project_Milestone.AsEnumerable()
                                      where cate.Type == "category"
                                      select new T_TicketCategory_custom
                                      {
                                          name = cate.Name,
                                          id = cate.Id,
                                          parent = null,
                                          //Đếm số lượng ticket thuộc cate
                                          count = TicketList.Where(m => m.categoryId == cate.Id).Count()
                                      }).OrderByDescending(o => o.name).ToList();
            return PartialView("_PartialKB", TicketList);
        }
    }
}