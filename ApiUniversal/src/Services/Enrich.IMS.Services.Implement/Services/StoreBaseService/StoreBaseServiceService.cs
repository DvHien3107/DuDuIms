using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Core;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.StoreBaseService;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class StoreBaseServiceService : EnrichBaseService<StoreBaseService, StoreBaseServiceDto>, IStoreBaseServiceService
    {
        #region field
        private IStoreBaseServiceMapper _mapper => _mapperGeneric as IStoreBaseServiceMapper;
        private readonly IStoreBaseServiceRepository _repository;
        private readonly IEnrichLog _enrichLog;
        #endregion

        #region contructor
        public StoreBaseServiceService(
            IStoreBaseServiceRepository repository,
            IStoreBaseServiceMapper mapper,
            IEnrichLog enrichLog) : base(repository, mapper)
        {
            _repository = repository;
            _enrichLog = enrichLog;
        }
        #endregion


        /// <summary>
        /// Validation SMS remaining for send
        /// </summary>
        /// <param name="store"></param>
        /// <param name="totalnumber"></param>
        /// <returns></returns>
        public async Task<BaseServiceRemainingResponse> SyncRemainingValidationAsync(string store, int totalnumber, BaseServiceEnum.Code baseService)
        {
            var sessionId = Guid.NewGuid().ToString("N");
            try
            {
                var result = new BaseServiceRemainingResponse();
                var baseServiceSMS = await _repository.GetBaseServiceByStoreCode(baseService.ToString(), store);
                if (baseServiceSMS == null) throw new Exception(ValidationMessages.NotFound.BaseService);

                var remaining = baseServiceSMS.RemainingValue - totalnumber;
                if (totalnumber == 0)
                {
                    result.Accept = true;
                    result.Remaining = baseServiceSMS.RemainingValue;

                    _enrichLog.Info(@$"{sessionId} [SMS remaining validation] Accept {JsonConvert.SerializeObject(new
                    {
                        StoreCode = store,
                        NumberRequest = totalnumber,
                        Remaining = baseServiceSMS.RemainingValue
                    })}");
                }
                else if (remaining >= 0)
                {
                    baseServiceSMS.RemainingValue = remaining;
                    await _repository.UpdateAsync(baseServiceSMS);

                    result.Accept = true;
                    result.Remaining = baseServiceSMS.RemainingValue;

                    _enrichLog.Info(@$"{sessionId} [SMS remaining validation] Accept {JsonConvert.SerializeObject(new
                    {
                        StoreCode = store,
                        NumberRequest = totalnumber,
                        Remaining = baseServiceSMS.RemainingValue
                    })}");
                }
                else
                {
                    result.Accept = false;
                    result.Remaining = baseServiceSMS.RemainingValue;

                    _enrichLog.Info(@$"{sessionId} [SMS remaining validation] Decline {JsonConvert.SerializeObject(new
                    {
                        StoreCode = store,
                        NumberRequest = totalnumber,
                        Remaining = baseServiceSMS.RemainingValue
                    })}");
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}