using Enrich.BusinessEvents.IMS;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SendEmail;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class OrderEventService : GenericService<OrderEvent, OrderEventDto>, IOrderEventService
    {
        private readonly IOrderEventRepository _repository;
        private readonly IEnrichSendEmail _enrichSendEmail;
        private readonly EnrichContext _context;
        private readonly IEnrichLog _logger;
        private readonly string _imsHost;
        private readonly string[] _emails;

        public OrderEventService(
            IConfiguration appConfig,
            IOrderEventRepository repository,
            IOrderEventMapper mapper,
            EnrichContext context,
            IEnrichSendEmail enrichSendEmail,
            IEnrichLog logger)
            : base(repository, mapper)
        {
            _imsHost = appConfig["IMSHost"];
            _emails = appConfig.GetSection("ReciverEmails").Get<string[]>();
            _repository = repository;
            _context = context;
            _enrichSendEmail = enrichSendEmail;
            _logger = logger;
        }

        /// <summary>
        /// Send notification payment later list to config emails
        /// </summary>
        /// <param name="events"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task SendNotificationPaymentLaterAsync(IEnumerable<BaseEvent<OrderPaymentLaterEvent>> events, string sessionId = "")
        {
            if (_emails.Length == 0)
            {
                _logger.Error($"{sessionId} {ValidationMessages.NotFound.EmailConfig}");
                return;
            }
            string timeNow = TimeHelper.GetUTCNow().ToString(Constants.Format.Date_MMMddyyyy);
            string subject = $"List payment later invoice on {timeNow}";
            string content = $"Dear,<br/><br/>" +
                             $"<p>List payment later invoice on {timeNow}</p>" +
                             $"<table style='border-collapse: collapse; margin-top: 10px;font-size: 12px;'><thead><tr>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Order code</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Store code</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Store name</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Grand total</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Due date</th>" +
                             $"</tr></thead><tbody>";

            foreach(var item in events)
            {
                //build content email
                if(item.Value != null)
                {
                    content += $"<tr>" +
                                $"<td style='border:1px solid gray;padding: 5px;text-align: left'><b><a href='{_imsHost}/order/estimatesdetail/{item.Value.OrderId}'>#<b>{item.Value.OrderCode}</b></a></b></td>" +
                            $"<td style='border:1px solid gray;padding: 5px;text-align: left'><b><a href='{_imsHost}/merchantman/detail/{item.Value.CustomerId}'>{item.Value.StoreCode}</a></b></td>" +
                                $"<td style='border:1px solid gray;padding: 5px;text-align: left'>{item.Value.StoreName}</td>" +
                                $"<td style='border:1px solid gray;padding: 5px;text-align: left'>{item.Value.GrandTotal.ToString(Constants.Format.CurrencyDollar)}</td>" +
                                $"<td style='border:1px solid gray;padding: 5px;text-align: left'>{item.Value.DueDate?.ToString(Constants.Format.Date_MMMddyyyy)}</td>" +
                                $"</tr>";
                }

                //update event completed
                await EventCompletedAsync(item);
            }
            content += $"</tbody></table><br/>Thank you!";

            var email = new SendEmailBySendGridTemplateId
            {
                TemplateId = SendGridTemplate.FreeStyle,
                Service = _context.UserFullName,
                Subject = subject,
                Data = new { subject, content }
            };
            email.To = _emails.Select(em => new EmailAddress { Email = em });

            await _enrichSendEmail.SendBySendGridTemplateId(email);
            _logger.Info($"{sessionId} Send email success", new { events = events.Select(c => c.BusinessEventId), email.To});
        }

        /// <summary>
        /// Send notification payment failed to config emails
        /// </summary>
        /// <param name="events"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task SendNotificationPaymentFailedAsync(IEnumerable<BaseEvent<OrderPaymentFailedEvent>> events, string sessionId = "")
        {
            if (_emails.Length == 0)
            {
                _logger.Error($"{sessionId} {ValidationMessages.NotFound.EmailConfig}");
                return;
            }
            string timeNow = TimeHelper.GetUTCNow().ToString(Constants.Format.Date_MMMddyyyy);
            string subject = $"List payment failed invoice on {timeNow}";
            string content = $"Dear,<br/><br/>" +
                             $"<p>List payment failed invoice on {timeNow} UTC</p>" +
                             $"<table style='border-collapse: collapse; margin-top: 10px;font-size: 12px;'><thead><tr>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Order code</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Grand total</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Store</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Payment type</th>" +
                             $"<th style='border:1px solid gray;padding: 5px'>Transaction note</th>" +
                             $"</tr></thead><tbody>";

            foreach(var item in events)
            {
                //build content email
                if(item.Value != null)
                {
                    content += $"<tr>" +
                            $"<td style='border:1px solid gray;padding: 5px;text-align: left'><b><a href='{_imsHost}/order/estimatesdetail/{item.Value.OrderId}'>#{item.Value.OrderCode}</a></b></td>" +
                            $"<td style='border:1px solid gray;padding: 5px;text-align: left'>{item.Value.GrandTotal.ToString(Constants.Format.CurrencyDollar)}</td>" +
                            $"<td style='border:1px solid gray;padding: 5px;text-align: left'><b><a href='{_imsHost}/merchantman/detail/{item.Value.CustomerId}?tab=transaction'>{item.Value.StoreCode}</a></b></td>" +
                            $"<td style='border:1px solid gray;padding: 5px;text-align: left'>{item.Value.PaymentType}</td>" +
                            $"<td style='border:1px solid gray;padding: 5px;text-align: left'>{item.Value.ResponseText}</td>" +
                            $"</tr>";
                }

                //update event completed
                await EventCompletedAsync(item);
            }
            content += $"</tbody></table><br/>Thank you!";

            var email = new SendEmailBySendGridTemplateId
            {
                TemplateId = SendGridTemplate.FreeStyle,
                Service = _context.UserFullName,
                Subject = subject,
                Data = new { subject, content }
            };
            email.To = _emails.Select(em => new EmailAddress { Email = em });

            await _enrichSendEmail.SendBySendGridTemplateId(email);
            _logger.Info($"{sessionId} Send email success", new { events = events.Select(c=>c.BusinessEventId), email.To});
        }

        /// <summary>
        /// Change event data to completed
        /// </summary>
        /// <param name="baseEvent"></param>
        /// <returns></returns>
        private async Task EventCompletedAsync(BaseEvent baseEvent)
        {
            var orderEvent = new OrderEvent
            {
                Id = baseEvent.Id,
                Status = (int)BusinessEventEnum.Status.Completed,
                CompleteAt = TimeHelper.GetUTCNow(),
                CompleteBy = _context.UserFullName,
                BusinessEventId = baseEvent.BusinessEventId
            };

            await _repository.UpdateAsync(orderEvent);
        }
    }
}