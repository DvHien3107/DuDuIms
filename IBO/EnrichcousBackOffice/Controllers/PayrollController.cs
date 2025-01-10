using DataTables.AspNet.Core;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.CustomizeModel.Payroll;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using PdfTextCoordinate;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly Dictionary<string, bool> access;
        private readonly P_Member cMem;
        private readonly IMailingService _mailingService;
        private readonly ILogService _logService;

        public PayrollController(IMailingService mailingService, ILogService logService)
        {
            _mailingService = mailingService;
            _logService = logService;
            access = AppLB.Authority.GetAccessAuthority();
            cMem = AppLB.Authority.GetCurrentMember();
        }

        #region PayRoll
        /// <summary>
        /// index
        /// </summary>
        public ActionResult PayRoll()
        {
            return View();
        }
        /// <summary>
        /// Change Tab
        /// </summary>
        /// <param name="TabName">Tab Name</param>
        public ActionResult ChangeTab(string TabName)
        {
            WebDataModel db = new WebDataModel();
            switch (TabName)
            {
                // tab payment paid
                case "payment-paid":

                    var currentYear = DateTime.UtcNow.ToString("yyyy");
                    var employeePayrollPayment = (from paidPayment in db.P_EmployeePayrollPayment where paidPayment.Month.Contains(currentYear) join bank in db.P_BankSupport on paidPayment.PaymentMethod equals bank.Code orderby paidPayment.PaymentDate descending select new { paidPayment, bank }).ToList().Select(x => new SelectListItem
                    {
                        Value = x.paidPayment.PaidNumber,
                        Text = x.bank.Name + " " + x.paidPayment.PaymentDate.Value.ToString("MMM d, yyyy")
                    }).ToList();
                    if ((access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true) && employeePayrollPayment.Count() == 0)
                    {
                        employeePayrollPayment.Add(new SelectListItem
                        {
                            Value = "",
                            Text = "N/A"
                        });
                    }
                    ViewBag.employeePayrollPayment = employeePayrollPayment;
                    return PartialView("_PaidPayment");
                // tab payslip
                case "payment-payslip":
                    currentYear = DateTime.UtcNow.ToString("yyyy");

                    return PartialView("_PaySlip");
                //tab payment pending
                default:
                    return PartialView("_PendingPayment");
            }
        }
        #region Payment Pending
        /// <summary>
        /// Detail member
        /// </summary>
        /// <param name="id">Member Number</param>
        public ActionResult Detail(string id)
        {
            WebDataModel db = new WebDataModel();
            var member = db.P_Member.Where(x => x.MemberNumber == id).Select(x => new DetailEmployeeCustomizeModel
            {
                MemberNumber = x.MemberNumber,
                FullName = x.FullName,
                Address = x.Address + ", " + x.City + ", " + x.State + ", " + x.ZipCode + ", " + x.Country,
                BaseSalary = x.BaseSalary,
                Phone = x.CellPhone,
                Email = x.Email1,
                Avatar = x.Picture != null ? x.Picture : (x.Gender == "Male" ? "/Upload/Img/Female.png" : "/Upload/Img/Male.png")
            }).FirstOrDefault();
            return View(member);
        }
        /// <summary>
        /// Load list datatable ajax for tab payment pending
        /// </summary>
        /// <param name="dataTablesRequest">Param of datatable jquery</param>
        /// <param name="SearchText">Search Text</param>
        /// <param name="DepartMentId">Department Id</param>
        /// <param name="Groups">Groups Id</param>
        /// <param name="Date">Month</param>
        [HttpPost]
        public ActionResult LoadListMember(IDataTablesRequest dataTablesRequest, string SearchText, string Status, string DepartMentId, string Groups, DateTime? Date)
        {
            WebDataModel db = new WebDataModel();
            int totalRecord = 0;
            decimal? totalPendingPayment = 0;
            //decimal? totalCommission = 0;
            decimal? totalAll = 0;
            string MonthPayroll = Date.Value.ToString("MM/yyyy");
            var query = from member in db.P_Member
                        where member.Active == true
                        && member.Delete != true
                        //&& !((from pr in db.P_EmployeePayrollPayment where pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year select pr).Any(a=>a.GroupMemberNumber.Contains(member.MemberNumber)))
                        //let setaspaid = db.P_EmployeePayrollPayment.Where(pr => pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year).Any(a => a.GroupMemberNumber.Contains(member.MemberNumber))
                        let prl = db.P_EmployeePayroll.Where(p => p.RecipientMemberNumber == member.MemberNumber && p.PayrollMonth == MonthPayroll).ToList()
                        select new { member, prl/*, setaspaid*/ };
            if ((access.Any(k => k.Key.Equals("payroll_viewall")) != true || access["payroll_viewall"] != true) && (access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true))
            {
                if ((access.Any(k => k.Key.Equals("payroll_view")) == true && access["payroll_view"] == true))
                {
                    query = query.Where(x => x.member.MemberNumber.Contains(cMem.MemberNumber));
                }
                else
                {
                    throw new Exception("access is denied");
                }
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.member.FullName.Contains(SearchText) || x.member.CellPhone.Contains(SearchText) || x.member.Email1.Contains(SearchText) || x.member.MemberNumber.Contains(SearchText));
            }
            if (!string.IsNullOrEmpty(Status))
            {
                if (Status == "NotYetCalculated")
                {
                    query = query.Where(x => x.prl.Count() == 0);
                }
                else if (Status == "NotPaid")
                {
                    query = query.Where(x => x.prl.Count() > 0 && x.prl.Where(y => !string.IsNullOrEmpty(y.PaidNumber)).Count() == 0);
                }
                else if (Status == "NotComplete")
                {
                    query = query.Where(x => x.prl.Count() > 0 && x.prl.Where(y => !string.IsNullOrEmpty(y.PaidNumber)).Count() > 0 && x.prl.Where(y => string.IsNullOrEmpty(y.PaidNumber)).Count() > 0);
                }
                else if (Status == "Completed")
                {
                    query = query.Where(x => x.prl.Count() > 0 && x.prl.Where(y => string.IsNullOrEmpty(y.PaidNumber)).Count() == 0);
                }
            }
            if (!string.IsNullOrEmpty(DepartMentId))
            {
                if (DepartMentId == "Other")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.member.DepartmentId));
                }
                else
                {
                    query = query.Where(x => x.member.DepartmentId.Contains(DepartMentId));
                    if (!string.IsNullOrEmpty(Groups))
                    {
                        if (Groups == "Other")
                        {
                            long DepartMentIdTypeLong = long.Parse(DepartMentId);
                            var ListGroupsExist = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentIdTypeLong);
                            List<string> ListMemberExistInDep = new List<string>();
                            foreach (var dep in ListGroupsExist)
                            {
                                if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                                {
                                    ListMemberExistInDep.AddRange(dep.GroupMemberNumber.Split(','));
                                }
                                if (!string.IsNullOrEmpty(dep.LeaderNumber))
                                {
                                    ListMemberExistInDep.Add(dep.LeaderNumber);
                                }
                                if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                                {
                                    ListMemberExistInDep.Add(dep.SupervisorNumber);
                                }
                            }
                            ListMemberExistInDep = ListMemberExistInDep.GroupBy(x => x).Select(x => x.Key).ToList();
                            query = from member in query where !(ListMemberExistInDep.Contains(member.member.MemberNumber)) select member;
                        }
                        else
                        {
                            List<string> ListMemberGroups = new List<string>();
                            var Group = (from dep in db.P_Department where dep.Id.ToString() == Groups select dep).FirstOrDefault();
                            if (!string.IsNullOrEmpty(Group?.GroupMemberNumber))
                            {
                                ListMemberGroups.AddRange(Group.GroupMemberNumber.Split(','));
                            }
                            if (!string.IsNullOrEmpty(Group?.LeaderNumber))
                            {
                                ListMemberGroups.Add(Group.LeaderNumber);
                            }
                            if (!string.IsNullOrEmpty(Group?.SupervisorNumber))
                            {
                                ListMemberGroups.Add(Group.SupervisorNumber);
                            }
                            ListMemberGroups.GroupBy(x => x).Select(x => x.Key);
                            query = from member in query where ListMemberGroups.Contains(member.member.MemberNumber) select member;
                        }
                    }
                }
            }
            totalRecord = query.Count();
            // totalBaseSalary = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true).FirstOrDefault().Amount);
            //totalCommission = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true && y.Type == "Commission").Sum(y => y.Amount));
            totalPendingPayment = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true && string.IsNullOrEmpty(y.PaidNumber)).Sum(y => y.Amount));
            totalAll = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true).Sum(y => y.Amount)) ?? 0;
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            if (colSearch == null)
            {
                query = query.OrderByDescending(s => s.member.CreateAt);
            }
            else
            {
                switch (colSearch.Name)
                {
                    case "MemberNumber":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.member.MemberNumber);
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.member.MemberNumber);
                        }
                        break;
                    case "MemberName":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.member.FullName);
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.member.FullName);
                        }
                        break;
                    case "BaseSalary":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).Sum(y => y.Amount));
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).Sum(y => y.Amount));
                        }
                        break;
                    case "Commission":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.prl.Where(y => y.Type == "Commission" && y.IsCalculate.Value).Sum(y => y.Amount));
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.prl.Where(y => y.Type == "Commission" && y.IsCalculate.Value).Sum(y => y.Amount));
                        }
                        break;
                    case "Total":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.prl.Where(y => y.IsCalculate.Value).Sum(y => y.Amount));
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.prl.Where(y => y.IsCalculate.Value).Sum(y => y.Amount));
                        }
                        break;
                    case "CalculateDate":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.prl.FirstOrDefault().CreatedAt);
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.prl.FirstOrDefault().CreatedAt);
                        }
                        break;
                    case "IsCalculated":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.prl.FirstOrDefault().IsCalculate);
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.prl.FirstOrDefault().IsCalculate);
                        }
                        break;
                    case "Paid":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.prl.FirstOrDefault().PaidNumber);
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.prl.FirstOrDefault().PaidNumber);
                        }
                        break;
                    default:
                        query = query.OrderByDescending(s => s.member.CreateAt);
                        break;
                }
            }
            query = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var ViewData = query.ToList().Select(x => new
            {
                MemberNumber = x.member.MemberNumber,
                FullName = x.member.FullName,
                Email = x.member.Email1,
                Address = x.member.Address + ", " + x.member.City + ", " + x.member.State + ", " + x.member.ZipCode + ", " + x.member.Country,
                Phone = x.member.CellPhone,
                //BaseSalary = (x.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).FirstOrDefault() != null ? x.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).FirstOrDefault().Amount : 0).Value.ToString("$#,##0.##"),
                //Commission = x.prl.Where(y => y.Type == "Commission" && y.IsCalculate.Value).Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                PendingPayment = x.prl.Where(y => y.IsCalculate.Value == true & string.IsNullOrEmpty(y.PaidNumber)).Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                Total = x.prl.Where(y => y.IsCalculate.Value).Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                Gender = x.member.Gender,
                JoinDate = x.member.JoinDate != null ? x.member.JoinDate.Value.ToString("dd/MM/yyyy") : "",
                Picture = x.member.Picture,
                DepartmentName = x.member.DepartmentName,
                IsCalculated = x.prl.Count() > 0,
                Paid = x.prl.Where(y => !string.IsNullOrEmpty(y.PaidNumber)).Count() > 0,
                PaidStatusName = x.prl.Where(y => string.IsNullOrEmpty(y.PaidNumber)).Count() > 0 ? "Not complete" : "Complete",
                CalculatedDate = x.prl.FirstOrDefault() != null ? x.prl.FirstOrDefault().UpdatedAt != null ? x.prl.FirstOrDefault().UpdatedAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : x.prl.FirstOrDefault().CreatedAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : "",
                CalculatedBy = x.prl.FirstOrDefault() != null ? x.prl.FirstOrDefault().UpdatedBy ?? x.prl.FirstOrDefault()?.CreatedBy : "",

            });
            return Json(new
            {
                totalPendingPayment = totalPendingPayment != null ? totalPendingPayment.Value.ToString("$#,##0.##") : "$0",
                //totalCommission = totalCommission != null ? totalCommission.Value.ToString("$#,##0.##") : "$0",
                totalAll = totalAll.Value.ToString("$#,##0.##"),
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                draw = dataTablesRequest.Draw,
                data = ViewData
            });
        }
        /// <summary>
        /// Update total for datatable then change commision in detail
        /// </summary>
        /// <param name="dataTablesRequest">Param of datatable jquery</param>
        /// <param name="Name">Member Name</param>
        /// <param name="Phone">Member Phone</param>
        /// <param name="DepartMentId">Department Id</param>
        /// <param name="Groups">Groups Id</param>
        /// <param name="Date">Month</param>
        [HttpPost]
        public ActionResult UpdateTotal(string SearchText, string CalculationStatus, string DepartMentId, string Groups, DateTime? Date)
        {
            WebDataModel db = new WebDataModel();
            decimal? totalPendingPayment = 0;
            // decimal? totalCommission = 0;
            decimal? totalAll = 0;
            string MonthPayroll = Date.Value.ToString("MM/yyyy");
            var query = from member in db.P_Member
                        where member.Active == true
                        && member.Delete != true
                        //&& !((from pr in db.P_EmployeePayrollPayment where pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year select pr).Any(a=>a.GroupMemberNumber.Contains(member.MemberNumber)))
                        //let setaspaid = db.P_EmployeePayrollPayment.Where(pr => pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year).Any(a => a.GroupMemberNumber.Contains(member.MemberNumber))
                        let prl = db.P_EmployeePayroll.Where(p => p.RecipientMemberNumber == member.MemberNumber && p.PayrollMonth == MonthPayroll).ToList()
                        select new { member, prl/*, setaspaid*/ };
            if ((access.Any(k => k.Key.Equals("payroll_viewall")) != true || access["payroll_viewall"] != true) && (access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true))
            {
                if ((access.Any(k => k.Key.Equals("payroll_view")) == true && access["payroll_view"] == true))
                {
                    query = query.Where(x => x.member.MemberNumber.Contains(cMem.MemberNumber));
                }
                else
                {
                    throw new Exception("access is denied");
                }
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.member.FullName.Contains(SearchText) || x.member.CellPhone.Contains(SearchText) || x.member.Email1.Contains(SearchText) || x.member.MemberNumber.Contains(SearchText));
            }
            if (!string.IsNullOrEmpty(CalculationStatus))
            {
                if (CalculationStatus == "NotCalculatedYet")
                {
                    query = query.Where(x => x.prl.Count() == 0);
                }
                else if (CalculationStatus == "Calculated")
                {
                    query = query.Where(x => x.prl.Count() > 0);
                }
            }
            if (!string.IsNullOrEmpty(DepartMentId))
            {
                if (DepartMentId == "Other")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.member.DepartmentId));
                }
                else
                {
                    query = query.Where(x => x.member.DepartmentId.Contains(DepartMentId));
                    if (!string.IsNullOrEmpty(Groups))
                    {
                        if (Groups == "Other")
                        {
                            long DepartMentIdTypeLong = long.Parse(DepartMentId);
                            var ListGroupsExist = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentIdTypeLong);
                            List<string> ListMemberExistInDep = new List<string>();
                            foreach (var dep in ListGroupsExist)
                            {
                                if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                                {
                                    ListMemberExistInDep.AddRange(dep.GroupMemberNumber.Split(','));
                                }
                                if (!string.IsNullOrEmpty(dep.LeaderNumber))
                                {
                                    ListMemberExistInDep.Add(dep.LeaderNumber);
                                }
                                if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                                {
                                    ListMemberExistInDep.Add(dep.SupervisorNumber);
                                }
                            }
                            ListMemberExistInDep = ListMemberExistInDep.GroupBy(x => x).Select(x => x.Key).ToList();
                            query = from member in query where !(ListMemberExistInDep.Contains(member.member.MemberNumber)) select member;
                        }
                        else
                        {
                            List<string> ListMemberGroups = new List<string>();
                            var Group = (from dep in db.P_Department where dep.Id.ToString() == Groups select dep).FirstOrDefault();
                            if (!string.IsNullOrEmpty(Group?.GroupMemberNumber))
                            {
                                ListMemberGroups.AddRange(Group.GroupMemberNumber.Split(','));
                            }
                            if (!string.IsNullOrEmpty(Group?.LeaderNumber))
                            {
                                ListMemberGroups.Add(Group.LeaderNumber);
                            }
                            if (!string.IsNullOrEmpty(Group?.SupervisorNumber))
                            {
                                ListMemberGroups.Add(Group.SupervisorNumber);
                            }
                            ListMemberGroups.GroupBy(x => x).Select(x => x.Key);
                            query = from member in query where ListMemberGroups.Contains(member.member.MemberNumber) select member;
                        }
                    }
                }
            }
            //totalBaseSalary = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true && y.Type == "BaseSalary").FirstOrDefault().Amount);
            //totalCommission = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true && y.Type == "Commission").Sum(y => y.Amount));
            totalPendingPayment = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true && string.IsNullOrEmpty(y.PaidNumber)).Sum(y => y.Amount));
            totalAll = query.Sum(x => x.prl.Where(y => y.IsCalculate.Value == true).Sum(y => y.Amount)) ?? 0;
            return Json(new
            {
                totalPendingPayment = totalPendingPayment != null ? totalPendingPayment.Value.ToString("$#,##0.##") : "$0",
                //totalCommission = totalCommission != null ? totalCommission.Value.ToString("$#,##0.##") : "$0",
                totalAll = totalAll.Value.ToString("$#,##0.##"),
            });
        }
        /// <summary>
        /// refresh row for datatable when change data
        /// </summary>
        /// <param name="MemberNumber">Member Number</param>
        /// <param name="Date">Month</param>
        [HttpPost]
        public ActionResult UpdateRow(string MemberNumber, DateTime? Date)
        {
            WebDataModel db = new WebDataModel();
            string MonthPayroll = Date.Value.ToString("MM/yyyy");
            var x = (from member in db.P_Member
                     where member.MemberNumber == MemberNumber
                     //&& !((from pr in db.P_EmployeePayrollPayment where pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year select pr).Any(a=>a.GroupMemberNumber.Contains(member.MemberNumber)))
                     //let setaspaid = db.P_EmployeePayrollPayment.Where(pr => pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year).Any(a => a.GroupMemberNumber.Contains(member.MemberNumber))
                     let prl = db.P_EmployeePayroll.Where(p => p.RecipientMemberNumber == member.MemberNumber && p.PayrollMonth == MonthPayroll).ToList()
                     select new { member, prl/*, setaspaid*/ }).FirstOrDefault();
            var ViewData = new
            {
                MemberNumber = x.member.MemberNumber,
                FullName = x.member.FullName,
                Email = x.member.Email1,
                Address = x.member.Address + ", " + x.member.City + ", " + x.member.State + ", " + x.member.ZipCode + ", " + x.member.Country,
                Phone = x.member.CellPhone,
                //BaseSalary = (x.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).FirstOrDefault() != null ? x.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).FirstOrDefault().Amount : 0).Value.ToString("$#,##0.##"),
                //Commission = x.prl.Where(y => y.Type == "Commission" && y.IsCalculate.Value).Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                PendingPayment = x.prl.Where(y => y.IsCalculate.Value == true & string.IsNullOrEmpty(y.PaidNumber)).Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                Total = x.prl.Where(y => y.IsCalculate.Value).Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                Gender = x.member.Gender,
                JoinDate = x.member.JoinDate != null ? x.member.JoinDate.Value.ToString("dd/MM/yyyy") : "",
                Picture = x.member.Picture,
                DepartmentName = x.member.DepartmentName,
                IsCalculated = x.prl.Count() > 0,
                Paid = x.prl.Where(y => !string.IsNullOrEmpty(y.PaidNumber)).Count() > 0,
                PaidStatusName = x.prl.Where(y => string.IsNullOrEmpty(y.PaidNumber)).Count() > 0 ? "Not complete" : "Complete",
                CalculatedDate = x.prl.FirstOrDefault() != null ? x.prl.FirstOrDefault().UpdatedAt != null ? x.prl.FirstOrDefault().UpdatedAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : x.prl.FirstOrDefault().CreatedAt.Value.ToString("dd/MM/yyyy hh:mm:ss") : "",
                CalculatedBy = x.prl.FirstOrDefault() != null ? x.prl.FirstOrDefault().UpdatedBy ?? x.prl.FirstOrDefault()?.CreatedBy : "",
            };
            return Json(ViewData);
        }
        /// <summary>
        /// calculate payroll for all employee selected
        /// </summary>
        /// <param name="MemberNumber">Member Number</param>
        /// <param name="CommissionSelected">Id employee commission</param>
        [HttpPost]
        public async Task<ActionResult> PaymentProcess(DateTime? Date, string MemberNumber, string[] CommissionSelected/*,string PaymentMethod, string PaidNumber, string CardNumber, string Description*/)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    WebDataModel db = new WebDataModel();
                    string TypeEmployeeSetting = CommSetting.Employee.Text();
                    string LevelMember = LevelCommSetting.Member.Text();
                    string LevelLeader = LevelCommSetting.Leader.Text();
                    string LevelManager = LevelCommSetting.Manager.Text();
                    string LevelDirector = LevelCommSetting.Director.Text();
                    decimal? CommMemberPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelMember).FirstOrDefault().CommPercent;
                    decimal? CommLeaderPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelLeader).FirstOrDefault().CommPercent;
                    decimal? CommManagerPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelManager).FirstOrDefault().CommPercent;
                    decimal? CommDirectorPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelDirector).FirstOrDefault().CommPercent;
                    var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
                    var CurrentPayrollMonth = Date.Value.ToString("MM/yyyy");
                    //check exist 
                    var BaseSalaryIsCalculated = db.P_EmployeePayroll.Where(x => x.PayrollMonth == CurrentPayrollMonth && x.Type == "BaseSalary" && x.RecipientMemberNumber == MemberNumber).FirstOrDefault();
                    if (BaseSalaryIsCalculated != null)
                    {
                        if (string.IsNullOrEmpty(BaseSalaryIsCalculated.PaidNumber))
                        {
                            db.P_EmployeePayroll.Remove(BaseSalaryIsCalculated);
                            var EmployeePayrollBaseSalary = new P_EmployeePayroll
                            {
                                Amount = member.BaseSalary,
                                Type = "BaseSalary",
                                RecipientName = member.FullName,
                                RecipientMemberNumber = member.MemberNumber,
                                PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                CreatedAt = DateTime.UtcNow,
                                IsCalculate = true,
                                CreatedBy = cMem.FullName,
                            };
                            db.P_EmployeePayroll.Add(EmployeePayrollBaseSalary);
                        }
                        else
                        {
                            BaseSalaryIsCalculated.UpdatedAt = DateTime.UtcNow;
                            BaseSalaryIsCalculated.UpdatedBy = cMem.FullName;
                        }
                    }
                    else
                    {
                        var EmployeePayrollBaseSalary = new P_EmployeePayroll
                        {
                            Amount = member.BaseSalary,
                            Type = "BaseSalary",
                            RecipientName = member.FullName,
                            RecipientMemberNumber = member.MemberNumber,
                            PayrollMonth = Date.Value.ToString("MM/yyyy"),
                            CreatedAt = DateTime.UtcNow,
                            IsCalculate = true,
                            CreatedBy = cMem.FullName,
                        };
                        db.P_EmployeePayroll.Add(EmployeePayrollBaseSalary);
                    }

                    var ListPayrollComission = new List<P_EmployeePayroll>();
                    if (member.DepartmentName?.Contains("SALES & MARKETING") == true)
                    {
                        // calculate commission for type member
                        var QueryCustrans = from tran in db.C_CustomerTransaction
                                            where tran.CreateAt.Value.Month == Date.Value.Month && tran.CreateAt.Value.Year == Date.Value.Year && tran.PaymentStatus == "Success"
                                            join order in db.O_Orders on tran.OrdersCode equals order.OrdersCode
                                            where order.SalesMemberNumber == MemberNumber
                                            let pr = db.P_EmployeePayroll.Where(x => x.ContractId == order.OrdersCode && x.CustomerTransactionId == tran.Id && x.Type == "Commission" && x.TypeComm == LevelMember).FirstOrDefault()
                                            select new { tran, order, pr };
                        foreach (var x in QueryCustrans.ToList())
                        {
                            if (x.pr != null)
                            {
                                if (string.IsNullOrEmpty(x.pr.PaidNumber))
                                {
                                    db.P_EmployeePayroll.Remove(x.pr);
                                    ListPayrollComission.Add(new P_EmployeePayroll
                                    {
                                        CustomerTransactionId = x.tran.Id,
                                        TransactionAmount = x.tran.Amount,
                                        Amount = x.tran.Amount * CommMemberPercent / 100,
                                        Type = "Commission",
                                        TypeComm = LevelMember,
                                        RecipientName = member.FullName,
                                        RecipientMemberNumber = member.MemberNumber,
                                        ContractId = x.order.OrdersCode,
                                        CustomerName = x.order.CustomerName,
                                        PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                        CreatedAt = DateTime.UtcNow,
                                        IsCalculate = true,
                                        CreatedBy = cMem.FullName,
                                    });
                                }
                                else
                                {
                                    x.pr.UpdatedBy = cMem.FullName;
                                    x.pr.UpdatedAt = DateTime.UtcNow;
                                }

                            }
                            else
                            {
                                ListPayrollComission.Add(new P_EmployeePayroll
                                {
                                    CustomerTransactionId = x.tran.Id,
                                    TransactionAmount = x.tran.Amount,
                                    Amount = x.tran.Amount * CommMemberPercent / 100,
                                    Type = "Commission",
                                    TypeComm = LevelMember,
                                    RecipientName = member.FullName,
                                    RecipientMemberNumber = member.MemberNumber,
                                    ContractId = x.order.OrdersCode,
                                    CustomerName = x.order.CustomerName,
                                    PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                    CreatedAt = DateTime.UtcNow,
                                    IsCalculate = true,
                                    CreatedBy = cMem.FullName,
                                });
                            }
                        }

                        // calculate commission for type leader 
                        var LeaderDep = db.P_Department.Where(x => x.LeaderNumber == MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                        List<string> MemberOfLeader = new List<string>();
                        if (LeaderDep.Count() > 0)
                        {
                            foreach (var dep in LeaderDep)
                            {
                                if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                                {
                                    MemberOfLeader.AddRange(dep.GroupMemberNumber.Split(','));
                                }
                            }
                            MemberOfLeader = MemberOfLeader.GroupBy(x => x).Select(x => x.Key).ToList();
                            var ListTransByLeader = (from tran in db.C_CustomerTransaction
                                                     where tran.CreateAt.Value.Month == Date.Value.Month && tran.CreateAt.Value.Year == Date.Value.Year && tran.PaymentStatus == "Success"
                                                     join order in db.O_Orders on tran.OrdersCode equals order.OrdersCode
                                                     where order.CreatedAt.Value.Month == Date.Value.Month && order.CreatedAt.Value.Year == Date.Value.Year
                                                     join mb in MemberOfLeader on order.SalesMemberNumber equals mb
                                                     let pr = db.P_EmployeePayroll.Where(x => x.ContractId == order.OrdersCode && x.CustomerTransactionId == tran.Id && x.Type == "Commission" && x.TypeComm == LevelLeader).FirstOrDefault()
                                                     select new { tran, pr, order }).ToList();
                            foreach (var x in ListTransByLeader.ToList())
                            {
                                if (x.pr != null)
                                {
                                    if (string.IsNullOrEmpty(x.pr.PaidNumber))
                                    {
                                        db.P_EmployeePayroll.Remove(x.pr);
                                        ListPayrollComission.Add(new P_EmployeePayroll
                                        {
                                            CustomerTransactionId = x.tran.Id,
                                            TransactionAmount = x.tran.Amount,
                                            Amount = x.tran.Amount * CommLeaderPercent / 100,
                                            Type = "Commission",
                                            TypeComm = LevelLeader,
                                            RecipientName = member.FullName,
                                            RecipientMemberNumber = member.MemberNumber,
                                            ContractId = x.order.OrdersCode,
                                            CustomerName = x.order.CustomerName,
                                            PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                            CreatedAt = DateTime.UtcNow,
                                            CreatedBy = cMem.FullName,
                                            IsCalculate = true,
                                        });
                                    }
                                    else
                                    {
                                        x.pr.UpdatedBy = cMem.FullName;
                                        x.pr.UpdatedAt = DateTime.UtcNow;
                                    }
                                }
                                else
                                {
                                    ListPayrollComission.Add(new P_EmployeePayroll
                                    {
                                        CustomerTransactionId = x.tran.Id,
                                        TransactionAmount = x.tran.Amount,
                                        Amount = x.tran.Amount * CommLeaderPercent / 100,
                                        Type = "Commission",
                                        TypeComm = LevelLeader,
                                        RecipientName = member.FullName,
                                        RecipientMemberNumber = member.MemberNumber,
                                        ContractId = x.order.OrdersCode,
                                        CustomerName = x.order.CustomerName,
                                        PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                        CreatedAt = DateTime.UtcNow,
                                        CreatedBy = cMem.FullName,
                                        IsCalculate = true,
                                    });
                                }
                            }

                        }
                    }
                    // calculate commission for type manager
                    var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                    List<string> MemberOfManager = new List<string>();
                    if (ManagerDep.Count() > 0)
                    {
                        foreach (var dep in ManagerDep)
                        {
                            if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                            {
                                MemberOfManager.AddRange(dep.GroupMemberNumber.Split(','));
                            }
                        }
                        MemberOfManager = MemberOfManager.GroupBy(x => x).Select(x => x.Key).ToList();
                        var ListTransByManager = (from tran in db.C_CustomerTransaction
                                                  where tran.CreateAt.Value.Month == Date.Value.Month && tran.CreateAt.Value.Year == Date.Value.Year && tran.PaymentStatus == "Success"
                                                  join order in db.O_Orders on tran.OrdersCode equals order.OrdersCode
                                                  where order.CreatedAt.Value.Month == Date.Value.Month && order.CreatedAt.Value.Year == Date.Value.Year
                                                  join mb in MemberOfManager on order.SalesMemberNumber equals mb
                                                  let pr = db.P_EmployeePayroll.Where(x => x.ContractId == order.OrdersCode && x.CustomerTransactionId == tran.Id && x.Type == "Commission" && x.TypeComm == LevelManager).FirstOrDefault()
                                                  select new { tran, pr, order }).ToList();

                        foreach (var x in ListTransByManager.ToList())
                        {
                            if (x.pr != null)
                            {
                                if (string.IsNullOrEmpty(x.pr.PaidNumber))
                                {
                                    db.P_EmployeePayroll.Remove(x.pr);
                                    ListPayrollComission.Add(new P_EmployeePayroll
                                    {
                                        TransactionAmount = x.tran.Amount,
                                        CustomerTransactionId = x.tran.Id,
                                        Amount = x.tran.Amount * CommManagerPercent / 100,
                                        Type = "Commission",
                                        TypeComm = LevelManager,
                                        RecipientName = member.FullName,
                                        RecipientMemberNumber = member.MemberNumber,
                                        ContractId = x.order.OrdersCode,
                                        CustomerName = x.order.CustomerName,
                                        PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                        CreatedAt = DateTime.UtcNow,
                                        CreatedBy = cMem.FullName,
                                        IsCalculate = true,
                                    });
                                }
                                else
                                {
                                    x.pr.UpdatedBy = cMem.FullName;
                                    x.pr.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                            else
                            {

                                ListPayrollComission.Add(new P_EmployeePayroll
                                {
                                    TransactionAmount = x.tran.Amount,
                                    CustomerTransactionId = x.tran.Id,
                                    Amount = x.tran.Amount * CommManagerPercent / 100,
                                    Type = "Commission",
                                    TypeComm = LevelManager,
                                    RecipientName = member.FullName,
                                    RecipientMemberNumber = member.MemberNumber,
                                    ContractId = x.order.OrdersCode,
                                    CustomerName = x.order.CustomerName,
                                    PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = cMem.FullName,
                                    IsCalculate = true,
                                });
                            }
                        }
                    }
                    // calculate commission for type director 
                    var DirectorDep = db.P_Department.Where(x => x.ParentDepartmentId == null && x.LeaderNumber == MemberNumber).ToList();
                    List<string> MemberOfDirector = new List<string>();
                    if (DirectorDep.Count() > 0)
                    {
                        foreach (var drtordep in DirectorDep)
                        {
                            var dep = db.P_Department.Where(x => x.ParentDepartmentId == drtordep.Id && x.Type == "SALES").ToList();
                            if (dep.Count() > 0)
                            {
                                foreach (var d in dep)
                                {
                                    if (!string.IsNullOrEmpty(d.GroupMemberNumber))
                                    {
                                        MemberOfDirector.AddRange(d.GroupMemberNumber.Split(','));
                                    }
                                }
                            }
                        }
                        MemberOfDirector = MemberOfDirector.GroupBy(x => x).Select(x => x.Key).ToList();
                        var ListTransByDirector = (from tran in db.C_CustomerTransaction
                                                   where tran.CreateAt.Value.Month == Date.Value.Month && tran.CreateAt.Value.Year == Date.Value.Year && tran.PaymentStatus == "Success"
                                                   join order in db.O_Orders on tran.OrdersCode equals order.OrdersCode
                                                   where order.CreatedAt.Value.Month == Date.Value.Month && order.CreatedAt.Value.Year == Date.Value.Year
                                                   join mb in MemberOfDirector on order.SalesMemberNumber equals mb
                                                   let pr = db.P_EmployeePayroll.Where(x => x.ContractId == order.OrdersCode && x.CustomerTransactionId == tran.Id && x.Type == "Commission" && x.TypeComm == LevelDirector).FirstOrDefault()
                                                   select new { tran, pr, order }).ToList();
                        foreach (var x in ListTransByDirector.ToList())
                        {
                            if (x.pr != null)
                            {
                                if (string.IsNullOrEmpty(x.pr.PaidNumber))
                                {
                                    db.P_EmployeePayroll.Remove(x.pr);
                                    ListPayrollComission.Add(new P_EmployeePayroll
                                    {
                                        TransactionAmount = x.tran.Amount,
                                        CustomerTransactionId = x.tran.Id,
                                        Amount = x.tran.Amount * CommDirectorPercent / 100,
                                        Type = "Commission",
                                        TypeComm = LevelDirector,
                                        RecipientName = member.FullName,
                                        RecipientMemberNumber = member.MemberNumber,
                                        ContractId = x.order.OrdersCode,
                                        CustomerName = x.order.CustomerName,
                                        PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                        CreatedAt = DateTime.UtcNow,
                                        CreatedBy = cMem.FullName,
                                        IsCalculate = true,
                                    });
                                }
                                else
                                {
                                    x.pr.UpdatedBy = cMem.FullName;
                                    x.pr.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                ListPayrollComission.Add(new P_EmployeePayroll
                                {
                                    TransactionAmount = x.tran.Amount,
                                    CustomerTransactionId = x.tran.Id,
                                    Amount = x.tran.Amount * CommDirectorPercent / 100,
                                    Type = "Commission",
                                    TypeComm = LevelDirector,
                                    RecipientName = member.FullName,
                                    RecipientMemberNumber = member.MemberNumber,
                                    ContractId = x.order.OrdersCode,
                                    CustomerName = x.order.CustomerName,
                                    PayrollMonth = Date.Value.ToString("MM/yyyy"),
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = cMem.FullName,
                                    IsCalculate = true,
                                });

                            }
                        }

                    }
                    db.P_EmployeePayroll.AddRange(ListPayrollComission);
                    await db.SaveChangesAsync();
                    scope.Complete();
                    return Json(true);
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                    _writeLogErrorService.InsertLogError(ex);
                    return Json(false);
                }
            }
        }
        /// <summary>
        /// detail payroll
        /// </summary>
        /// <param name="MemberNumber">Member Number</param>
        /// <param name="Date">Date</param>
        public ActionResult DetailPayRoll(string MemberNumber, DateTime Date)
        {
            WebDataModel db = new WebDataModel();
            string TypeEmployeeSetting = CommSetting.Employee.Text();
            string LevelMember = LevelCommSetting.Member.Text();
            string LevelLeader = LevelCommSetting.Leader.Text();
            string LevelManager = LevelCommSetting.Manager.Text();
            string LevelDirector = LevelCommSetting.Director.Text();
            decimal? CommMemberPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelMember).FirstOrDefault().CommPercent;
            decimal? CommLeaderPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelLeader).FirstOrDefault().CommPercent;
            decimal? CommManagerPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelManager).FirstOrDefault().CommPercent;
            decimal? CommDirectorPercent = db.P_CommEmployeeSetting.Where(x => x.Type == TypeEmployeeSetting && x.LevelName == LevelDirector).FirstOrDefault().CommPercent;
            var Month = Date.ToString("MM/yyyy");
            var Member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
            bool? IsCalculate = false;
            var ListComission = new List<EmployeeComission>();
            IsCalculate = db.P_EmployeePayroll.Where(x => x.PayrollMonth == Month && x.RecipientMemberNumber == MemberNumber).Count() > 0;
            ViewBag.IsCalculate = IsCalculate;
            ViewBag.IsComplete = db.P_EmployeePayroll.Where(x => x.RecipientMemberNumber == MemberNumber && x.PayrollMonth == Month && string.IsNullOrEmpty(x.PaidNumber)).Count() == 0;
            if (IsCalculate.Value)
            {
                var QueryCustrans = from emppr in db.P_EmployeePayroll
                                    where emppr.PayrollMonth == Month && emppr.Type == "Commission" && emppr.RecipientMemberNumber == MemberNumber
                                    join order in db.O_Orders on emppr.ContractId equals order.OrdersCode
                                    join tran in db.C_CustomerTransaction on emppr.CustomerTransactionId equals tran.Id
                                    let typeSubcrition = db.Order_Subcription.Where(x => x.OrderCode == order.OrdersCode).Count() > 0 ? "Service" : ""
                                    let typeHardWare = db.Order_Products.Where(x => x.OrderCode == order.OrdersCode).Count() > 0 ? "Hardware" : ""
                                    select new { emppr, order, tran, typeSubcrition, typeHardWare };
                ListComission.AddRange(QueryCustrans.Select(x => new EmployeeComission
                {
                    EmpPayrollId = x.emppr.Id,
                    OrderCode = x.order.OrdersCode,
                    Status = x.order.Status,
                    TypeOfInvoice = x.typeSubcrition + ((!string.IsNullOrEmpty(x.typeHardWare) && !string.IsNullOrEmpty(x.typeSubcrition)) ? " & " : "") + x.typeHardWare,
                    CustomerName = x.order.CustomerName,
                    Amount = x.emppr.TransactionAmount,
                    TransactionId = x.emppr.CustomerTransactionId,
                    SalesPersonName = x.order.SalesName,
                    SalesPersonNumber = x.order.SalesMemberNumber,
                    Type = x.emppr.TypeComm,
                    Comisssion = x.emppr.Amount,
                    PaidCommission = x.emppr.Amount ?? 0,
                    IsCalculated = x.emppr.IsCalculate,
                    Paid = !string.IsNullOrEmpty(x.emppr.PaidNumber) ? true : false,
                    //BalanceCommissionAmount = x.pr == null ? 0 : !string.IsNullOrEmpty(x.pr.PaidNumber) ? (x.emppr.Amount * CommMemberPercent) / 100 - x.pr.Amount : 0,
                }).ToList()); ;
                var PaidBaseSalary = db.P_EmployeePayroll.Where(x => x.RecipientMemberNumber == MemberNumber && x.Type == "BaseSalary" && x.PayrollMonth == Month).FirstOrDefault();
                bool SetAsPaid = db.P_EmployeePayrollPayment.Where(x => x.Month == Month && x.GroupMemberNumber.Contains(MemberNumber)).Count() > 0;
                var ViewModel = new DetailEmployeePayrollCustomizeModel()
                {
                    EmpPayrollBaseSalaryId = PaidBaseSalary.Id,
                    EmployeeId = Member.MemberNumber,
                    EmployeeName = Member.FullName,
                    BaseSalary = PaidBaseSalary == null ? Member.BaseSalary : PaidBaseSalary.Amount,
                    IsCalculatedBaseSalary = PaidBaseSalary == null ? false : PaidBaseSalary.IsCalculate.Value,
                    PaidBaseSalary = PaidBaseSalary == null ? false : !string.IsNullOrEmpty(PaidBaseSalary.PaidNumber) ? true : false,
                    Address = Member.Address + ", " + Member.City + ", " + Member.State + ", " + Member.ZipCode + ", " + Member.Country,
                    Phone = Member.CellPhone,
                    SetAsPaid = SetAsPaid,
                    Email = Member.Email1,
                    Department = Member.DepartmentName,
                    IsSalesPerson = Member.DepartmentName?.Contains("SALES & MARKETING") ?? false,
                    ListEmployeeComission = ListComission,
                    PercentCommission = new PercentCommission
                    {
                        PercentComissionMember = CommMemberPercent
                    }
                };
                return PartialView("_DetailPayroll", ViewModel);
            }
            else
            {
                var ViewModel = new DetailEmployeePayrollCustomizeModel()
                {
                    EmployeeId = Member.MemberNumber,
                    EmployeeName = Member.FullName,
                    BaseSalary = Member.BaseSalary,
                    Address = Member.Address + ", " + Member.City + ", " + Member.State + ", " + Member.ZipCode + ", " + Member.Country,
                    Phone = Member.CellPhone,
                    Email = Member.Email1,
                    Department = Member.DepartmentName,
                    IsSalesPerson = Member.DepartmentName?.Contains("SALES & MARKETING") ?? false,
                    ListEmployeeComission = ListComission,
                    PercentCommission = new PercentCommission
                    {
                        PercentComissionMember = CommMemberPercent
                    }
                };
                return PartialView("_DetailPayroll", ViewModel);
            }
        }
        //public decimal? ComissionTotal(string MemberNumber, DateTime? Date)
        //{
        //    WebDataModel db = new WebDataModel();
        //    string MonthPayroll = Date.Value.ToString("MM/yyyy");
        //    var commissionCalculated = db.P_EmployeePayroll.Where(x => x.Type == "Commission" && x.RecipientMemberNumber == MemberNumber && x.PayrollMonth == MonthPayroll).Sum(x => x.Amount);
        //    return commissionCalculated;
        //}

        /// <summary>
        /// Set as paid all employee selected 
        /// </summary>
        /// <param name="ListMemberNumber">List Member number selected</param>
        /// <param name="PaymentMethod">Payment Method</param>
        /// <param name="CardNumber">Card number (4 number last digit)</param>
        /// <param name="Date">Date</param>
        /// <param name="PaymentDate">Payment Date</param>
        /// <param name="Comment">Description Payment</param>
        [HttpPost]
        public ActionResult SetAsPaidMultiEmp(string ListMemberNumber, string PaymentMethod/*, string CardNumber*/, string Comment, string Description, DateTime? Date, DateTime PaymentDate)
        {
            WebDataModel db = new WebDataModel();
            var ListEmp = ListMemberNumber.Split(',');
            string Month = Date.Value.ToString("MM/yyyy");
            var member = from mb in db.P_Member join listmb in ListEmp on mb.MemberNumber equals listmb select mb;
            List<string> GroupsMemberNumber = new List<string>();
            List<string> GroupsMemberName = new List<string>();
            GroupsMemberNumber = member.Select(x => x.MemberNumber).ToList();
            GroupsMemberName = member.Select(x => x.FullName).ToList();
            var EmpPayroll = db.P_EmployeePayrollPayment.Where(x => x.Month == Month && DbFunctions.TruncateTime(x.PaymentDate) == PaymentDate && x.PaymentMethod == PaymentMethod).FirstOrDefault();
            if (EmpPayroll != null)
            {
                List<string> GroupsMemberNumberSetAsPaid = new List<string>();
                List<string> GroupsMemberNameSetAsPaid = new List<string>();
                foreach (var mb in member)
                {
                    var ListPayRoll = db.P_EmployeePayroll.Where(x => x.RecipientMemberNumber == mb.MemberNumber && x.PayrollMonth == Month && x.IsCalculate.Value == true && string.IsNullOrEmpty(x.PaidNumber)).ToList();
                    if (ListPayRoll.Count() > 0 && ListPayRoll.Sum(x => x.Amount) > 0)
                    {
                        GroupsMemberNumberSetAsPaid.Add(mb.MemberNumber);
                        GroupsMemberNameSetAsPaid.Add(mb.FullName);
                        foreach (var pr in ListPayRoll)
                        {
                            pr.PaidNumber = EmpPayroll.PaidNumber;
                        }
                    }
                }
                if (GroupsMemberNumberSetAsPaid.Count > 0)
                {
                    EmpPayroll.GroupMemberName = EmpPayroll.GroupMemberName + "," + string.Join(",", GroupsMemberNameSetAsPaid);
                    EmpPayroll.GroupMemberNumber = EmpPayroll.GroupMemberNumber + "," + string.Join(",", GroupsMemberNumberSetAsPaid);
                    //EmpPayroll.CardNumber = EmpPayroll.CardNumber + "|" + CardNumber;
                    EmpPayroll.Comment = EmpPayroll.Comment + "|" + Comment;
                    EmpPayroll.UpdateAt = DateTime.UtcNow;
                    EmpPayroll.UpdateBy = cMem.FullName;
                }
                db.SaveChanges();
                return Json(true);
            }
            else
            {
                var PaidNumber = CreatePaidNumber(DateTime.Now.ToString("yyyyMM"));
                List<string> GroupsMemberNumberSetAsPaid = new List<string>();
                List<string> GroupsMemberNameSetAsPaid = new List<string>();
                foreach (var mb in member)
                {
                    var ListPayRoll = db.P_EmployeePayroll.Where(x => x.RecipientMemberNumber == mb.MemberNumber && x.PayrollMonth == Month && x.IsCalculate.Value == true && string.IsNullOrEmpty(x.PaidNumber)).ToList();
                    if (ListPayRoll.Count() > 0 && ListPayRoll.Sum(x => x.Amount) > 0)
                    {
                        GroupsMemberNumberSetAsPaid.Add(mb.MemberNumber);
                        GroupsMemberNameSetAsPaid.Add(mb.FullName);
                        foreach (var pr in ListPayRoll)
                        {
                            pr.PaidNumber = PaidNumber;
                        }
                    }
                }
                if (GroupsMemberNumberSetAsPaid.Count > 0)
                {
                    var EmpPayrollPayment = new P_EmployeePayrollPayment
                    {
                        PaidNumber = PaidNumber,
                        PaymentMethod = PaymentMethod,
                        GroupMemberName = string.Join(",", GroupsMemberNameSetAsPaid),
                        GroupMemberNumber = string.Join(",", GroupsMemberNumberSetAsPaid),
                        //CardNumber = CardNumber,
                        Description = Description,
                        PaymentDate = PaymentDate,
                        CreateAt = DateTime.UtcNow,
                        Comment = Comment,
                        Month = Month,
                        CreateBy = cMem.FullName
                    };
                    db.P_EmployeePayrollPayment.Add(EmpPayrollPayment);
                }
                db.SaveChanges();
                return Json(true);
            }
        }
        /// <summary>
        /// Set as paid by EmployeeCommission Id in detail payroll when set as paid 
        /// </summary>
        /// <param name="EmpPayrollIds">List Id EmployeeCommission Id</param>
        /// <param name="PaymentMethod">Payment Method</param>
        /// <param name="CardNumber">Card number (4 number last digit)</param>
        /// <param name="Date">Date</param>
        /// <param name="PaymentDate">Payment Date</param>
        /// <param name="Comment">Description Payment</param>
        public ActionResult SetAsPaidByEmpPayroll(string EmpPayrollIds, string PaymentMethod/*, string CardNumber*/, string Comment, string Description, DateTime? Date, DateTime PaymentDate)
        {
            WebDataModel db = new WebDataModel();
            var ListPr = EmpPayrollIds.Split(',').Select(int.Parse).ToArray();
            string Month = Date.Value.ToString("MM/yyyy");
            var member = from listpr in ListPr join pr in db.P_EmployeePayroll on listpr equals pr.Id join mb in db.P_Member on pr.RecipientMemberNumber equals mb.MemberNumber select new { pr, mb };
            string MemberNumber;
            string MemberName;
            MemberNumber = member.Select(x => x.mb.MemberNumber).FirstOrDefault();
            MemberName = member.Select(x => x.mb.FullName).FirstOrDefault();
            var EmpPayroll = db.P_EmployeePayrollPayment.Where(x => x.Month == Month && DbFunctions.TruncateTime(x.PaymentDate) == PaymentDate && x.PaymentMethod == PaymentMethod).FirstOrDefault();
            if (EmpPayroll != null)
            {
                if (!EmpPayroll.GroupMemberNumber.Contains(MemberNumber))
                {
                    EmpPayroll.GroupMemberName = EmpPayroll.GroupMemberName + "," + MemberName;
                    EmpPayroll.GroupMemberNumber = EmpPayroll.GroupMemberNumber + "," + MemberNumber;
                }
                //EmpPayroll.CardNumber = EmpPayroll.CardNumber + "|" + CardNumber;
                EmpPayroll.Comment = EmpPayroll.Comment + "|" + Comment;
                EmpPayroll.UpdateAt = DateTime.UtcNow;
                EmpPayroll.UpdateBy = cMem.FullName;
                foreach (var pr in ListPr)
                {
                    var payroll = db.P_EmployeePayroll.Where(x => x.Id == pr).FirstOrDefault();
                    if (payroll != null)
                    {
                        payroll.PaidNumber = EmpPayroll.PaidNumber;
                    }
                }
                db.SaveChanges();
                return Json(true);
            }
            else
            {
                var PaidNumber = CreatePaidNumber(DateTime.Now.ToString("yyyyMM"));
                var EmpPayrollPayment = new P_EmployeePayrollPayment
                {
                    PaidNumber = PaidNumber,
                    PaymentMethod = PaymentMethod,
                    GroupMemberName = MemberName,
                    GroupMemberNumber = MemberNumber,
                    //CardNumber = CardNumber,
                    Description = Description,
                    PaymentDate = PaymentDate,
                    CreateAt = DateTime.UtcNow,
                    Comment = Comment,
                    Month = Month,
                    CreateBy = cMem.FullName
                };
                db.P_EmployeePayrollPayment.Add(EmpPayrollPayment);
                foreach (var pr in ListPr)
                {
                    var payroll = db.P_EmployeePayroll.Where(x => x.Id == pr).FirstOrDefault();
                    if (payroll != null)
                    {
                        payroll.PaidNumber = PaidNumber;
                    }
                }
                db.SaveChanges();
                return Json(true);
            }
        }
        [HttpGet]
        /// <summary>
        /// view detail transaction
        /// </summary>
        /// <param name="OrderCode">Order Code</param>
        public ActionResult ViewTransaction(string OrderCode)
        {
            WebDataModel db = new WebDataModel();
            var transaction = db.C_CustomerTransaction.Where(t => t.OrdersCode == OrderCode).ToList();
            return PartialView("_PopupTransaction", transaction);
        }
        /// <summary>
        /// update commission then calculated payroll
        /// </summary>
        /// <param name="EmpPayrollId">EmpPayrollId</param>
        [HttpPost]
        public ActionResult updateCmm(long EmpPayrollId)
        {
            WebDataModel db = new WebDataModel();
            var empPayroll = db.P_EmployeePayroll.Where(x => x.Id == EmpPayrollId).FirstOrDefault();
            empPayroll.IsCalculate = !empPayroll.IsCalculate;
            db.SaveChanges();
            return Json(true);
        }
        #endregion
        //[HttpPost]
        //public ActionResult SetAsPaid(string MemberNumber, string Bank, string PaymentMethod, string CardNumber, string Description, DateTime Date)
        //{
        //    WebDataModel db = new WebDataModel();
        //    var PaidNumber = CreatePaidNumber(DateTime.Now.ToString("yyyyMM"));
        //    var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
        //    var EmpPayrollPayment = new P_EmployeePayrollPayment
        //    {
        //        PaidNumber = PaidNumber,
        //        PaymentMethod = PaymentMethod,
        //        PaymentName = Bank,
        //        GroupMemberName = member.FullName,
        //        GroupMemberNumber = member.MemberNumber,
        //        CardNumber = CardNumber,
        //        Description = Description,
        //        CreateAt = DateTime.UtcNow,
        //        CreateBy = cMem.FullName
        //    };
        //    db.P_EmployeePayrollPayment.Add(EmpPayrollPayment);
        //    string Month = Date.ToString("MM/yyyy");
        //    var EmpPayRoll = db.P_EmployeePayroll.Where(x => x.RecipientMemberNumber == MemberNumber && x.PayrollMonth == Month);
        //    foreach (var item in EmpPayRoll)
        //    {
        //        item.PaidNumber = PaidNumber;
        //    }
        //    db.SaveChanges();
        //    return Json(true);
        //}
        #region Paid Payment
        /// <summary>
        /// load data for datatable by payment
        /// </summary>
        /// <param name="dataTablesRequest">param datatable jquery</param>
        /// <param name="DepartMentId">department id</param>
        /// <param name="PaidNumber">Paid Number</param>
        /// <param name="Year">Year</param>
        public ActionResult LoadListPaidByPayment(IDataTablesRequest dataTablesRequest, string DepartMentId, string EmpName, string PaidNumber/*, DateTime? PaymentDate,*/, string Year)
        {
            WebDataModel db = new WebDataModel();
            int totalRecord = 0;
            if (!string.IsNullOrEmpty(DepartMentId))
            {
                string[] ListMemberInDeparment = db.P_Member.Where(x => x.DepartmentId.Contains(DepartMentId)).Select(x => x.MemberNumber).ToArray();
                var query = from pr in db.P_EmployeePayrollPayment
                            where pr.Month.Contains(Year)
                            join methodpayment in db.P_BankSupport on pr.PaymentMethod equals methodpayment.Code
                            let listPayrollMember = db.P_EmployeePayroll.Where(p => p.PaidNumber == pr.PaidNumber && ListMemberInDeparment.Any(n => p.RecipientMemberNumber.Contains(n))).ToList()
                            select new { pr, listPayrollMember, methodpayment };
                query = query.Where(x => x.listPayrollMember.Count() > 0);
                if (!string.IsNullOrEmpty(EmpName))
                {
                    query = from q in query let listPayrollMember = q.listPayrollMember.Where(x => x.RecipientName.Contains(EmpName)).ToList() select new { q.pr, listPayrollMember, q.methodpayment };
                    query = query.Where(x => x.listPayrollMember.Count() > 0);
                }
                if (!string.IsNullOrEmpty(PaidNumber))
                {
                    query = query.Where(x => x.pr.PaidNumber.Contains(PaidNumber));
                }
                totalRecord = query.Count();
                query = query.OrderByDescending(x => x.pr.Id).Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
                var ViewData = query.ToList().Select(x => new
                {
                    Date = x.pr.CreateAt.Value.ToString("MM/dd/yyyy hh:mm:ss"),
                    Method = x.methodpayment.Name,
                    TotalAmount = x.listPayrollMember.Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                    TotalMember = x.listPayrollMember.GroupBy(y => y.RecipientMemberNumber).Count(),
                    PaidNumber = x.pr.PaidNumber,
                    PaidDate = x.pr.PaymentDate.Value.ToString("MM/dd/yyyy"),
                    CreateBy = x.pr.CreateBy,
                    Description = x.pr.Description,
                }); ;
                return Json(new
                {
                    recordsTotal = totalRecord,
                    recordsFiltered = totalRecord,
                    draw = dataTablesRequest.Draw,
                    data = ViewData
                });
            }
            else
            {
                var query = from pr in db.P_EmployeePayrollPayment
                            where pr.Month.Contains(Year)
                            join methodpayment in db.P_BankSupport on pr.PaymentMethod equals methodpayment.Code
                            let listPayrollMember = db.P_EmployeePayroll.Where(p => p.PaidNumber == pr.PaidNumber).ToList()
                            select new { pr, listPayrollMember, methodpayment };
                if (!string.IsNullOrEmpty(EmpName))
                {
                    query = from q in query let listPayrollMember = q.listPayrollMember.Where(x => x.RecipientName.Contains(EmpName)).ToList() select new { q.pr, listPayrollMember, q.methodpayment };
                    query = query.Where(x => x.listPayrollMember.Count() > 0);
                }
                if (!string.IsNullOrEmpty(PaidNumber))
                {
                    query = query.Where(x => x.pr.PaidNumber.Contains(PaidNumber));
                }
                totalRecord = query.Count();
                query = query.OrderByDescending(x => x.pr.Id).Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
                var ViewData = query.ToList().Select(x => new
                {
                    Date = x.pr.CreateAt.Value.ToString("MM/dd/yyyy hh:mm:ss"),
                    Method = x.methodpayment.Name,
                    TotalAmount = x.listPayrollMember.Sum(y => y.Amount).Value.ToString("$#,##0.##"),
                    TotalMember = x.listPayrollMember.GroupBy(y => y.RecipientMemberNumber).Count(),
                    PaidNumber = x.pr.PaidNumber,
                    PaidDate = x.pr.PaymentDate.Value.ToString("MM/dd/yyyy"),
                    CreateBy = x.pr.CreateBy,
                    Description = x.pr.Description,
                }); ;
                return Json(new
                {
                    recordsTotal = totalRecord,
                    recordsFiltered = totalRecord,
                    draw = dataTablesRequest.Draw,
                    data = ViewData
                });
            }
        }
        /// <summary>
        /// load data for datatable by employee
        /// </summary>
        /// <param name="dataTablesRequest">param datatable jquery</param>
        /// <param name="DepartMentId">department id</param>
        /// <param name="PaidNumber">Paid Number</param>
        /// <param name="Year">Year</param>
        public ActionResult LoadListPaidByEmployee(IDataTablesRequest dataTablesRequest, string DepartMentId, string EmpName, string PaidNumber, string Year)
        {
            WebDataModel db = new WebDataModel();
            int totalRecord = 0;
            decimal? totalBaseSalary = 0;
            decimal? totalCommission = 0;
            decimal? totalAll = 0;
            //string MonthPayroll = Date.Value.ToString("MM/yyyy");
            if ((access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true) && string.IsNullOrEmpty(PaidNumber))
            {
                PaidNumber = (from paidPayment in db.P_EmployeePayrollPayment where paidPayment.Month.Contains(Year) orderby paidPayment.PaymentDate descending select new { paidPayment }).FirstOrDefault()?.paidPayment.PaidNumber ?? "";
            }
            var query = from pr in db.P_EmployeePayroll
                        where /*pr.PayrollMonth == MonthPayroll && !string.IsNullOrEmpty(*/pr.PaidNumber == PaidNumber/*)*/ && pr.IsCalculate.Value && pr.PayrollMonth.Contains(Year)
                        join member in db.P_Member on pr.RecipientMemberNumber equals member.MemberNumber
                        join payment in db.P_EmployeePayrollPayment on pr.PaidNumber equals payment.PaidNumber
                        join methodpayment in db.P_BankSupport on payment.PaymentMethod equals methodpayment.Code
                        select new { pr, payment, member, methodpayment };
            if ((access.Any(k => k.Key.Equals("payroll_viewall")) != true || access["payroll_viewall"] != true) && (access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true))
            {
                if ((access.Any(k => k.Key.Equals("payroll_view")) == true && access["payroll_view"] == true))
                {
                    query = query.Where(x => x.pr.RecipientMemberNumber == cMem.MemberNumber);
                }
                else
                {
                    throw new Exception("access is denied");
                }
            }
            else
            {
                totalCommission = (db.P_EmployeePayroll.Where(x => x.Type == "Commission" /*&& x.PayrollMonth == MonthPayroll*/).Sum(x => x.Amount)) ?? 0;
            }
            if (!string.IsNullOrEmpty(DepartMentId))
            {
                if (DepartMentId == "Other")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.member.DepartmentId));
                }
                else
                {
                    query = query.Where(x => x.member.DepartmentId.Contains(DepartMentId));
                    //if (!string.IsNullOrEmpty(Groups))
                    //{
                    //    if (Groups == "Other")
                    //    {
                    //        long DepartMentIdTypeLong = long.Parse(DepartMentId);
                    //        var ListGroupsExist = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentIdTypeLong);
                    //        List<string> ListMemberExistInDep = new List<string>();
                    //        foreach (var dep in ListGroupsExist)
                    //        {
                    //            if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                    //            {
                    //                ListMemberExistInDep.AddRange(dep.GroupMemberNumber.Split(','));
                    //            }
                    //            if (!string.IsNullOrEmpty(dep.LeaderNumber))
                    //            {
                    //                ListMemberExistInDep.Add(dep.LeaderNumber);
                    //            }
                    //            if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                    //            {
                    //                ListMemberExistInDep.Add(dep.SupervisorNumber);
                    //            }
                    //        }
                    //        ListMemberExistInDep = ListMemberExistInDep.GroupBy(x => x).Select(x => x.Key).ToList();
                    //        query = from member in query where !(ListMemberExistInDep.Contains(member.member.MemberNumber)) select member;
                    //    }
                    //    else
                    //    {
                    //        List<string> ListMemberGroups = new List<string>();
                    //        var Group = (from dep in db.P_Department where dep.Id.ToString() == Groups select dep).FirstOrDefault();
                    //        if (!string.IsNullOrEmpty(Group?.GroupMemberNumber))
                    //        {
                    //            ListMemberGroups.AddRange(Group.GroupMemberNumber.Split(','));
                    //        }
                    //        if (!string.IsNullOrEmpty(Group?.LeaderNumber))
                    //        {
                    //            ListMemberGroups.Add(Group.LeaderNumber);
                    //        }
                    //        if (!string.IsNullOrEmpty(Group?.SupervisorNumber))
                    //        {
                    //            ListMemberGroups.Add(Group.SupervisorNumber);
                    //        }
                    //        ListMemberGroups.GroupBy(x => x).Select(x => x.Key);
                    //        query = from member in query where ListMemberGroups.Contains(member.member.MemberNumber) select member;
                    //    }
                    //}
                }
            }
            //if (!string.IsNullOrEmpty(PaidNumber))
            //{
            //    query = query.Where(x => x.pr.PaidNumber.Contains(PaidNumber));
            //}
            if (!string.IsNullOrEmpty(EmpName))
            {
                query = query.Where(x => x.pr.RecipientName.Contains(EmpName));
            }
            //if (!string.IsNullOrEmpty(PaymentMethod))
            //{
            //    query = query.Where(x => x.payment.PaymentMethod.Contains(PaymentMethod));
            //}
            //if (PaymentDate != null)
            //{
            //    query = query.Where(x => DbFunctions.TruncateTime(x.payment.CreateAt) == PaymentDate);
            //}
            var listquery = query.GroupBy(x => x.pr.RecipientMemberNumber).OrderBy(i => i.Key);
            totalRecord = listquery.Count();
            totalBaseSalary = listquery.Sum(x => x.Where(g => g.pr.Type == "BaseSalary").Sum(y => y.pr.Amount));
            totalCommission = listquery.Sum(x => x.Where(g => g.pr.Type == "Commission").Sum(y => y.pr.Amount));
            totalAll = listquery.Sum(x => x.Sum(y => y.pr.Amount));
            var ViewData = listquery.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x => new
            {
                Date = x.FirstOrDefault().payment.CreateAt.Value.ToString("MM/dd/yyyy hh:mm:ss"),
                CreateBy = x.FirstOrDefault().payment.CreateBy,
                Method = x.FirstOrDefault().methodpayment.Name,
                TotalAll = x.Sum(y => y.pr.Amount).Value.ToString("$#,##0.##"),
                TotalBaseSalary = x.Where(y => y.pr.Type == "BaseSalary").Sum(y => y.pr.Amount).Value.ToString("$#,##0.##"),
                TotalCommission = x.Where(y => y.pr.Type == "Commission").Sum(y => y.pr.Amount).Value.ToString("$#,##0.##"),
                EmpName = x.FirstOrDefault().pr.RecipientName,
                PaidNumber = x.FirstOrDefault().pr.PaidNumber,
                PaidDate = x.FirstOrDefault().payment.PaymentDate.Value.ToString("MM/dd/yyyy"),
                EmpId = x.FirstOrDefault().pr.RecipientMemberNumber,
            });
            return Json(new
            {
                totalBaseSalary = totalBaseSalary != null ? totalBaseSalary.Value.ToString("$#,##0.##") : "$0",
                totalCommission = totalCommission != null ? totalCommission.Value.ToString("$#,##0.##") : "$0",
                totalAll = totalCommission != null ? totalAll.Value.ToString("$#,##0.##") : "$0",
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                draw = dataTablesRequest.Draw,
                data = ViewData
            });
        }
        /// <summary>
        /// detail paid payment
        /// </summary>
        /// <param name="DepartmentId">department id</param>
        /// <param name="PaidNumber">Paid Number</param>

        public ActionResult DetailPaidPayment(string PaidNumber, string DepartmentId, string EmpName)
        {
            WebDataModel db = new WebDataModel();
            var PaidPayment = (from paid in db.P_EmployeePayrollPayment where paid.PaidNumber == PaidNumber join bank in db.P_BankSupport on paid.PaymentMethod equals bank.Code select new { paid, bank }).FirstOrDefault();
            var ListMember = PaidPayment.paid.GroupMemberNumber.Split(',');
            if (!string.IsNullOrEmpty(DepartmentId))
            {
                ListMember = (from member in db.P_Member where member.DepartmentId.Contains(DepartmentId) join m in ListMember on member.MemberNumber equals m select new { member }).Select(x => x.member.MemberNumber).ToArray();
            }
            if (!string.IsNullOrEmpty(EmpName))
            {
                ListMember = (from member in db.P_Member where member.FullName.Contains(EmpName) join m in ListMember on member.MemberNumber equals m select new { member }).Select(x => x.member.MemberNumber).ToArray();
            }
            List<EmployeeDetailPayment> ListMemberNumber = new List<EmployeeDetailPayment>();
            foreach (var member in ListMember)
            {
                var Payroll = (from pr in db.P_EmployeePayroll
                               where pr.RecipientMemberNumber == member && pr.PaidNumber == PaidNumber && pr.IsCalculate.Value && pr.PayrollMonth == PaidPayment.paid.Month
                               select pr).ToList();
                var model = new EmployeeDetailPayment();
                model.MemberName = Payroll.FirstOrDefault().RecipientName;
                model.MemberNumber = member;
                model.PaidDate = PaidPayment.paid.CreateAt.Value.ToString("MM/dd/yyyy hh:mm:ss");
                model.TotalSalary = Payroll.Where(x => x.Type == "BaseSalary").FirstOrDefault()?.Amount ?? 0;
                model.TotalCommission = Payroll.Where(x => x.Type == "Commission").Sum(x => x.Amount);
                model.PaidMethod = PaidPayment.bank.Name;
                ListMemberNumber.Add(model);
            }
            var Detail = new DetailPayrollPayment
            {
                PaidDescription = PaidPayment.paid.Description,
                PaidNumber = PaidPayment.paid.PaidNumber,
                ListEmployee = ListMemberNumber
            };
            return PartialView("_DetailPayrollPayment", Detail);
        }
        /// <summary>
        /// popup detail payroll is paid for employee
        /// </summary>
        /// <param name="MemberNumber">member number</param>
        /// <param name="PaidNumber">Paid Number</param>
        [HttpPost]
        public ActionResult PopUpDetailPayroll(string MemberNumber, string PaidNumber)
        {
            WebDataModel db = new WebDataModel();
            var Member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
            var Salary = db.P_EmployeePayroll.Where(x => x.Type == "BaseSalary" && x.RecipientMemberNumber == MemberNumber && x.PaidNumber == PaidNumber && x.IsCalculate.Value).FirstOrDefault();
            var ListCommission = (from pr in db.P_EmployeePayroll
                                  where pr.PaidNumber == PaidNumber && pr.Type == "Commission" && pr.RecipientMemberNumber == MemberNumber && pr.IsCalculate.Value
                                  join tran in db.C_CustomerTransaction on pr.CustomerTransactionId equals tran.Id
                                  join order in db.O_Orders on tran.OrdersCode equals order.OrdersCode
                                  let typeSubcrition = db.Order_Subcription.Where(x => x.OrderCode == order.OrdersCode).Count() > 0 ? "Service" : ""
                                  let typeHardWare = db.Order_Products.Where(x => x.OrderCode == order.OrdersCode).Count() > 0 ? "Hardware" : ""
                                  orderby pr.Id
                                  select new EmployeeComission
                                  {
                                      TypeOfInvoice = typeSubcrition + ((!string.IsNullOrEmpty(typeHardWare) && !string.IsNullOrEmpty(typeSubcrition)) ? " & " : "") + typeHardWare,
                                      OrderCode = order.OrdersCode,
                                      Status = order.Status,
                                      CustomerName = order.CustomerName,
                                      Amount = tran.Amount,
                                      Type = pr.TypeComm,
                                      SalesPersonName = order.SalesName,
                                      SalesPersonNumber = order.SalesMemberNumber,
                                      TransactionId = pr.CustomerTransactionId,
                                      Comisssion = pr.Amount,
                                  }).ToList();
            var ViewModel = new DetailEmployeePayrollCustomizeModel()
            {
                EmployeeId = Member.MemberNumber,
                EmployeeName = Member.FullName,
                BaseSalary = Salary?.Amount ?? 0,
                Address = Member.Address + ", " + Member.City + ", " + Member.State + ", " + Member.ZipCode + ", " + Member.Country,
                Phone = Member.CellPhone,
                Email = Member.Email1,
                IsSalesPerson = Member.DepartmentName.Contains("SALES & MARKETING"),
                Department = Member.DepartmentName,
                ListEmployeeComission = ListCommission,
            };
            return PartialView("_PopupDetailPayroll", ViewModel);
        }
        #endregion
        #endregion
        #region Export Excel
        public async Task<FileStreamResult> ExportExcelPendingPayment(string Name, string Phone, string DepartMentId, string Groups, DateTime? Date)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                int statusMerchant = LeadStatus.Merchant.Code<int>();
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });
                //view all sales lead if permission is view all
                string webRootPath = "/upload/other/";
                string fileName = @"TempData.xlsx";
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(System.IO.Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    //name style
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;
                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);
                    //header style
                    IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Underline = FontUnderlineType.Double;
                    font1.FontHeightInPoints = 13;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                    ISheet excelSheet = workbook.CreateSheet("Pending Payment Report");
                    //set column width
                    excelSheet.SetColumnWidth(1, 15 * 256);
                    excelSheet.SetColumnWidth(2, 15 * 256);
                    excelSheet.SetColumnWidth(3, 20 * 256);
                    excelSheet.SetColumnWidth(4, 10 * 256);
                    excelSheet.SetColumnWidth(5, 20 * 256);
                    excelSheet.SetColumnWidth(6, 15 * 256);
                    excelSheet.SetColumnWidth(7, 20 * 256);
                    excelSheet.SetColumnWidth(8, 20 * 256);
                    //report info
                    IRow row1 = excelSheet.CreateRow(0);
                    row1.CreateCell(0).SetCellValue(webinfo?.CompanyName);
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 1));
                    IRow row2 = excelSheet.CreateRow(1);
                    row2.CreateCell(0).SetCellValue(address.Length > 0 ? address[0] : "---");
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 1));
                    IRow row3 = excelSheet.CreateRow(2);
                    row3.CreateCell(0).SetCellValue(address.Length > 0 ? address[1] + "," + address[2] + address[3] : "---,--- #####");
                    excelSheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 1));
                    IRow row4 = excelSheet.CreateRow(3);
                    row4.CreateCell(0).SetCellValue("www.enrichcous.com");
                    excelSheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 1));
                    ICell cell = row1.CreateCell(5);
                    cell.SetCellValue(new XSSFRichTextString("Payroll Report"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 5, 6));
                    cell.CellStyle = style;
                    row2.CreateCell(5).SetCellValue("Date: " + DateTime.Now.ToString("MM dd,yyyy hh:mm tt"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));
                    excelSheet.CreateFreezePane(0, 9, 0, 9);
                    //header table
                    IRow header = excelSheet.CreateRow(8);
                    string[] head_titles = { "#", "Employee Id", "Employee Name", "Phone", "Base Salary ($)", "Commission ($)", "Total ($)", "Is Calculated", "Paid" };
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]); c.CellStyle = style1;
                    }
                    //search info
                    IRow s_row = excelSheet.CreateRow(6);
                    s_row.CreateCell(1).SetCellValue("Filter");
                    s_row.CreateCell(1).SetCellValue("Name : " + Name);
                    s_row.CreateCell(3).SetCellValue("Phone: " + Phone);
                    string departmentName = "All";
                    string groupsName = "All";
                    if (!string.IsNullOrEmpty(DepartMentId))
                    {
                        if (DepartMentId == "Other")
                        {
                            departmentName = "Other";
                        }
                        else
                        {
                            var typeDepartMentId = long.Parse(DepartMentId);
                            departmentName = db.P_Department.Where(x => x.Id == typeDepartMentId).FirstOrDefault().Name;
                        }
                    }
                    if (!string.IsNullOrEmpty(Groups))
                    {
                        if (Groups == "Other")
                        {
                            groupsName = "Other";
                        }
                        else
                        {
                            var typeGroupsId = long.Parse(Groups);
                            groupsName = db.P_Department.Where(x => x.Id == typeGroupsId).FirstOrDefault().Name;
                        }
                    }
                    s_row.CreateCell(4).SetCellValue("DepartMent : " + departmentName);
                    s_row.CreateCell(6).SetCellValue("Groups: " + groupsName);
                    int row_num = 8;
                    //table data
                    int index = 1;
                    string MonthPayroll = Date.Value.ToString("MM/yyyy");
                    var query = from member in db.P_Member
                                where member.Active == true
                                && member.Delete != true
                                //&& !((from pr in db.P_EmployeePayrollPayment where pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year select pr).Any(a=>a.GroupMemberNumber.Contains(member.MemberNumber)))
                                //let setaspaid = db.P_EmployeePayrollPayment.Where(pr => pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year).Any(a => a.GroupMemberNumber.Contains(member.MemberNumber))
                                let prl = db.P_EmployeePayroll.Where(p => p.RecipientMemberNumber == member.MemberNumber && p.PayrollMonth == MonthPayroll).ToList()
                                select new { member, prl/*, setaspaid*/ };
                    if ((access.Any(k => k.Key.Equals("payroll_viewall")) != true || access["payroll_viewall"] != true) && (access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true))
                    {
                        if ((access.Any(k => k.Key.Equals("payroll_view")) == true && access["payroll_view"] == true))
                        {
                            query = query.Where(x => x.member.MemberNumber.Contains(cMem.MemberNumber));
                        }
                        else
                        {
                            throw new Exception("access is denied");
                        }
                    }
                    if (!string.IsNullOrEmpty(Name))
                    {
                        query = query.Where(x => x.member.FullName.Contains(Name));
                    }
                    if (!string.IsNullOrEmpty(Phone))
                    {
                        query = query.Where(x => x.member.CellPhone.Contains(Phone));
                    }
                    if (!string.IsNullOrEmpty(DepartMentId))
                    {
                        if (DepartMentId == "Other")
                        {
                            query = query.Where(x => string.IsNullOrEmpty(x.member.DepartmentId));
                        }
                        else
                        {
                            query = query.Where(x => x.member.DepartmentId.Contains(DepartMentId));
                            if (!string.IsNullOrEmpty(Groups))
                            {
                                if (Groups == "Other")
                                {
                                    long DepartMentIdTypeLong = long.Parse(DepartMentId);
                                    var ListGroupsExist = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentIdTypeLong);
                                    List<string> ListMemberExistInDep = new List<string>();
                                    foreach (var dep in ListGroupsExist)
                                    {
                                        if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                                        {
                                            ListMemberExistInDep.AddRange(dep.GroupMemberNumber.Split(','));
                                        }
                                        if (!string.IsNullOrEmpty(dep.LeaderNumber))
                                        {
                                            ListMemberExistInDep.Add(dep.LeaderNumber);
                                        }
                                        if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                                        {
                                            ListMemberExistInDep.Add(dep.SupervisorNumber);
                                        }
                                    }
                                    ListMemberExistInDep = ListMemberExistInDep.GroupBy(x => x).Select(x => x.Key).ToList();
                                    query = from member in query where !(ListMemberExistInDep.Contains(member.member.MemberNumber)) select member;
                                }
                                else
                                {
                                    List<string> ListMemberGroups = new List<string>();
                                    var Group = (from dep in db.P_Department where dep.Id.ToString() == Groups select dep).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(Group?.GroupMemberNumber))
                                    {
                                        ListMemberGroups.AddRange(Group.GroupMemberNumber.Split(','));
                                    }
                                    if (!string.IsNullOrEmpty(Group?.LeaderNumber))
                                    {
                                        ListMemberGroups.Add(Group.LeaderNumber);
                                    }
                                    if (!string.IsNullOrEmpty(Group?.SupervisorNumber))
                                    {
                                        ListMemberGroups.Add(Group.SupervisorNumber);
                                    }
                                    ListMemberGroups.GroupBy(x => x).Select(x => x.Key);
                                    query = from member in query where ListMemberGroups.Contains(member.member.MemberNumber) select member;
                                }
                            }
                        }
                    }
                    foreach (var item in query)
                    {
                        row_num++;
                        IRow row_next_1 = excelSheet.CreateRow(row_num);
                        row_next_1.CreateCell(0).SetCellValue(index);
                        row_next_1.CreateCell(1).SetCellValue(item.member.MemberNumber);
                        row_next_1.CreateCell(2).SetCellValue(item.member.FullName);
                        row_next_1.CreateCell(3).SetCellValue(item.member.CellPhone);
                        row_next_1.CreateCell(4).SetCellValue((item.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).FirstOrDefault() != null ? item.prl.Where(y => y.Type == "BaseSalary" && y.IsCalculate.Value).FirstOrDefault().Amount : 0).ToString());
                        row_next_1.CreateCell(5).SetCellValue(item.prl.Where(y => y.Type == "Commission" && y.IsCalculate.Value).Sum(y => y.Amount).ToString());
                        row_next_1.CreateCell(6).SetCellValue(item.prl.Where(y => y.IsCalculate.Value).Sum(y => y.Amount).ToString());
                        row_next_1.CreateCell(7).SetCellValue(item.prl.Count() > 0);
                        row_next_1.CreateCell(8).SetCellValue(item.prl.Where(y => !string.IsNullOrEmpty(y.PaidNumber)).Count() > 0);
                        index++;
                    }
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        excelSheet.AutoSizeColumn(i);
                    }
                    workbook.Write(fs);
                }
                using (var fileStream = new FileStream(Server.MapPath(System.IO.Path.Combine(webRootPath, fileName)), FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;
                string _fileName = "PendingPaymentReport_" + DateTime.UtcNow.ToString("yyyyMMdd") + ".xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<FileStreamResult> ExportExcelPaidPayment(string DepartMentId, string EmpName, string PaidNumber, string Year)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                int statusMerchant = LeadStatus.Merchant.Code<int>();
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });
                //view all sales lead if permission is view all
                string webRootPath = "/upload/other/";
                string fileName = @"TempData.xlsx";
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(System.IO.Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    //name style
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;
                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);
                    //header style
                    IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Underline = FontUnderlineType.Double;
                    font1.FontHeightInPoints = 13;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                    ISheet excelSheet = workbook.CreateSheet("Payroll Paid Report");
                    //set column width
                    excelSheet.SetColumnWidth(1, 15 * 256);
                    excelSheet.SetColumnWidth(2, 15 * 256);
                    excelSheet.SetColumnWidth(3, 20 * 256);
                    excelSheet.SetColumnWidth(4, 10 * 256);
                    excelSheet.SetColumnWidth(5, 20 * 256);
                    excelSheet.SetColumnWidth(6, 15 * 256);
                    excelSheet.SetColumnWidth(7, 20 * 256);
                    excelSheet.SetColumnWidth(8, 20 * 256);
                    //report info
                    IRow row1 = excelSheet.CreateRow(0);
                    row1.CreateCell(0).SetCellValue(webinfo?.CompanyName);
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 1));
                    IRow row2 = excelSheet.CreateRow(1);
                    row2.CreateCell(0).SetCellValue(address.Length > 0 ? address[0] : "---");
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 1));
                    IRow row3 = excelSheet.CreateRow(2);
                    row3.CreateCell(0).SetCellValue(address.Length > 0 ? address[1] + "," + address[2] + address[3] : "---,--- #####");
                    excelSheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 1));
                    IRow row4 = excelSheet.CreateRow(3);
                    row4.CreateCell(0).SetCellValue("www.enrichcous.com");
                    excelSheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 1));
                    ICell cell = row1.CreateCell(5);
                    cell.SetCellValue(new XSSFRichTextString("Payroll Paid Report"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 5, 6));
                    cell.CellStyle = style;
                    row2.CreateCell(5).SetCellValue("Date: " + DateTime.Now.ToString("MM dd,yyyy hh:mm tt"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));
                    excelSheet.CreateFreezePane(0, 9, 0, 9);
                    //header table
                    IRow header = excelSheet.CreateRow(8);
                    string[] head_titles = { "#", "Employee Id", "Employee Name", "Phone", "Base Salary ($)", "Commission ($)", "Total ($)" };
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]); c.CellStyle = style1;
                    }
                    //search info
                    IRow s_row = excelSheet.CreateRow(6);
                    s_row.CreateCell(1).SetCellValue("Filter");
                    s_row.CreateCell(1).SetCellValue("Name : " + EmpName);
                    string departmentName = "";
                    if (!string.IsNullOrEmpty(DepartMentId))
                    {
                        if (DepartMentId == "Other")
                        {
                            departmentName = "Other";
                        }
                        else
                        {
                            var typeDepartMentId = long.Parse(DepartMentId);
                            departmentName = db.P_Department.Where(x => x.Id == typeDepartMentId).FirstOrDefault().Name;
                        }
                    }

                    s_row.CreateCell(4).SetCellValue("DepartMent : " + departmentName);
                    int row_num = 8;
                    //table data
                    int index = 1;
                    if ((access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true) && string.IsNullOrEmpty(PaidNumber))
                    {
                        PaidNumber = (from paidPayment in db.P_EmployeePayrollPayment where paidPayment.Month.Contains(Year) orderby paidPayment.PaymentDate descending select new { paidPayment }).FirstOrDefault()?.paidPayment.PaidNumber ?? "";
                    }
                    var query = from pr in db.P_EmployeePayroll
                                where /*pr.PayrollMonth == MonthPayroll && !string.IsNullOrEmpty(*/pr.PaidNumber == PaidNumber/*)*/ && pr.IsCalculate.Value && pr.PayrollMonth.Contains(Year)
                                join member in db.P_Member on pr.RecipientMemberNumber equals member.MemberNumber
                                join payment in db.P_EmployeePayrollPayment on pr.PaidNumber equals payment.PaidNumber
                                join methodpayment in db.P_BankSupport on payment.PaymentMethod equals methodpayment.Code
                                select new { pr, payment, member, methodpayment };
                    if ((access.Any(k => k.Key.Equals("payroll_viewall")) != true || access["payroll_viewall"] != true) && (access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true))
                    {
                        if ((access.Any(k => k.Key.Equals("payroll_view")) == true && access["payroll_view"] == true))
                        {
                            query = query.Where(x => x.pr.RecipientMemberNumber == cMem.MemberNumber);
                        }
                        else
                        {
                            throw new Exception("access is denied");
                        }
                    }
                    if (!string.IsNullOrEmpty(DepartMentId))
                    {
                        if (DepartMentId == "Other")
                        {
                            query = query.Where(x => string.IsNullOrEmpty(x.member.DepartmentId));
                        }
                        else
                        {
                            query = query.Where(x => x.member.DepartmentId.Contains(DepartMentId));
                            //if (!string.IsNullOrEmpty(Groups))
                            //{
                            //    if (Groups == "Other")
                            //    {
                            //        long DepartMentIdTypeLong = long.Parse(DepartMentId);
                            //        var ListGroupsExist = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentIdTypeLong);
                            //        List<string> ListMemberExistInDep = new List<string>();
                            //        foreach (var dep in ListGroupsExist)
                            //        {
                            //            if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                            //            {
                            //                ListMemberExistInDep.AddRange(dep.GroupMemberNumber.Split(','));
                            //            }
                            //            if (!string.IsNullOrEmpty(dep.LeaderNumber))
                            //            {
                            //                ListMemberExistInDep.Add(dep.LeaderNumber);
                            //            }
                            //            if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                            //            {
                            //                ListMemberExistInDep.Add(dep.SupervisorNumber);
                            //            }
                            //        }
                            //        ListMemberExistInDep = ListMemberExistInDep.GroupBy(x => x).Select(x => x.Key).ToList();
                            //        query = from member in query where !(ListMemberExistInDep.Contains(member.member.MemberNumber)) select member;
                            //    }
                            //    else
                            //    {
                            //        List<string> ListMemberGroups = new List<string>();
                            //        var Group = (from dep in db.P_Department where dep.Id.ToString() == Groups select dep).FirstOrDefault();
                            //        if (!string.IsNullOrEmpty(Group?.GroupMemberNumber))
                            //        {
                            //            ListMemberGroups.AddRange(Group.GroupMemberNumber.Split(','));
                            //        }
                            //        if (!string.IsNullOrEmpty(Group?.LeaderNumber))
                            //        {
                            //            ListMemberGroups.Add(Group.LeaderNumber);
                            //        }
                            //        if (!string.IsNullOrEmpty(Group?.SupervisorNumber))
                            //        {
                            //            ListMemberGroups.Add(Group.SupervisorNumber);
                            //        }
                            //        ListMemberGroups.GroupBy(x => x).Select(x => x.Key);
                            //        query = from member in query where ListMemberGroups.Contains(member.member.MemberNumber) select member;
                            //    }
                            //}
                        }
                    }
                    if (!string.IsNullOrEmpty(EmpName))
                    {
                        query = query.Where(x => x.pr.RecipientName.Contains(EmpName));
                    }

                    var listquery = query.GroupBy(x => x.pr.RecipientMemberNumber).OrderBy(i => i.Key);
                    foreach (var item in listquery)
                    {
                        row_num++;
                        IRow row_next_1 = excelSheet.CreateRow(row_num);
                        row_next_1.CreateCell(0).SetCellValue(index);
                        row_next_1.CreateCell(1).SetCellValue(item.FirstOrDefault().member.MemberNumber);
                        row_next_1.CreateCell(2).SetCellValue(item.FirstOrDefault().member.FullName);
                        row_next_1.CreateCell(3).SetCellValue(item.FirstOrDefault().member.CellPhone);
                        row_next_1.CreateCell(4).SetCellValue(item.Where(y => y.pr.Type == "BaseSalary").Sum(y => y.pr.Amount).Value.ToString("$#,##0.##"));
                        row_next_1.CreateCell(5).SetCellValue(item.Where(y => y.pr.Type == "Commission").Sum(y => y.pr.Amount).Value.ToString("$#,##0.##"));
                        row_next_1.CreateCell(6).SetCellValue(item.Sum(y => y.pr.Amount).Value.ToString("$#,##0.##"));
                        index++;
                    }
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        excelSheet.AutoSizeColumn(i);
                    }
                    workbook.Write(fs);
                }
                using (var fileStream = new FileStream(Server.MapPath(System.IO.Path.Combine(webRootPath, fileName)), FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;
                string _fileName = "PaidPaymentReport_" + DateTime.UtcNow.ToString("yyyyMMdd") + ".xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Payslip
        [HttpPost]
        public ActionResult LoadListPayslip(IDataTablesRequest dataTablesRequest, string SearchText, string Status, string DepartMentId, string Groups, DateTime? Date)
        {
            WebDataModel db = new WebDataModel();
            int totalRecord = 0;

            string MonthPayroll = Date.Value.ToString("MM/yyyy");
            var query = from member in db.P_Member
                        where member.Active == true
                        && member.Delete != true
                        join payslip in db.P_PayrollFiles on member.MemberNumber equals payslip.MemberNumber
                        where payslip.PayrollDate == Date && payslip.IsActive == true && payslip.IsApproved == true
                        //  let prl = db.P_EmployeePayroll.Where(p => p.RecipientMemberNumber == member.MemberNumber && p.PayrollMonth == MonthPayroll).ToList()
                        select new { member, payslip/*, setaspaid*/ };
            if ((access.Any(k => k.Key.Equals("payroll_viewall")) != true || access["payroll_viewall"] != true) && (access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true))
            {
                if ((access.Any(k => k.Key.Equals("payroll_view")) == true && access["payroll_view"] == true))
                {
                    query = query.Where(x => x.member.MemberNumber.Contains(cMem.MemberNumber));
                }
                else
                {
                    throw new Exception("access is denied");
                }
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.member.FullName.Contains(SearchText) || x.member.CellPhone.Contains(SearchText) || x.member.Email1.Contains(SearchText) || x.member.MemberNumber.Contains(SearchText));
            }

            if (!string.IsNullOrEmpty(DepartMentId))
            {
                if (DepartMentId == "Other")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.member.DepartmentId));
                }
                else
                {
                    query = query.Where(x => x.member.DepartmentId.Contains(DepartMentId));
                    if (!string.IsNullOrEmpty(Groups))
                    {
                        if (Groups == "Other")
                        {
                            long DepartMentIdTypeLong = long.Parse(DepartMentId);
                            var ListGroupsExist = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentIdTypeLong);
                            List<string> ListMemberExistInDep = new List<string>();
                            foreach (var dep in ListGroupsExist)
                            {
                                if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                                {
                                    ListMemberExistInDep.AddRange(dep.GroupMemberNumber.Split(','));
                                }
                                if (!string.IsNullOrEmpty(dep.LeaderNumber))
                                {
                                    ListMemberExistInDep.Add(dep.LeaderNumber);
                                }
                                if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                                {
                                    ListMemberExistInDep.Add(dep.SupervisorNumber);
                                }
                            }
                            ListMemberExistInDep = ListMemberExistInDep.GroupBy(x => x).Select(x => x.Key).ToList();
                            query = from member in query where !(ListMemberExistInDep.Contains(member.member.MemberNumber)) select member;
                        }
                        else
                        {
                            List<string> ListMemberGroups = new List<string>();
                            var Group = (from dep in db.P_Department where dep.Id.ToString() == Groups select dep).FirstOrDefault();
                            if (!string.IsNullOrEmpty(Group?.GroupMemberNumber))
                            {
                                ListMemberGroups.AddRange(Group.GroupMemberNumber.Split(','));
                            }
                            if (!string.IsNullOrEmpty(Group?.LeaderNumber))
                            {
                                ListMemberGroups.Add(Group.LeaderNumber);
                            }
                            if (!string.IsNullOrEmpty(Group?.SupervisorNumber))
                            {
                                ListMemberGroups.Add(Group.SupervisorNumber);
                            }
                            ListMemberGroups.GroupBy(x => x).Select(x => x.Key);
                            query = from member in query where ListMemberGroups.Contains(member.member.MemberNumber) select member;
                        }
                    }
                }
            }
            totalRecord = query.Count();

            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            if (colSearch == null)
            {
                query = query.OrderByDescending(s => s.member.CreateAt);
            }
            else
            {
                switch (colSearch.Name)
                {
                    case "MemberNumber":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.member.MemberNumber);
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.member.MemberNumber);
                        }
                        break;
                    case "MemberName":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            query = query.OrderBy(s => s.member.FullName);
                        }
                        else
                        {
                            query = query.OrderByDescending(s => s.member.FullName);
                        }
                        break;
                    default:
                        query = query.OrderByDescending(s => s.member.CreateAt);
                        break;
                }
            }
            query = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var ViewData = query.ToList().Select(x => new
            {
                MemberNumber = x.member.MemberNumber,
                EmployeeId = x.member.EmployeeId,
                EmployeeName = x.member.FullName,
                Email = x.member.Email1,
                Address = x.member.Address + ", " + x.member.City + ", " + x.member.State + ", " + x.member.ZipCode + ", " + x.member.Country,
                Phone = x.member.CellPhone,
                Gender = x.member.Gender,
                JoinDate = x.member.JoinDate != null ? x.member.JoinDate.Value.ToString("dd/MM/yyyy") : "",
                Picture = x.member.Picture,
                DepartmentName = x.member.DepartmentName,
                FilePath = x.payslip.FilePath,
                PayslipId = x.payslip.Id,
                IsSendEmail = x.payslip.IsSendMail
            });
            return Json(new
            {
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                draw = dataTablesRequest.Draw,
                data = ViewData
            });
        }

        [HttpPost]
        public ActionResult GetPopupPayslip(DateTime? Date)
        {
            ViewBag.Date = Date;
            return PartialView("_PayslipUpload");
        }
        [HttpPost]
        public ActionResult UploadPayslip(DateTime? Date)
        {
            int filesTotal = Request.Files.Count;
            int start = 0;
            WebDataModel db = new WebDataModel();
            var PayslipFiles = new List<P_PayrollFiles>();
            var strPath = "/Upload/PayrollTemp/";
            DirectoryInfo d = new DirectoryInfo(Server.MapPath(strPath));
            if (!d.Exists)
            {
                d.Create();
            }
            for (int i = start; i < filesTotal; i++)
            {

                HttpPostedFileBase file = HttpContext.Request.Files[i];

                if (file != null && file.FileName != "")
                {
                    if (file.ContentLength > 104857600)
                    {
                        break;
                    }
                    string fileName = AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
                    fileName = DateTime.Now.ToString("yyMMddhhmmssfff") + "_" + System.IO.Path.GetFileName(fileName);
                    string fullPath = System.IO.Path.Combine(Server.MapPath(strPath), fileName);
                    file.SaveAs(fullPath);
                    using (var reader = new PdfReader(fullPath))
                    {
                        var parser = new PdfReaderContentParser(reader);
                        var strategy = parser.ProcessContent(1, new LocationTextExtractionStrategyWithPosition());
                        var res = strategy.GetLocations();

                        reader.Close();
                        var searchResult = res.Where(p => p.Text.Contains("ENR")).OrderBy(p => p.Y).Reverse().ToList();
                        var employeeId = searchResult.FirstOrDefault(x => x.Text.Trim().Length == 9).Text.Trim();
                        var member = db.P_Member.Where(x => x.EmployeeId == employeeId).FirstOrDefault();
                        if (member != null)
                        {
                            var payslip = new P_PayrollFiles()
                            {
                                CreateAt = DateTime.UtcNow,
                                CreateBy = cMem.FullName,
                                IsSendMail = false,
                                FilePath = System.IO.Path.Combine(strPath, fileName),
                                PayrollDate = Date.Value,
                                IsApproved = false,
                                FileName = fileName,
                                IsActive = true,
                                MemberNumber = member.MemberNumber,
                                EmployeeId = employeeId,
                                EmployeeName = member.FullName,

                            };
                            PayslipFiles.Add(payslip);
                        }
                    }
                }
            }
            db.P_PayrollFiles.AddRange(PayslipFiles);
            db.SaveChanges();
            var viewModel = new List<PayslipUpLoadModel>();
            foreach (var payslip in PayslipFiles)
            {
                viewModel.Add(new PayslipUpLoadModel
                {
                    Id = payslip.Id,
                    CreateAt = payslip.CreateAt,
                    EmployeeId = payslip.EmployeeId,
                    EmployeeName = payslip.EmployeeName,
                    FilePath = payslip.FilePath,
                    CreateBy = payslip.CreateBy,
                    PayrollDate = payslip.PayrollDate,
                    FileName = payslip.FileName,
                    IsExist = db.P_PayrollFiles.Any(x => x.IsActive == true && x.IsApproved == true)
                });
            }
            return Json(viewModel);
        }

        public ActionResult SendPayslipEmailAllEmployee(DateTime? Date)
        {
            WebDataModel db = new WebDataModel();
            var payslip = db.P_PayrollFiles.Where(x => x.PayrollDate == Date && x.IsSendMail == false && x.IsApproved == true).ToList();

            return Json(true);
        }

        public ActionResult AprrovePayslipUpload(List<int> UploadIds)
        {
            WebDataModel db = new WebDataModel();
            var payslips = db.P_PayrollFiles.Where(x => UploadIds.Any(y => y == x.Id)).ToList();
            payslips.ForEach(x => x.IsApproved = true);
            db.SaveChanges();
            return Json(new { status = true, message = "approve success" });
        }
        public async Task<ActionResult> SendMailByPayslipId(int? Id)
        {
            WebDataModel db = new WebDataModel();
            var payslip = db.P_PayrollFiles.Find(Id);
            if (payslip == null)
                return Json(new { status = false, message = "Payslip not found" });

            var member = db.P_Member.Where(x => x.MemberNumber == payslip.MemberNumber).FirstOrDefault();
            await SendPayslip(member, payslip);
            payslip.IsSendMail = true;
            db.SaveChanges();
            return Json(new { status = true, message = "send email success" });
        }
        public async Task<ActionResult> SendEmailAllPayslip(DateTime? Date)
        {
            WebDataModel db = new WebDataModel();
            var payslips = db.P_PayrollFiles.Where(x => x.IsApproved == true && x.PayrollDate == Date && x.IsSendMail != true).ToList();
            foreach (var payslip in payslips)
            {
                var member = db.P_Member.Where(x => x.MemberNumber == payslip.MemberNumber).FirstOrDefault();
                await SendPayslip(member, payslip);
                payslip.IsSendMail = true;
            }
            db.SaveChanges();
            return Json(new { status = true, message = "send email success" });
        }
        public async Task SendPayslip(P_Member member, P_PayrollFiles payslip)
        {
            string subject = $"PHIEU LUONG THANG " + payslip.PayrollDate.Month;
            string content = $"<b>Thank you and best regards,</b>" +
                             $"<p>Lanh Dang (Ms.)" +
                             $"HP: +84 90 904 7892";
            var emailData = new { content = content, subject = subject };
            var msg = await _mailingService.SendEmail_HR(member.PersonalEmail, member.FullName, subject, "", emailData, System.IO.Path.Combine(payslip.FilePath));
        }
        #endregion
        #region Utilities
        public ActionResult RepairBank(string Method)
        {
            WebDataModel db = new WebDataModel();
            var Bank = db.P_BankSupport.Where(x => x.Status == 1).Select(x => new SelectListItem
            {
                Value = x.Code,
                Text = x.Name,
            }).ToList();
            return Json(Bank);
        }
        [HttpPost]
        public ActionResult CreateOrUpdateBankSupport(P_BankSupport model)
        {
            WebDataModel db = new WebDataModel();
            if (!string.IsNullOrEmpty(model.Code))
            {
                var entity = db.P_BankSupport.Where(x => x.Code == model.Code).FirstOrDefault();
                entity.Comment = model.Comment;
                entity.Name = model.Name;
                entity.Account = model.Account;
                entity.Method = model.Method;
                entity.UpdateBy = cMem.FullName;
                entity.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();
                return Json(new { status = true, command = "update", selectedId = model.Code });
            }
            else
            {
                int lastdigit = 1;
                var lastCode = db.P_BankSupport.OrderByDescending(x => x.Code).FirstOrDefault();
                if (lastCode != null)
                {
                    lastdigit = int.Parse(lastCode.Code) + 1;
                }
                model.Code = lastdigit.ToString("D4");
                model.Status = 1;
                model.CreatedAt = DateTime.UtcNow;
                model.CreatedBy = cMem.FullName;
                db.P_BankSupport.Add(model);
                db.SaveChanges();
                return Json(new { status = true, command = "create", selectedId = model.Code });
            }
        }
        public ActionResult ShowPopUpSetAsPaid(string Type, string EmpId)
        {
            WebDataModel db = new WebDataModel();
            ViewBag.Type = Type;
            ViewBag.EmpId = EmpId;
            var ListMethod = db.P_BankSupport.Where(x => x.Status == 1).ToList();
            ViewBag.ListMethod = ListMethod;
            return PartialView("_PopUpSetAsPaid");
        }
        public ActionResult GetDetailBank(string code)
        {
            WebDataModel db = new WebDataModel();
            var bank = db.P_BankSupport.Where(x => x.Code == code).FirstOrDefault();
            if (bank != null)
            {
                return Json(new { status = true, data = bank });
            }
            else
            {
                return Json(new { status = false });
            }
        }
        public ActionResult LoadBankSupport(string Method)
        {
            WebDataModel db = new WebDataModel();
            var Bank = db.P_BankSupport.Where(x => x.Status == 1).ToList();
            return PartialView("_LoadBankSupport", Bank);
        }
        public ActionResult LoadDropDownListPaidNumber(string Year)
        {
            WebDataModel db = new WebDataModel();
            var employeePayrollPayment = (from paidPayment in db.P_EmployeePayrollPayment where paidPayment.Month.Contains(Year) join bank in db.P_BankSupport on paidPayment.PaymentMethod equals bank.Code orderby paidPayment.PaymentDate descending select new { paidPayment, bank }).ToList().Select(x => new SelectListItem
            {
                Value = x.paidPayment.PaidNumber,
                Text = x.bank.Name + " " + x.paidPayment.PaymentDate.Value.ToString("MMM d, yyyy")
            }).ToList();
            if ((access.Any(k => k.Key.Equals("payroll_accounting")) != true || access["payroll_accounting"] != true) && employeePayrollPayment.Count() == 0)
            {
                employeePayrollPayment.Add(new SelectListItem
                {
                    Value = "",
                    Text = "N/A"
                });
            }
            return Json(employeePayrollPayment);
        }
        public string CreatePaidNumber(string YYMM)
        {
            WebDataModel db = new WebDataModel();
            int lastDigit = 1;
            var LastPaidNumber = db.P_EmployeePayrollPayment.Where(x => x.PaidNumber.Contains(YYMM)).OrderByDescending(x => x.Id).FirstOrDefault();
            if (LastPaidNumber != null)
            {
                string substr = LastPaidNumber.PaidNumber.Substring(LastPaidNumber.PaidNumber.Length - 4);
                lastDigit = Int32.Parse(substr) + 1;
            }
            var PaidNumber = YYMM + lastDigit.ToString("D4");
            return PaidNumber;
        }
        public string CountNumberMemberInDepartment(long? Department)
        {
            WebDataModel db = new WebDataModel();
            int? total = 0;
            total = db.P_Department.Where(x => x.ParentDepartmentId == Department).ToList().Sum(x => x.GroupMemberNumber.Split(',').Length);
            return total.ToString();
        }
        public ActionResult LoadDepartment(DateTime? Date)
        {
            WebDataModel db = new WebDataModel();
            List<SelectListItem> AllDep = new List<SelectListItem>();
            var query = from member in db.P_Member
                        where member.Active == true && member.Delete != true
                        //&& !(from pr in db.P_EmployeePayrollPayment where pr.CreateAt.Value.Month == Date.Value.Month && pr.CreateAt.Value.Year == Date.Value.Year select pr).Any(a => a.GroupMemberNumber.Contains(member.MemberNumber))
                        select member;
            AllDep.Add(new SelectListItem
            {
                Text = "ALL" + " (" + query.Count() + ")"
            });
            AllDep.AddRange((from dep in db.P_Department
                             where dep.ParentDepartmentId == null
                             let members = query.Where(x => x.DepartmentId.Contains(dep.Id.ToString())).ToList()
                             select new { dep, members }).ToList().Select(x => new SelectListItem
                             {
                                 Value = x.dep.Id.ToString(),
                                 Text = x.dep.Name + " (" + x.members.Count().ToString() + ")"
                             }).ToList());
            AllDep.Add(new SelectListItem
            {
                Value = "Other",
                Text = "OTHER" + " (" + query.Where(x => string.IsNullOrEmpty(x.DepartmentId)).Count() + ")"
            });
            return Json(AllDep);
        }
        public ActionResult LoadGroups(long? DepartMentId)
        {
            WebDataModel db = new WebDataModel();
            var Groups = new List<SelectListItem>();
            string DepartMentIdFormatString = DepartMentId.ToString();
            var AllMemberOfDep = db.P_Member.Where(x => x.DepartmentId.Contains(DepartMentIdFormatString) && x.Active != false && x.Delete != true);
            Groups.Add(new SelectListItem
            {
                Text = "ALL" + " (" + AllMemberOfDep.Count() + ")"
            });
            var ChildrenGroups = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentId);
            Groups.AddRange(ChildrenGroups.ToList().Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name + " (" + (x.GroupMemberNumber.Split(',').Count() + (!string.IsNullOrEmpty(x.LeaderNumber) ? 1 : 0) + (!string.IsNullOrEmpty(x.SupervisorNumber) ? 1 : 0)) + ")"
            }).ToList());
            List<string> MemberExistInGroups = new List<string>();
            var AllGr = db.P_Department.Where(x => x.ParentDepartmentId == DepartMentId).ToList();
            foreach (var gr in AllGr)
            {
                if (!string.IsNullOrEmpty(gr.GroupMemberNumber))
                {
                    MemberExistInGroups.AddRange(gr.GroupMemberNumber.Split(','));
                }
                if (!string.IsNullOrEmpty(gr.LeaderNumber))
                {
                    MemberExistInGroups.Add(gr.LeaderNumber);
                }
                if (!string.IsNullOrEmpty(gr.SupervisorNumber))
                {
                    MemberExistInGroups.Add(gr.SupervisorNumber);
                }
            }
            MemberExistInGroups = MemberExistInGroups.GroupBy(x => x).Select(x => x.Key).ToList();
            var TotalMemberNotExistInGroups = AllMemberOfDep.Count() - MemberExistInGroups.Count;
            Groups.Add(new SelectListItem
            {
                Value = "Other",
                Text = "OTHER" + " (" + TotalMemberNotExistInGroups + ")"
            });
            return Json(Groups);
        }
        #endregion
    }
}