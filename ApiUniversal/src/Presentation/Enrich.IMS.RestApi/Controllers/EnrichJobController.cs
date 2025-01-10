using Enrich.IMS.Dto;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
	[EnrichRoute(EnrichRouteName.EnrichJob)]
	public class EnrichJobController : EnrichApiController
	{
		#region Fields
		private readonly IRecurringPlanningService _serviceRecurringPlanning;
		#endregion

		#region Ctors
		public EnrichJobController(IRecurringPlanningService serviceRecurringPlanning)
		{
			_serviceRecurringPlanning = serviceRecurringPlanning;
		}
		#endregion

		/// <summary>
		/// run recurring
		/// </summary>
		/// <returns></returns>
		[HttpPost("run-recurring")]
		public async Task<IActionResult> RunRecurringAsync()
		{
			await _serviceRecurringPlanning.RecurringSubscriptionAsync();
			return Ok();
		}

		/// <summary>
		/// run recurring by store
		/// </summary>
		/// <returns></returns>
		[HttpPost("run-recurring-by-customercode/{customerCode}")]
		public async Task<IActionResult> RunRecurringByStoreCodeAsync(string customerCode)
		{
			await _serviceRecurringPlanning.RecurringSubscriptionByCustomerCodeAsync(customerCode);
			return Ok();
		}
	}
}
