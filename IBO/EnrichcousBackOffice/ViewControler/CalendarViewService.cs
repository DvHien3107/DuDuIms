using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.ViewControler
{
    public class CalendarViewService
    {
        private WebDataModel db;

        public CalendarViewService(WebDataModel db = null)
        {
            if (db == null)
            {
                this.db = new WebDataModel();
            }
            else
            {
                this.db = db;
            }
        }

        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        public List<Calendar_Event> GetAllEvent(int year, out string err, bool viewall = false)
        {
            try
            {
                if ((access.Any(k => k.Key.Equals("home_calendarevent_viewall")) == true && access["home_calendarevent_viewall"] == true)
                    && viewall)
                {
                    var ev = db.Calendar_Event.Where(e => e.StartEvent.Contains(year.ToString())).ToList();
                    err = string.Empty;
                    return ev;
                }
                else
                {
                    var ev = db.Calendar_Event.Where(e => e.StartEvent.Contains(year.ToString()) && e.MemberNumber == cMem.MemberNumber).ToList();
                    err = string.Empty;
                    return ev;
                }

            }
            catch (Exception ex)
            {
                err = (ex.InnerException?.Message) ?? ex.Message;
                return null;
            }
        }

        public Calendar_Event GetEventById(string id, out string err)
        {
            try
            {
                var ev = db.Calendar_Event.Find(id);
                err = string.Empty;
                return ev;

            }
            catch (Exception ex)
            {
                err = (ex.InnerException?.Message) ?? ex.Message;
                return null;
            }
        }

        /// <summary>
        /// add new event
        /// </summary>
        /// <param name="calendar_Event"></param>
        /// <param name="db"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public Calendar_Event AddNewEvent(Calendar_Event calendar_Event, out string err)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(calendar_Event.Id))
                {
                    calendar_Event.Id = DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(11, 9999).ToString();
                }
                db.Calendar_Event.Add(calendar_Event);
                db.SaveChanges();
                err = string.Empty;
                return calendar_Event;
            }
            catch (Exception ex)
            {
                err = (ex.InnerException?.Message) ?? ex.Message;
                return null;
            }
        }

        public Calendar_Event UpdateEvent(string id, Calendar_Event ce, string updateBy, out string err)
        {
            try
            {
                var ev = db.Calendar_Event.Find(id);
                if (ev == null)
                {
                    throw new Exception("Event is not found");
                }

                ev.Name = ce.Name;
                ev.StartEvent = ce.StartEvent;
                ev.EndEvent = ce.EndEvent;
                ev.Description = ce.Description;
                ev.TimeZone = ce.TimeZone;
                ev.Done = ce.Done;
                if (ev.Done == 1)
                {
                    ev.Color = "#228B22";//green
                }
                else if ((!string.IsNullOrWhiteSpace(ev.EndEvent) && DateTime.Parse(ev.EndEvent) < DateTime.Now)
                        || (string.IsNullOrWhiteSpace(ev.EndEvent) && !string.IsNullOrWhiteSpace(ev.StartEvent) && DateTime.Parse(ev.StartEvent)< DateTime.Now))
                {
                    ev.Color = "#fb1912";//red
                }
                else
                {
                    ev.Color = "#3788d8";//blue
                }
                ev.LastUpdateBy = updateBy;
                ev.LastUpdateAt = DateTime.UtcNow;
                db.Entry(ev).State = System.Data.Entity.EntityState.Modified;
                var update_lead = db.C_SalesLead.Where(l => l.CustomerCode == ev.CustomerCode).FirstOrDefault();
                if (update_lead != null)
                {
                    update_lead.UpdateBy = cMem.FullName;
                    update_lead.UpdateAt = DateTime.UtcNow;
                    db.Entry(update_lead).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                err = string.Empty;
                return ev;

            }
            catch (Exception ex)
            {
                err = (ex.InnerException?.Message) ?? ex.Message;
                return null;
            }


        }

        public Calendar_Event UpdateEvent(string id, string start, string end, string updateBy, out string err)
        {
            try
            {
                var ev = db.Calendar_Event.Find(id);
                if (ev == null)
                {
                    throw new Exception("Event is not found");
                }

                ev.StartEvent = start;
                ev.EndEvent = end;
                ev.LastUpdateBy = updateBy;
                ev.LastUpdateAt = DateTime.UtcNow;
                if (ev.Done == 1)
                {
                    ev.Color = "#228B22";//green
                }
                else if ((!string.IsNullOrWhiteSpace(ev.EndEvent) && DateTime.Parse(ev.EndEvent) < DateTime.UtcNow)
                       || (string.IsNullOrWhiteSpace(ev.EndEvent) && !string.IsNullOrWhiteSpace(ev.StartEvent) && DateTime.Parse(ev.StartEvent) < DateTime.UtcNow))
                {
                    ev.Color = "#fb1912";//orange
                }
                else
                {
                    ev.Color = "#3788d8";//blue
                }
                db.Entry(ev).State = System.Data.Entity.EntityState.Modified;
                var update_lead = db.C_SalesLead.Where(l => l.CustomerCode == ev.CustomerCode).FirstOrDefault();
                if (update_lead != null)
                {

                    update_lead.UpdateBy = cMem.FullName;
                    update_lead.UpdateAt = DateTime.UtcNow;
                    db.Entry(update_lead).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();
                err = string.Empty;
                return ev;

            }
            catch (Exception ex)
            {
                err = (ex.InnerException?.Message) ?? ex.Message;
                return null;
            }


        }

        public bool RemoveEvent(string id, out string err)
        {
            try
            {
                var ev = db.Calendar_Event.Find(id);
                if (ev == null)
                {
                    throw new Exception("Event is not found");
                }

                db.Calendar_Event.Remove(ev);
                db.SaveChanges();
                err = string.Empty;
                return true;

            }
            catch (Exception ex)
            {
                err = (ex.InnerException?.Message) ?? ex.Message;
                return false;
            }

        }

    }
}