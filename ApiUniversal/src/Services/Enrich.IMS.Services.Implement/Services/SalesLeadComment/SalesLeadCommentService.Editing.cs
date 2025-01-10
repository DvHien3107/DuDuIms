using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Entities;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadCommentService
    {
        /// <summary>
        /// create new sales lead comment async
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<SalesLeadCommentUpdateResponse> CreateAsync(SalesLeadCommentDto dto)
        {
            var response = new SalesLeadCommentUpdateResponse();

            await SaveSalesLeadCommentAsync(true, new SalesLeadCommentUpdateRequest { NewDto = dto, ValidateData = true, IsNew = true });

            response.CreatedId = dto.Id;
            return response;
        }

        /// <summary>
        /// Update data sales lead comment 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SalesLeadCommentUpdateResponse> UpdateAsync(SalesLeadCommentUpdateRequest request)
        {
            var response = new SalesLeadCommentUpdateResponse();

            await SaveSalesLeadCommentAsync(false, request, validateData: true);
            //extend response in here

            return response;
        }

        /// <summary>
        /// Save a salesLead comment async
        /// </summary>
        /// <param name="isNew">Insert or update</param>
        /// <param name="request">UpdateSalesLeadRequest </param>
        /// <param name="validateData">Should validate or not</param>
        /// <returns></returns>
        private async Task SaveSalesLeadCommentAsync(bool isNew, SalesLeadCommentUpdateRequest request, bool validateData = true)
        {
            if (validateData)
                await ValidateSaveAsync(isNew, request.NewDto);
            try
            {
                var response = new SalesLeadCommentUpdateResponse();

                // format data before save
                await OptimizeBeforeSaveAsync(isNew, request);

                //map data from dto to entity
                var salesLead = _mapper.Map<SalesLeadComment>(request.NewDto);

                // save data
                request.NewDto.Id = await _repository.AddGetAsync(isNew, salesLead);
            }
            catch (Exception ex)
            {
                ThrowWithInner(ExceptionCodes.SaleLeadComment_SaveError, ex.Message, ex);
            }
        }

        /// <summary>
        /// optimize sales lead comment before save data async
        /// </summary>
        /// <param name="isNew"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task OptimizeBeforeSaveAsync(bool isNew, SalesLeadCommentUpdateRequest request)
        {
            // build sales lead comment
            await _builder.BuildForSave(isNew, request.NewDto);
        }
    }
}