using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadService
    {
        /// <summary>
        /// Create new salesLead
        /// </summary>
        /// <param name="dto">new SalesLeadDto</param>
        /// <returns>UpdateSalesLeadResponse</returns>
        public async Task<SalesLeadUpdateResponse> CreateSalesLeadAsync(SalesLeadDto dto)
        {
            var response = new SalesLeadUpdateResponse();

            await SaveFullSalesLeadAsync(true, new SalesLeadUpdateRequest { NewDto = dto, ValidateData = true, IsNew = true });

            response.CreatedId = dto.Id;
            return response;
        }

        /// <summary>
        /// Update an existed saleslead
        /// </summary>
        /// <param name="request">UpdateSalesLeadRequest</param>
        /// <returns>UpdateSalesLeadResponse</returns>
        public async Task<SalesLeadUpdateResponse> UpdateSalesLeadAsync(SalesLeadUpdateRequest request)
        {
            var response = new SalesLeadUpdateResponse();
            request.UpdateOption.Customer = true;
            await SaveFullSalesLeadAsync(false, request, validateData: true);
            //extend response in here

            return response;
        }

        /// <summary>
        /// Save a salesLead
        /// </summary>
        /// <param name="isNew">Insert or update</param>
        /// <param name="request">UpdateSalesLeadRequest </param>
        /// <param name="validateData">Should validate or not</param>
        /// <returns></returns>
        private async Task SaveFullSalesLeadAsync(bool isNew, SalesLeadUpdateRequest request, bool validateData = true)
        {

            if (validateData)
                await ValidateSaveSalesLeadAsync(isNew, request.NewDto, request.OldDto);
            try
            {
                var response = new SalesLeadUpdateResponse();

                // format data before save
                await OptimizeSalesLeadBeforeSaveAsync(isNew, request);

                //map data from dto to entity
                var salesLead = _mapper.Map<SalesLead>(request.NewDto);

                // save data
                await _repository.SaveSalesLeadByTranAsync(isNew, salesLead, async (salesLeadId) =>
                {
                    request.NewDto.Id = salesLeadId;
                    await SaveSaleLeadCustomerAsync(isNew, salesLead.CustomerCode, request);
                });
            }
            catch (Exception ex)
            {
                ThrowWithInner(ExceptionCodes.SaleLead_SaveError, ex.Message, ex);
            }
        }

        /// <summary>
        /// build saleslead before saving it
        /// </summary>
        /// <param name="isNew">insert or update</param>
        /// <param name="newDto">new salesLeadDto</param>
        /// <param name="oldDto">old salesLeadDto</param>
        /// <param name="updateOption">SalesLeadUpdateOption</param>
        /// <returns></returns>
        private async Task OptimizeSalesLeadBeforeSaveAsync(bool isNew, SalesLeadUpdateRequest request)
        {
            // build sales lead
            await _builder.BuildForSave(isNew, request.NewDto);

            // build customer 
            if (request.UpdateOption == null || request.UpdateOption.Customer)
            {
                if (request.NewDto.Customer == null)
                    request.NewDto.Customer = new CustomerDto();
                _builderCustomer.BuildForSaveFromSalesLead(isNew || request.IsVerify, request.NewDto.Customer, request.NewDto);
            }
        }

        /// <summary>
        /// save customer after saving saleslead
        /// </summary>
        /// <param name="isNew">insert or update</param>
        /// <param name="customerCode">customerCode</param>
        /// <param name="request">UpdateSalesLeadRequest</param>
        /// <returns></returns>
        private async Task SaveSaleLeadCustomerAsync(bool isNew, string customerCode, SalesLeadUpdateRequest request)
        {
            if ((request.UpdateOption == null || request.UpdateOption.Customer) && request.NewDto.Customer != null)
            {
                await SaveSaleLeadCustomerAsync(isNew, customerCode, request.NewDto.Customer);
            }
        }

        /// <summary>
        /// save customer
        /// </summary>
        /// <param name="isNew">insert or update</param>
        /// <param name="customerCode">customerCode</param>
        /// <param name="customerDto">CustomerDto</param>
        /// <returns></returns>
        private async Task SaveSaleLeadCustomerAsync(bool isNew, string customerCode, CustomerDto customerDto)
        {
            var customer = _mapper.Map<Customer>(customerDto);
            await _repository.SaveSaleLeadCustomerAsync(customerCode, customer, isNew);

        }
    }
}
