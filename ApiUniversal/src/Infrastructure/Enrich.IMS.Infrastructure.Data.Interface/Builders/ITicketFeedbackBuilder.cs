using Enrich.IMS.Dto;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface ITicketFeedbackBuilder
    {
        /// <summary>
        /// Build sales lead comment for save
        /// </summary>
        /// <param name="isNew"></param>
        /// <param name="comment">sales lead comment</param>
        /// <returns></returns>
        Task BuildForSave(bool isNew, TicketFeedbackDto ticketFeedback);
    }
}
