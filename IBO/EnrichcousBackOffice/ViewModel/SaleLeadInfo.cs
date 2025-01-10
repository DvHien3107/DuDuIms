using System;
using System.Linq;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Utils;
using Newtonsoft.Json;

namespace EnrichcousBackOffice.ViewModel
{
    public class SaleLeadInfo : C_Customer
    {
        public string SaleLeadId { get; set; }
        public int? SL_Status { get; set; }
        public string SL_StatusName { get; set; }
        public bool IsSendMailCreateTrial { get; set; }
        public bool IsVerify { get; set; }
        public int? PotentialRateScore { get; set; }
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public long? TeamNumber { get; set; }
        public string TeamName { get; set; }
        public string Features_Interes { get; set; }
        public string Features_Interes_other { get; set; }
        public MoreInfo MoreInfo { get; set; }
        public DateTime? UpdateAt { get; private set; }
        public string SL_StatusColor { get; set; }
        public Calendar_Event LatestAppointment;
        public Calendar_Event NextAppointment;

        public static SaleLeadInfo MakeSelect(C_Customer c, C_SalesLead sl, C_SalesLead_Status sl_status, bool fullView = false)
        {
            SaleLeadInfo newLead = JsonConvert.DeserializeObject<SaleLeadInfo>(JsonConvert.SerializeObject(c));
            newLead.SaleLeadId = sl.Id;
            newLead.SL_StatusName = sl.SL_StatusName;
            newLead.MemberNumber = sl.MemberNumber;
            newLead.TeamNumber = sl.TeamNumber;
            newLead.PotentialRateScore = sl.PotentialRateScore;
            newLead.Features_Interes = sl.Features_Interes;
            newLead.Features_Interes_other = sl.Features_Interes_other;
            newLead.CreateAt = sl.CreateAt;
            newLead.CreateBy = sl.CreateBy;
            newLead.UpdateAt = sl.UpdateAt;
            newLead.UpdateBy = sl.UpdateBy;
            // Status
            newLead.SL_Status = sl_status.Order;
            newLead.SL_StatusName = sl_status.Name;
            newLead.SL_StatusColor = sl_status.Color;
            newLead.IsSendMailCreateTrial = sl.L_IsSendMail == true ? true : false;
            newLead.IsVerify = sl.L_IsVerify == true ? true : false;
            if ((sl.L_MoreInfo != null))
            {
                newLead.MoreInfo = JsonConvert.DeserializeObject<MoreInfo>(sl.L_MoreInfo);
            }
            if (fullView == false)
            {
                return newLead;
            }

            using (WebDataModel db = new WebDataModel())
            {
                //  Salesperson
                newLead.MemberName = (
                    from m in db.P_Member
                    where m.MemberNumber == sl.MemberNumber
                    select m
                ).FirstOrDefault()?.FullName;
                // Team Name
                newLead.TeamName = (
                   from m in db.P_Department.AsEnumerable()
                   where m.Id == sl.TeamNumber
                   select m
               ).FirstOrDefault()?.Name;
                // Member Number Name
                newLead.MemberName = (
                   from m in db.P_Member.AsEnumerable()
                   where m.MemberNumber == sl.MemberNumber
                   select m
               ).FirstOrDefault()?.FullName;

                // Latest time Appointment
                newLead.LatestAppointment = (from cal in db.Calendar_Event where cal.SalesLeadId == sl.Id select cal).AsEnumerable().Where(cal=>AppFunc.IsPastTime(cal.StartEvent)).OrderByDescending(cal => cal.StartEvent).FirstOrDefault();

                // Future time Appointment
                newLead.NextAppointment = (from cal in db.Calendar_Event where cal.SalesLeadId == sl.Id select cal).AsEnumerable().Where(cal => AppFunc.IsFutureTime(cal.StartEvent) && cal.Done !=1 && cal.Type == "Event").OrderBy(cal => cal.StartEvent).FirstOrDefault();
            }

            return newLead;
        }
    }

}