using Enrich.Common;
using Enrich.Core.Container;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.NewCustomerGoal;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class NewCustomerGoalService : GenericService<NewCustomerGoal, NewCustomerGoalDto>, INewCustomerGoalService
    {
        private INewCustomerGoalMapper _mapper => _mapperGeneric as INewCustomerGoalMapper;
        private readonly INewCustomerGoalRepository _repository;
        private readonly IEnrichContainer _container;
        private readonly INewCustomerGoalBuilder _builder;

        public NewCustomerGoalService(
            INewCustomerGoalRepository repository,
            INewCustomerGoalMapper mapper,
            IEnrichContainer container,
            INewCustomerGoalBuilder builder)
            : base(repository, mapper)
        {
            _repository = repository;
            _container = container;
            _builder = builder;
        }
        public async Task<NewCustomerGoalDto> GetDetailAsync(int goalId)
        {
            var entity = await _repository.FindByIdAsync(goalId);
            if (entity == null) return null;

            var dto = _mapper.Map<NewCustomerGoalDto>(entity);

            return dto;
        }

        public async Task<NewCustomerGoalUpdateResponse> CreateGoalAsync(NewCustomerGoalDto dto)
        {
            var response = new NewCustomerGoalUpdateResponse();

            await SaveFullAsync(true, new NewCustomerGoalUpdateRequest { NewDto = dto, ValidateData = true, IsNew = true });

            response.CreatedId = dto.Id;
            return response;
        }

        /// <summary>
        /// Update an existed saleslead
        /// </summary>
        /// <param name="request">UpdateSalesLeadRequest</param>
        /// <returns>UpdateSalesLeadResponse</returns>
        public async Task<NewCustomerGoalUpdateResponse> UpdateGoalAsync(NewCustomerGoalUpdateRequest request)
        {
            var response = new NewCustomerGoalUpdateResponse();

            await SaveFullAsync(false, request, validateData: true);

            return response;
        }

        private async Task SaveFullAsync(bool isNew, NewCustomerGoalUpdateRequest request, bool validateData = true)
        {

            if (validateData)
                await ValidateSaveAsync(isNew, request.NewDto, request.OldDto);
            try
            {
                var response = new NewCustomerGoalUpdateResponse();

                _builder.BuildForSave(isNew, request.NewDto);

                //map data from dto to entity
                var goal = _mapper.Map<NewCustomerGoal>(request.NewDto);

                // save data
                await _repository.SaveByTranAsync(isNew, goal);
                request.NewDto.Id = goal.Id;
            }
            catch (Exception ex)
            {
                ThrowWithInner(ExceptionCodes.Generic_CannotSave, ex.Message, ex);
            }
        }

    }
}
