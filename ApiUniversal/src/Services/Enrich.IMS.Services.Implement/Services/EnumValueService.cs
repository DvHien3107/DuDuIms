using Enrich.Core.Container;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.EnumValue;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class EnumValueService : GenericService<EnumValue, EnumValueDto>, IEnumValueService
    {
        private readonly IEnrichContainer _container;

        private IEnumValueRepository _repository => _repositoryGeneric as IEnumValueRepository;
        private IEnumValueMapper _mapper => _mapperGeneric as IEnumValueMapper;

        public EnumValueService(IEnumValueRepository repository, IEnumValueMapper mapper, IEnrichContainer container)
            : base(repository, mapper)
        {
            _container = container;
        }

        public async Task<(string Namespace, IEnumerable<IdNameDto> IdNames)> GetLookupAsync(LookupDataType type, EnumValueLookupDataRequest request)
        {
            var response = default((string Namespace, IEnumerable<IdNameDto> IdNames));

            switch (type)
            {
                case LookupDataType.SaleLeadType:
                    request.Namespace = EnumValueNameSpace.SaleLeadType;
                    break;
                case LookupDataType.SaleLeadStatus:
                    request.Namespace = EnumValueNameSpace.SaleLeadStatus;
                    break;            
                case LookupDataType.MerchantTabName:
                    request.Namespace = EnumValueNameSpace.MerchantTabName;
                    break;
                default:
                    return response;
            }

            response.Namespace = request.Namespace;
            response.IdNames = await GetIdNamesAsync(request);

            return response;
        }
        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
        }
    }
}
