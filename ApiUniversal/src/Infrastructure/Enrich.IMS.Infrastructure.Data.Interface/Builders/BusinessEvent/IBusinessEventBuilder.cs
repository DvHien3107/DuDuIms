using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface IBusinessEventBuilder
    {
        /// <summary>
        /// Builder data for create new business event
        /// </summary>
        void BuildForSave(BusinessEvent businessEvent);
    }
}