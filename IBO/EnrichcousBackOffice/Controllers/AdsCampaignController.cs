using DataTables.AspNet.Core;
using Enrich.DataTransfer;
using Enrich.IServices.Utils.SMS;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class AdsCampaignController : Controller
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        WebDataModel db = new WebDataModel();
        private readonly ISMSService _sMSService;

        public AdsCampaignController(ISMSService sMSService)
        {
            _sMSService = sMSService;
        }

        // GET: AdsCampaign
        public ActionResult Index(string FDate, string TDate, string SText, string SCampaign, string SAdstype, string SStatus, string Page = "")
        {
            if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                return RedirectToAction("Forbidden", "home");

            switch (Page.ToLower())
            {
                case "ads":
                    UserContent.TabHistory = "Ads Settings|/adscampaign?page=ads";
                    TempData["Page"] = "ads";
                    TempData.Keep("Page");
                    break;
                default:
                    UserContent.TabHistory = "Campaigns|/adscampaign?page=campaign";
                    TempData["Page"] = "campaign";
                    TempData.Keep("Page");
                    break;
            }
            TempData["FDate"] = FDate ?? DateTime.UtcNow.UtcToIMSDateTime().AddYears(-1).ToString("MM/dd/yyyy");
            TempData["TDate"] = TDate ?? DateTime.UtcNow.UtcToIMSDateTime().ToString("MM/dd/yyyy");
            TempData["SCampaign"] = SCampaign;
            TempData["SText"] = SText;
            TempData["SAdstype"] = SAdstype;
            //TempData["SResource"] = SResource;
            //TempData["SLicense"] = SLicense;
            //TempData["SState"] = SState;
            TempData["SStatus"] = SStatus ?? "all";
            return View();
        }
        public ActionResult LoadCampaign(IDataTablesRequest dataTablesRequest, string FDate, string TDate, string SText)
        {
            var statusRemove = CampaignStatus.Removed.Code<int>();
            var dataView = db.M_Campaign.AsEnumerable();
            //Filter
            dataView = dataView.Where(c => !statusRemove.Equals(c.Status));
            if (!string.IsNullOrEmpty(SText))
                dataView = dataView.Where(c => CommonFunc.SearchName(c.Name, SText));
            if (!string.IsNullOrEmpty(FDate))
            {
                var FromDate = DateTime.Parse(FDate).IMSToUTCDateTime();
                dataView = dataView.Where(c => c.CreateAt > FromDate);
            }
            if (!string.IsNullOrEmpty(TDate))
            {
                var ToDate = DateTime.Parse(TDate).AddDays(1);
                dataView = dataView.Where(c => c.CreateAt < ToDate);
            }
            //dataView = dataView.Where(c => c.CreateAt > DateTime.Parse(FDate) && c.CreateAt < DateTime.Parse(TDate).AddDays(1);

            //Sort datatable
            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "CreateAt":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.CreateAt);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.CreateAt);
                    }
                    break;
                case "Name":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.Name);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.Name);
                    }
                    break;
                case "NumberOfPeopleReached":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.NumberOfPeopleReached);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.NumberOfPeopleReached);
                    }
                    break;
                case "Note":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.Note);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.Note);
                    }
                    break;
                default:
                    dataView = dataView.OrderByDescending(s => s.CreateAt);
                    break;
            }
            var totalRecords = dataView.Count();
            var data = dataView.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).Select(x => new
            {
                x.Id,
                x.Status,
                x.CreateBy,
                x.Name,
                x.NumberOfPeopleReached,
                x.Note,
                x.TotalSuccess,
                x.TotalFailed,
                CreateAt = x.CreateAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy")
            });
            return Json(new
            {
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                draw = dataTablesRequest.Draw,
                data = data
            });
        }
        public JsonResult SaveCampaign()
        {
            try
            {
                if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                    throw new Exception("You don't have permission.");

                string Id = Request["CamId"] ?? string.Empty;
                string Name = Request["Name"] ?? string.Empty;
                string Note = Request["Note"] ?? string.Empty;

                var oldCamp = db.M_Campaign.Find(Id);
                if (oldCamp != null) //Edit
                {
                    oldCamp.Name = Name;
                    oldCamp.Note = Note;
                    oldCamp.LastUpdateAt = DateTime.UtcNow;
                    oldCamp.LastUpdateBy = cMem.FullName;
                    db.Entry(oldCamp).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, "Update Campaign success." });
                }
                else
                { //Create
                    var newCamp = new M_Campaign()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = Name,
                        Note = Note,
                        CreateAt = DateTime.UtcNow,
                        CreateBy = cMem.FullName,
                        TotalFailed = 0,
                        TotalSuccess = 0,
                        NumberOfPeopleReached = 0,
                        Status = CampaignStatus.Open.Code<int>(),
                    };

                    db.M_Campaign.Add(newCamp);
                    db.SaveChanges();
                    return Json(new object[] { true, "Create new Campaign success.", newCamp.Id, newCamp.Name });
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Create new Campaign fail. " + ex.Message });
            }
        }
        public JsonResult RemoveCampaign(string key)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                    throw new Exception("You don't have permission.");

                var camp = db.M_Campaign.Find(key);
                if (camp == null)
                    throw new Exception("Campaign not found.");
                camp.Status = CampaignStatus.Removed.Code<int>();
                camp.LastUpdateAt = DateTime.UtcNow;
                camp.LastUpdateBy = cMem.FullName;

                db.Entry(camp).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new object[] { true, "Remove " + camp.Name + " success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Remove fail. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult RemoveAds(string key)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                    throw new Exception("You don't have permission.");

                var ads = db.M_Ads.Find(key);
                if (ads == null)
                    throw new Exception("Ads not found.");
                ads.Status = AdsStatus.Removed.Text();
                ads.LastUpdateAt = DateTime.UtcNow;
                ads.LastUpdateBy = cMem.FullName;
                db.Entry(ads).State = System.Data.Entity.EntityState.Modified;

                var Camp = db.M_Campaign.Find(ads.CampaignId);
                if (Camp == null)
                    throw new Exception("Campaign not found.");

                var removedText = AdsStatus.Removed.Text();
                var total = db.M_Ads.Where(c => c.CampaignId.Equals(Camp.Id) && !c.Status.Equals(removedText)).GroupBy(c => c.CampaignId).Select(c => new
                {
                    NumberOfPeopleReached = c.Sum(s => s.NumberOfPeopleReached),
                    TotalSuccess = c.Sum(s => s.TotalSuccess),
                    TotalFailed = c.Sum(s => s.TotalFailed)
                }).FirstOrDefault();
                Camp.NumberOfPeopleReached = total.NumberOfPeopleReached;
                Camp.TotalSuccess = total.TotalSuccess;
                Camp.TotalFailed = total.TotalFailed;
                db.Entry(Camp).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

                return Json(new object[] { true, "Remove " + ads.Name + " success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Remove fail. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LoadAds(IDataTablesRequest dataTablesRequest, string FDate, string TDate, string SText, string SCampaign, string SAdstype, string SStatus)
        {
            var statusRemove = CampaignStatus.Removed.Code<int>();
            var dataView = db.M_Ads.Where(c => c.Status != AdsStatus.Removed.ToString()).Join(db.M_Campaign.Where(m => m.Status != -2), a => a.CampaignId, c => c.Id, (a, c) => new ViewAds
            {
                Id = a.Id,
                AdsType = a.AdsType,
                Resource = a.Resource,
                LicenseType = a.LicenseType,
                State = a.State,
                Zipcode = a.Zipcode,
                NumberOfPeopleReached = a.NumberOfPeopleReached,
                Additional = a.Additional,
                Message = a.Message,
                Status = a.Status,
                TotalSuccess = a.TotalSuccess,
                TotalFailed = a.TotalFailed,
                CreateAt = a.CreateAt,
                CampaignId = a.CampaignId,
                Name = a.Name,
                CompaignName = c.Name,
                Attachment = a.Attachment,
                FailedReason = a.FailedReason
            }).AsEnumerable();
            //Filter
            if (!string.IsNullOrEmpty(FDate))
            {
                var FromDate = DateTime.Parse(FDate).IMSToUTCDateTime();
                dataView = dataView.Where(c => c.CreateAt > FromDate);
            }
            if (!string.IsNullOrEmpty(TDate))
            {
                var ToDate = DateTime.Parse(TDate).AddDays(1);
                dataView = dataView.Where(c => c.CreateAt < ToDate);
            }
            if (!string.IsNullOrEmpty(SText))
            {
                dataView = dataView.Where(c => CommonFunc.SearchName(c.Name, SText) ||
                                            CommonFunc.SearchName(c.CompaignName, SText) ||
                                            CommonFunc.SearchName(c.Message, SText) ||
                                            CommonFunc.SearchName(c.Name, SText));
            }
            if (!string.IsNullOrEmpty(SCampaign))
            {
                dataView = dataView.Where(c => SCampaign == "all" || SCampaign == c.CampaignId);
            }
            if (!string.IsNullOrEmpty(SAdstype))
            {
                dataView = dataView.Where(c => SAdstype == "all" || SAdstype == c.AdsType);
            }
            if (!string.IsNullOrEmpty(SStatus))
            {
                dataView = dataView.Where(c => SStatus == "all" || SStatus == c.Status);
            }
            //if (!string.IsNullOrEmpty(SResource))
            //{
            //    dataView = dataView.Where(c => SResource.Split(',').Intersect(c.Resource.Split(',')).Count() > 0);
            //}
            //if (!string.IsNullOrEmpty(SLicense))
            //{
            //    dataView = dataView.Where(c => SLicense.Split(',').Intersect(c.LicenseType.Split(',')).Count() > 0);
            //}
            //if (!string.IsNullOrEmpty(SState))
            //{
            //    dataView = dataView.Where(c => SState.Split(',').Intersect(c.State.Split(',')).Count() > 0);
            //}
            //Sort
            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "CreateAt":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.CreateAt);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.CreateAt);
                    }
                    break;
                case "Name":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.Name);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.Name);
                    }
                    break;
                case "CompaignName":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.CompaignName);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.CompaignName);
                    }
                    break;
                case "AdsType":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.AdsType);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.AdsType);
                    }
                    break;
                case "Status":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.Status);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.Status);
                    }
                    break;
                case "NumberOfPeopleReached":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.NumberOfPeopleReached);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.NumberOfPeopleReached);
                    }
                    break;
                case "Message":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.Message);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.Message);
                    }
                    break;
                default:
                    dataView = dataView.OrderByDescending(s => s.CreateAt);
                    break;
            }

            var totalRecords = dataView.Count();
           var data = dataView.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).Select(x => new
            {
                x.Id,
                x.State,
                x.Name,
                CreateAt = x.CreateAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy"),
                x.CreateBy,
                x.Zipcode,
                x.TotalSuccess,
                x.TotalFailed,
                x.NumberOfPeopleReached,
                x.Message,
                x.Resource,
                x.FailedReason,
                x.AdsType,
                x.CompaignName,
                x.LicenseType,
                x.Additional,
                x.CampaignId

           }).ToList();
            return Json(new
            {
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                draw = dataTablesRequest.Draw,
                data = data
            });
        }
        public async Task<JsonResult> SaveAds()
        {
            try
            {
                if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                    throw new Exception("You don't have permission.");

                string AdsId = Request["AdsId"] ?? string.Empty;
                string CampId = Request["CompaignId"] ?? string.Empty;
                string Name = Request["Name"] ?? string.Empty;
                string AdsType = Request["AdsType"] ?? string.Empty;
                string Resource = Request["Resource"] ?? string.Empty;
                string LicenseType = Request["LicenseType"] ?? string.Empty;
                string State = Request["State"] ?? string.Empty;
                string Zipcode = Request["Zipcode"] ?? string.Empty;
                string Additional = Request["Additional"] ?? string.Empty;
                string Message = Request["Message"] ?? string.Empty;
                string NumberOfPeopleReached = string.IsNullOrEmpty(Request["NumberOfPeopleReached"]) ? "0" : Request["NumberOfPeopleReached"];
                string Attachment = Request["Attachment"] ?? string.Empty;

                var Camp = db.M_Campaign.Find(CampId);
                if (Camp == null)
                    throw new Exception("Campaign not found.");

                var oldAds = db.M_Ads.Find(AdsId);
                if (oldAds == null) //Create new
                {
                    var newAds = new M_Ads
                    {
                        Id = Guid.NewGuid().ToString(),
                        CampaignId = Camp.Id,
                        Name = Name,
                        AdsType = AdsType,
                        Resource = Resource,
                        LicenseType = LicenseType,
                        State = State,
                        Zipcode = Zipcode,
                        NumberOfPeopleReached = int.Parse(NumberOfPeopleReached),
                        Additional = Additional,
                        Message = Message,
                        Status = AdsStatus.Draft.Text(),
                        StatusDate = DateTime.UtcNow,
                        TotalSuccess = 0,
                        TotalFailed = 0,
                        TemplateID = "",
                        CreateAt = DateTime.UtcNow,
                        CreateBy = cMem.FullName,
                        Attachment = Attachment
                    };
                    db.M_Ads.Add(newAds);
                    await db.SaveChangesAsync();

                    var removedText = AdsStatus.Removed.Text();
                    var total = db.M_Ads.Where(c => c.CampaignId.Equals(Camp.Id) && !c.Status.Equals(removedText)).GroupBy(c => c.CampaignId).Select(c => new
                    {
                        NumberOfPeopleReached = c.Sum(s => s.NumberOfPeopleReached),
                        TotalSuccess = c.Sum(s => s.TotalSuccess),
                        TotalFailed = c.Sum(s => s.TotalFailed)
                    }).FirstOrDefault();
                    Camp.NumberOfPeopleReached = total.NumberOfPeopleReached;
                    Camp.TotalSuccess = total.TotalSuccess;
                    Camp.TotalFailed = total.TotalFailed;
                    db.Entry(Camp).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();

                    return Json(new object[] { true, "Create new Ads success.", newAds });
                }
                else //Edit
                {
                    oldAds.CampaignId = Camp.Id;
                    oldAds.Name = Name;
                    oldAds.AdsType = AdsType;
                    oldAds.Resource = Resource;
                    oldAds.LicenseType = LicenseType;
                    oldAds.State = State;
                    oldAds.Zipcode = Zipcode;
                    oldAds.NumberOfPeopleReached = int.Parse(NumberOfPeopleReached);
                    oldAds.Additional = Additional;
                    oldAds.Message = Message;
                    oldAds.LastUpdateBy = cMem.FullName;
                    oldAds.LastUpdateAt = DateTime.UtcNow;
                    oldAds.Attachment = Attachment;
                    Camp.NumberOfPeopleReached = oldAds.NumberOfPeopleReached;
                    Camp.TotalSuccess = oldAds.TotalSuccess;
                    Camp.TotalFailed = oldAds.TotalFailed;
                    db.Entry(oldAds).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();

                    var removedText = AdsStatus.Removed.Text();
                    var total = db.M_Ads.Where(c => c.CampaignId.Equals(Camp.Id) && !c.Status.Equals(removedText)).GroupBy(c => c.CampaignId).Select(c => new
                    {
                        NumberOfPeopleReached = c.Sum(s => s.NumberOfPeopleReached),
                        TotalSuccess = c.Sum(s => s.TotalSuccess),
                        TotalFailed = c.Sum(s => s.TotalFailed)
                    }).FirstOrDefault();
                    Camp.NumberOfPeopleReached = total.NumberOfPeopleReached;
                    Camp.TotalSuccess = total.TotalSuccess;
                    Camp.TotalFailed = total.TotalFailed;
                    db.Entry(Camp).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();

                    return Json(new object[] { true, "Update Ads success.", oldAds });
                }

            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Create new Ads fail. " + ex.Message });
            }
        }
        public JsonResult ChangeStatusAds(string key, string status)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                    throw new Exception("You don't have permission.");

                var ads = db.M_Ads.Find(key);
                if (ads == null)
                    throw new Exception("Ads not found.");
                ads.Status = status; // AdsStatus.Cancel.Text();
                ads.LastUpdateAt = DateTime.UtcNow;
                ads.LastUpdateBy = cMem.FullName;
                db.Entry(ads).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new object[] { true, "Cancel " + ads.Name + " success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Cancel fail. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public List<C_Customer> GetListCustomerByCondition(string resources = "", string licences = "", string states = "", string zipcodes = "", string additional = "", string type = "sms")
        {
            var dataFilter = new List<C_Customer>() { };
            string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
            var customerSlice = db.Store_Services
                        .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new
                        {
                            customerCode = ss.CustomerCode,
                            allowSlice = lp.AllowSlice,
                            active = ss.Active
                        })
                        .Where(c => c.active == 1 && c.allowSlice == true)
                        .Select(s => s.customerCode.ToString())
                        .ToList();

            if (CommonFunc.SearchName(resources, AdsCustommer.Merchant.Text())) // Merchant
            {
                var list_customer = db.C_Customer.Where(x => !STORE_IN_HOUSE.Equals(x.Type)
                                                    && !customerSlice.Any(c => c.Equals(x.CustomerCode)) && x.WordDetermine != "trial");
                list_customer = list_customer.Where(c => db.C_MerchantSubscribe.Any(m => m.CustomerCode == c.CustomerCode && !"inactive".Equals(m.Status)));
                dataFilter.AddRange(list_customer);
            }
            if (CommonFunc.SearchName(resources, AdsCustommer.Trial.Text())) // Trial
            {
                var list_customer = db.C_Customer.Where(x => x.WordDetermine.Trim().ToLower() == "trial" && !STORE_IN_HOUSE.Equals(x.Type));
                dataFilter.AddRange(list_customer);
            }
            if (CommonFunc.SearchName(resources, AdsCustommer.Lead.Text())) // Lead
            {
                var list_customer = db.C_Customer.Where(x => !db.O_Orders.Any(o => o.CustomerCode.Equals(x.CustomerCode)) && !STORE_IN_HOUSE.Equals(x.Type));
                dataFilter.AddRange(list_customer);
            }
            if (CommonFunc.SearchName(resources, AdsCustommer.Potential.Text())) // Potential
            {
                var list_customer = db.C_Customer.Where(x => !STORE_IN_HOUSE.Equals(x.Type)
                                                    && !customerSlice.Any(c => c.Equals(x.CustomerCode)));
                list_customer = list_customer.Where(c => !db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)));
                dataFilter.AddRange(list_customer);
            }
            if (CommonFunc.SearchName(resources, AdsCustommer.Data.Text())) // Data import
            {
            }
            if (CommonFunc.SearchName(resources, AdsCustommer.Other.Text())) // Other data
            {
            }

            if (!string.IsNullOrEmpty(states))
            {
                var list_customer = db.C_Customer.Where(c => (states.Contains(c.BusinessState ?? string.Empty) || states.Contains(c.SalonState ?? string.Empty) || states.Contains(c.State ?? string.Empty)) && !STORE_IN_HOUSE.Equals(c.Type));
                dataFilter.AddRange(list_customer);
            }
            if (!string.IsNullOrEmpty(zipcodes))
            {
                var searchZipcodes = zipcodes.Split(',');
                var list_customer = db.C_Customer.Where(c => searchZipcodes.Any(x=>x == c.BusinessZipCode || x==c.Zipcode||x==c.SalonZipcode ) && !STORE_IN_HOUSE.Equals(c.Type));
                dataFilter.AddRange(list_customer);
            }
            if (!string.IsNullOrEmpty(additional))
            {
                var _ran = new Random();
                additional.Split(',').ForEach(a =>
                {
                    var new_Cus = new C_Customer
                    {
                        Id = long.Parse(_ran.Next(1, 999999).ToString().PadLeft(5, '0')),
                        CellPhone = a
                    };
                    dataFilter.Add(new_Cus);
                });
            }
            if (!string.IsNullOrEmpty(licences))
            {
                var stringLicense = licences?.Split(',');
                var list_customer = db.C_Customer.Where(c => db.Store_Services.Any(ss => ss.CustomerCode == c.CustomerCode && stringLicense.Any(x=>x == ss.ProductCode) && ss.Active == 1 && ss.RenewDate>DateTime.UtcNow)                                                       
                                                            && c.WordDetermine != "Trial"
                                                            && !STORE_IN_HOUSE.Equals(c.Type)
                                                            ).AsEnumerable();
                dataFilter.AddRange(list_customer);
            }

            var dataList = dataFilter.GroupBy(c => c.Id).Select(x => x.First()).ToList();
            return dataList;
        }
        [HttpPost]
        public JsonResult PublicNumberReached(string resources = "", string licences = "", string states = "", string zipcodes = "", string additional = "")
        {
            try
            {
                if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                    throw new Exception("You don't have permission.");

                var number = GetListCustomerByCondition(resources, licences, states, zipcodes, additional).Count();
                return Json(new object[] { true, number }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error", "Get number of people reached error. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<JsonResult> SendSMSToMerchant(string AdsId, int startNumber, int countNumber = 100)
        {
            if (access.Any(k => k.Key.Equals("campaign_ads")) == false || access["campaign_ads"] == false)
                throw new Exception("You don't have permission.");

            var ads = db.M_Ads.Find(AdsId);
            if (ads == null)
                throw new Exception("Ads not found.");
            var Camp = db.M_Campaign.Find(ads.CampaignId);
            if (Camp == null)
                throw new Exception("Ads do not belong to any campaign.");

            try
            {
                var nextNumber = startNumber;
                if (startNumber + countNumber < ads.NumberOfPeopleReached)
                {
                    nextNumber = startNumber + countNumber;
                }
                else
                {
                    nextNumber = ads.NumberOfPeopleReached ?? -1;
                    countNumber = (ads.NumberOfPeopleReached ?? startNumber) - startNumber;
                }

                if (ads.AdsType == "sms") //Send SMS
                {
                    List<NotificationSMSModel> listNoty = new List<NotificationSMSModel>() { };
                    var mediaUrl = string.IsNullOrEmpty(ads.Attachment) ? null : ads.Attachment.Split('|').Select(l => new Uri(ConfigurationManager.AppSettings["IMSUrl"] + l)).ToList();
                    var listCustomer = GetListCustomerByCondition(ads.Resource, ads.LicenseType, ads.State, ads.Zipcode, ads.Additional, ads.AdsType).GetRange(startNumber, countNumber).GroupBy(c => c.CellPhone).Select(x => x.First()).ToList();
                    listNoty = listCustomer.Select(c => new NotificationSMSModel
                    {
                        PhoneNumber = c.CellPhone ?? c.OwnerMobile ?? c.SalonPhone,
                        Message = ads.Message,
                        MediaUrl = mediaUrl
                    }).Where(c => !string.IsNullOrEmpty(c.PhoneNumber)).ToList();

                    var resNoty = await _sMSService.Create(listNoty);
                    ads.TotalSuccess += resNoty.TotalSuccess;
                    ads.TotalFailed += resNoty.TotalFailed;
                    ads.FailedReason += resNoty.Message;
                    ads.Status = AdsStatus.Sent.Text();

                    await db.SaveChangesAsync();

                    var removedText = AdsStatus.Removed.Text();
                    var total = db.M_Ads.Where(c => c.CampaignId.Equals(Camp.Id) && !c.Status.Equals(removedText)).GroupBy(c => c.CampaignId).Select(c => new
                    {
                        NumberOfPeopleReached = c.Sum(s => s.NumberOfPeopleReached),
                        TotalSuccess = c.Sum(s => s.TotalSuccess),
                        TotalFailed = c.Sum(s => s.TotalFailed)
                    }).FirstOrDefault();
                    Camp.NumberOfPeopleReached = total.NumberOfPeopleReached;
                    Camp.TotalSuccess = total.TotalSuccess;
                    Camp.TotalFailed = total.TotalFailed;
                    db.Entry(Camp).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                else // Send Email
                {
                    throw new Exception("Send mail.");
                }

                return Json(new object[] { true, nextNumber, ads.FailedReason });
            }
            catch (Exception ex)
            {
                ads.Status = AdsStatus.Failed.Text();
                await db.SaveChangesAsync();
                return Json(new object[] { false, -1, ex.Message
    });
            }
        }
    }
}