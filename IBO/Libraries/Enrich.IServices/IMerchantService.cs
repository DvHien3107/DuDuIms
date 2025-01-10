using Enrich.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices
{
    public interface IMerchantService
    {
        void SaveHistoryUpdate(string storeCode, string Action);
    }
}
