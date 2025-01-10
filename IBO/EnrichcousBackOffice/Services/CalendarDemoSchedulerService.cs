using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils;
using EnrichcousBackOffice.Utils.IEnums;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TimeZoneConverter;

namespace EnrichcousBackOffice.Services
{
    public class CalendarDemoSchedulerService
    {
        private readonly string ApplicationName = "Calendar Demo";
        private readonly string calendarId = "primary";
        private readonly Google.Apis.Calendar.v3.CalendarService _calendarGoogleService = new Google.Apis.Calendar.v3.CalendarService();
        private readonly string IMSUrl = ConfigurationManager.AppSettings["IMSUrl"];
        private readonly string TypeDemoScheduler = EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString();
        private readonly WebDataModel db = new WebDataModel();
        public  readonly string Email = "";
        private const string ColorIDDemoScheduler = "7";
        private const string ColorIDSuccess = "2";
        private const string ColorIDCancel = "11";
        public CalendarDemoSchedulerService(UserCredential credential)
        {
            _calendarGoogleService = new Google.Apis.Calendar.v3.CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            Email = credential.UserId;
        }
        public List<Calendar_Event> GetList()
        {
            var member = db.P_Member.Where(x => x.PersonalEmail == Email).FirstOrDefault();
            var ListDemoScheduler = db.Calendar_Event.Where(x => x.Type == TypeDemoScheduler && x.MemberNumber == member.MemberNumber).ToList();
            return ListDemoScheduler;
        }
        public CalendarListEntry GetCalendar()
        {
            var calendar = _calendarGoogleService.CalendarList.Get("primary").Execute();
            return calendar;
        }
        public Calendar_Event GetEventById(string Id)
        {
            // Define parameters of request.
            Calendar_Event Event = db.Calendar_Event.Find(Id);
            return Event;
        }
        public Calendar_Event Insert(Calendar_Event ev)
        {
            // Event Event = _calendarGoogleService.Events.Insert(ev, calendarId).Execute();
            //save to db local
            Event eventCalendarGoogle = new Event();
            eventCalendarGoogle.Summary = ev.Name;
            eventCalendarGoogle.Description = ev.Description;
            //if (!string.IsNullOrEmpty(ev.AttendeesNumber))
            //{
            //  var Attendees = ev.AttendeesNumber.Split(',').Select(membernumber => new EventAttendee[] {
            //            new EventAttendee() { Email = db.P_Member.FirstOrDefault(x=>x.PersonalEmail== membernumber).PersonalEmail },
            //  });
            //}
            var cus = db.C_Customer.Where(x => x.CustomerCode == ev.CustomerCode).FirstOrDefault();
            var Attendees = new EventAttendee[] {
                  new EventAttendee() { Email = cus.SalonEmail },
                  new EventAttendee() { Email = cus.Email },
              };
            eventCalendarGoogle.Attendees = Attendees;
            string TimeZoneIANA = ev.TimeZone;
            if (ev.TimeZone == "Eastern" || ev.TimeZone == "Central" || ev.TimeZone == "Mountain" || ev.TimeZone == "Pacific" || ev.TimeZone == "VietNam")
            {
                var TimeZoneId = Ext.EnumParse<TIMEZONE_NUMBER_BY_ID>(ev.TimeZone).Code<string>();
                TimeZoneIANA = TZConvert.WindowsToIana(TimeZoneId);
            }
         
            var Start = new EventDateTime()
            {
                DateTime = AppFunc.ParseTime(ev.StartEvent),
                TimeZone = TimeZoneIANA
            };
            var End = new EventDateTime()
            {
                DateTime = AppFunc.ParseTime(ev.EndEvent),
                TimeZone = TimeZoneIANA
            };
            eventCalendarGoogle.Start = Start;
            eventCalendarGoogle.End = End;
            eventCalendarGoogle.Location = ev.Location;
            var Reminders = new Event.RemindersData()
            {
                UseDefault = false,
                Overrides = new EventReminder[] {
                        new EventReminder() { Method = "email", Minutes = 24 * 60 },
                        new EventReminder() { Method = "sms", Minutes = 60 },
                    }
            };
            eventCalendarGoogle.Reminders = Reminders;
            if (ev.Done == 0)
            {
                eventCalendarGoogle.ColorId = ColorIDCancel;
            }
            else if (ev.Done == 1)
            {
                eventCalendarGoogle.ColorId = ColorIDSuccess;
            }
            else
            {
                eventCalendarGoogle.ColorId = ColorIDDemoScheduler;
            }
            Event Event = _calendarGoogleService.Events.Insert(eventCalendarGoogle, calendarId).Execute();
            // update id event calendar gg for local
            ev.GoogleCalendarId = Event.Id;
            ev.ETag = Event.ETag;
            db.Calendar_Event.Add(ev);
            db.SaveChanges();
            return ev;
        }
        public Calendar_Event Update(Calendar_Event ev)
        {
            db.SaveChanges();
            Event eventCalendarGoogle = _calendarGoogleService.Events.Get(calendarId, ev.GoogleCalendarId).Execute();
            eventCalendarGoogle.Summary = ev.Name;
            eventCalendarGoogle.Description = ev.Description;
            if (!string.IsNullOrEmpty(ev.AttendeesNumber))
            {
                var Attendees = ev.AttendeesNumber.Split(',').Select(membernumber => new EventAttendee[] {
                        new EventAttendee() { Email = db.P_Member.FirstOrDefault(x=>x.PersonalEmail== membernumber).PersonalEmail },
              });
            }
            //Attendees.;
            string TimeZoneIANA = ev.TimeZone;
            if (ev.TimeZone == "Eastern" || ev.TimeZone == "Central" || ev.TimeZone == "Mountain" || ev.TimeZone == "Pacific" || ev.TimeZone == "VietNam")
            {
                var TimeZoneId = Ext.EnumParse<TIMEZONE_NUMBER_BY_ID>(ev.TimeZone).Code<string>();
                TimeZoneIANA = TZConvert.WindowsToIana(TimeZoneId);
            }
            var Start = new EventDateTime()
            {
                DateTime = AppFunc.ParseTime(ev.StartEvent),
                TimeZone = TimeZoneIANA
            };
            var End = new EventDateTime()
            {
                DateTime = AppFunc.ParseTime(ev.EndEvent),
                TimeZone = TimeZoneIANA
            };
            if (ev.Done == 0)
            {
                eventCalendarGoogle.ColorId = ColorIDCancel;
                eventCalendarGoogle.Status = "cancelled";
            }
            else if (ev.Done == 1)
            {
                eventCalendarGoogle.ColorId = ColorIDSuccess;
                eventCalendarGoogle.Status = "confirmed";
            }
            else
            {
                eventCalendarGoogle.ColorId = ColorIDDemoScheduler;
                eventCalendarGoogle.Status = "confirmed";
            }

            eventCalendarGoogle.Start = Start;
            eventCalendarGoogle.End = End;
            eventCalendarGoogle.Location = ev.Location;
           _calendarGoogleService.Events.Update(eventCalendarGoogle,calendarId, ev.GoogleCalendarId).Execute();
            return ev;
        }
        public void Delete(string GoogleEventId)
        {
            Event eventCalendarGoogle = _calendarGoogleService.Events.Get(calendarId, GoogleEventId).Execute();
            if (eventCalendarGoogle.Status != "cancelled")
            {
               _calendarGoogleService.Events.Delete(calendarId, GoogleEventId).Execute();
            }
        }
        public string Delete(Calendar_Event ev)
        {
            var delete = _calendarGoogleService.Events.Delete(calendarId, ev.GoogleCalendarId).Execute();
            db.Calendar_Event.Remove(ev);
            db.SaveChanges();
            return delete;
        }
        public void WatchEvent()
        {
            var channel = new Channel();
            channel.Id = Email;
            channel.Address = IMSUrl + "/Home/RefetchData?UserId="+Email;
            channel.Type = "web_hook";
            _calendarGoogleService.Events.Watch(channel, calendarId).Execute();
        }

        public void SyncDataLocallyAndGoogleCalendar()
        {
            var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == Email);
            EventsResource.ListRequest request = _calendarGoogleService.Events.List("primary");
            // request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = false;
            request.MaxResults = 2500;
            Events events = request.Execute();
            foreach(var item in events.Items)
            {
                var calendar_event = db.Calendar_Event.FirstOrDefault(x => x.Type == TypeDemoScheduler && x.MemberName == member.MemberNumber&& x.GoogleCalendarId ==item.Id);
                if (calendar_event != null)
                {
                    calendar_event.Name = item.Summary;
                    calendar_event.Description = item.Description;
                  
                }                 
            }
            db.SaveChanges();
        }
    }
}