using Enrich.DataTransfer.EnrichUniversal.NewCustomerGoal;
using Enrich.IServices;
using Enrich.IServices.Utils.EnrichUniversal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class NewCustomerGoalController : Controller
    {
        private readonly ILogService _logService;
        private readonly INewCustomerGoalService _newCustomerGoalService;

        public NewCustomerGoalController(ILogService logService, INewCustomerGoalService newCustomerGoalService = null)
        {
            _logService = logService;
            _newCustomerGoalService = newCustomerGoalService;
        }

        private async Task<NewCustomerGoalResponseDto> GetNewCustomerGoal(int Year)
        {
            var request = new NewCustomerGoalRequestDto();
            request.PageIndex = 1;
            request.PageSize = 12;
            request.Condition.Year = Year;
            request.Fields = "*";
            request.ViewMode = "Grid";
            request.OrderBy = "-CreateAt";  // asc => "CreateAt" || desc => "-CreateAt"
            var searchResponse = await _newCustomerGoalService.GetNewCustomerGoalList(request);
            return searchResponse;
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Load list from datatable ajax
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        public async Task<ActionResult> LoadListCustomerGoal(int? Year)
        {
            try
            {
                _logService.Info($"[NewCustomerGoal][LoadListNewCustomerGoal] start load new customer goal with Year {Year}");
                // Session.Remove(MERCHANT_LIST);
                var response = await this.GetNewCustomerGoal(Year??DateTime.UtcNow.Year);
                var result = new List<NewCustomerGoalDto>();
                //each parse data month
                for(var i = 1; i <= 12; i++)
                {
                    var currentGoalMonth = response.Records.FirstOrDefault(x => x.Month == i &&x.Year == Year);
                    if (currentGoalMonth != null)
                    {
                        result.Add(currentGoalMonth);
                    }
                    else
                    {
                        result.Add(new NewCustomerGoalDto
                        {
                            Month = i,
                            Year = Year.Value,
                            Goal = 0
                        });
                    }
                }
                _logService.Info($"[NewCustomerGoal][LoadListNewCustomerGoal] completed");
                return Json(new
                {
                    recordsFiltered = response.Pagination.TotalRecords,
                    recordsTotal = response.Pagination.TotalRecords,
                    data = result,
                });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[NewCustomerGoal][LoadListNewCustomerGoal] error load new customer goal with Year {Year}");

                throw;
            }
        }
        /// <summary>
        /// create or update goal new customer
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <param name="Goal"></param>
        /// <returns></returns>
        public async Task<ActionResult> UpdateOrCreateGoal(int? Id,int? Year,int? Month,int? Goal)
        {
            try
            {
                string messageNoty = "";
                if (Id > 0)
                {
                    var requestUpdate = new NewCustomerGoalUpdateRequest();
                    requestUpdate.op = "replace";
                    requestUpdate.path = "/Goal";
                    requestUpdate.value = Goal;
                    var request = new List<NewCustomerGoalUpdateRequest>();
                    request.Add(requestUpdate);
                    await _newCustomerGoalService.PatchUpdate(request, Id);
                    messageNoty = "Update new customer goal success";
                }
                else
                {
                    var request = new NewCustomerGoalDto();
                    request.Year = Year;
                    request.Month = Month;
                    request.Goal = Goal;
                    await _newCustomerGoalService.Create(request);
                    messageNoty = "Create new customer goal success";
                }
                return Json(new { status = true, message= messageNoty });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = "Update error: "+ex.Message });
            }
          
        }
    }
}