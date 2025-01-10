using Enrich.Core.UnitOfWork.Data;
using Enrich.DataTransfer;
using Enrich.Entities;
using Enrich.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services
{
    public class MerchantService : IMerchantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EnrichContext enrichContext;

        public MerchantService(IUnitOfWork unitOfWork, EnrichContext enrichContext)
        {
            _unitOfWork = unitOfWork;
            this.enrichContext = enrichContext;
        }

        public void SaveHistoryUpdate(string customerCode, string Action)
        {
            try
            {
                var customer = _unitOfWork.Repository<C_Customer>().Table.Where(x => x.CustomerCode == customerCode).FirstOrDefault();
                if (customer != null)
                {
                    var newHistory = (enrichContext.MemberFullName ?? "IMS system") + "$" + DateTime.UtcNow + "$" + Action;
                    var oldHistory = (customer.UpdateBy ?? string.Empty).Split('|').ToList();
                    if (oldHistory.Count == 3) oldHistory = oldHistory.Skip(1).ToList();
                    oldHistory.Add(newHistory);
                    customer.UpdateBy = string.Join("|", oldHistory);
                    _unitOfWork.Repository<C_Customer>().Update(customer);
                    //db.Entry(Customer).State = EntityState.Modified;
                    _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                //ignore
            }
        }
    }
}
