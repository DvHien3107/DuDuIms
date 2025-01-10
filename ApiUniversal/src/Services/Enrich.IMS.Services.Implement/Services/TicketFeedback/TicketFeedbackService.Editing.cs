using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Dto.TicketFeedback;
using Enrich.IMS.Entities;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class TicketFeedbackService
    {

        /// <summary>
        /// create new ticket feedback async
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<TicketFeedbackUpdateResponse> CreateAsync(TicketFeedbackDto dto)
        {
            var response = new TicketFeedbackUpdateResponse();

            await SaveTicketFeedbackAsync(true, new TicketFeedbackUpdateRequest { NewDto = dto, ValidateData = true, IsNew = true });

            response.CreatedId = dto.Id;
            return response;
        }

        /// <summary>
        /// Update data sales lead comment 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TicketFeedbackUpdateResponse> UpdateAsync(TicketFeedbackUpdateRequest request)
        {
            var response = new TicketFeedbackUpdateResponse();

            await SaveTicketFeedbackAsync(false, request, validateData: true);
            //extend response in here

            return response;
        }

        private async Task SaveTicketFeedbackAsync(bool isNew, TicketFeedbackUpdateRequest request, bool validateData = true)
        {
            if (validateData)
                await ValidateSaveAsync(isNew, request.NewDto);
            try
            {
                var response = new TicketFeedbackUpdateResponse();

                // format data before save
                await OptimizeBeforeSaveAsync(isNew, request);

                //map data from dto to entity
                var tcketFeedback = _mapper.Map<TicketFeedback>(request.NewDto);

                // save data
                request.NewDto.Id = await _repository.AddGetAsync(isNew, tcketFeedback);
            }
            catch (Exception ex)
            {
                ThrowWithInner(ExceptionCodes.SaleLeadComment_SaveError, ex.Message, ex);
            }
        }

        private async Task OptimizeBeforeSaveAsync(bool isNew, TicketFeedbackUpdateRequest request)
        {
            // build sales lead comment
            await _builder.BuildForSave(isNew, request.NewDto);
        }
    }
}