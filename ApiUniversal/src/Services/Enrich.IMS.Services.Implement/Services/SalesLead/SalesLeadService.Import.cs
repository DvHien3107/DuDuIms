using Enrich.Common.Enums;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Implement.Validation;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadService
    {
        public async Task<ImportSalesLeadResponse> ImportSalesLeadAsync(ImportSalesLeadRequest request)
        {
            var response = new ImportSalesLeadResponse();
            var saleleads = new List<SalesLead>();
#if DEBUG
            // example data, need to code
            response.TotalSucceedRows = 0;
            response.TotalFailRows = 0;

#endif
            try
            {
                if (request.ExcelContent != null)
                {
                    IWorkbook workbook = new XSSFWorkbook(request.ExcelContent);
                    ISheet sheet = workbook.GetSheetAt(0);
                    int rows = sheet.LastRowNum - 8; //8 is format file
                    int cols = sheet.GetRow(1).LastCellNum;
                    var rowList = Enumerable.Range(9, rows).ToList();

                    foreach (var item in rowList)
                    {
                        try
                        {
                            var salelead = new SalesLeadDto();
                            await _builder.BuildForImport(salelead);
                            await PopulateImportData(salelead, sheet.GetRow(item));
                            await ValidateSaveSalesLeadAsync(true, salelead);

                            var entity = _mapper.Map<SalesLead>(salelead);
                            //await AddAsync(salelead);
                            saleleads.Add(entity);
                            response.TotalSucceedRows++;
                        }
                        catch (EnrichException ex)
                        {
                            var msgException = (List<EnrichValidationFailure>)ex.ExtendData;
                            if (msgException != null)
                            {
                                response.ErrorResults.Add(new ImportSalesLeadResponse.ImportResult()
                                {
                                    RowIndex = item + 1,
                                    Message = msgException.FirstOrDefault()?.Message,
                                });
                            }
                            response.TotalFailRows++;
                        }
                    }

                    await _repository.BulkInsertAsync(saleleads);
                }
            }

            catch (Exception ex)
            {
                response.Status = ProcessStatus.Failed.ToString();
                response.ErrorMessage = ex.Message;
                _log.Error(ex, $"ImportSalesLead: {JsonConvert.SerializeObject(response)}");

                return response;
            }
            return response;
        }
    }
}