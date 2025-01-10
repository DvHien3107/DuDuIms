using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Authentication;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface;
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
    public class AccessKeyService : GenericService<AccessKey, AccessKeyDto>, IAccessKeyService
    {
        private readonly IAccessKeyRepository _repository;
        private readonly EnrichContext _context;
        private IAccessKeyMapper _mapper => _mapperGeneric as IAccessKeyMapper;
        public AccessKeyService(IAccessKeyRepository repository
           , IAccessKeyMapper mapper, EnrichContext context) : base(repository, mapper)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<AccessKeyDto> GetAccessKeyByKey(string secretKey)
        {
            var result = _repository.GetAccessKeyByKey(secretKey);
            if (result != null)
                return _mapper.Map<AccessKeyDto>(result);
            return new AccessKeyDto();
        }
    }
}
