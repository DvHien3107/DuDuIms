using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;

namespace Enrich.IMS.RestApi.Library.Filters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class EnrichNotNullAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var names = GetCheckNullParamInfos(actionContext);
            if (names == null || names.Length == 0)
            {
                return;
            }

            var nullTypeNames = names.Where(a => !actionContext.ActionArguments.ContainsKey(a.Name)).ToArray();
            if (nullTypeNames.Length > 0)
            {
                actionContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                actionContext.Result = new JsonResult(new
                {
                    code = actionContext.HttpContext.Response.StatusCode,
                    msg = $"Request is null: {string.Join(", ", nullTypeNames.Select(a => a.TypeName).Distinct())}"
                });
            }
        }

        private (string Name, string TypeName)[] GetCheckNullParamInfos(ActionExecutingContext actionContext)
        {
            if (!actionContext.Filters.Any(a => a is EnrichNotNullAttribute) || actionContext.ActionDescriptor.Parameters.Count == 0)
            {
                return null;
            }

            return actionContext.ActionDescriptor.Parameters.OfType<ControllerParameterDescriptor>()
                .Where(a => a.ParameterInfo.GetCustomAttributes(typeof(EnrichNotNullAttribute), false).Length > 0)
                .Select(a => (a.Name, a.ParameterType.Name))
                .ToArray();
        }
    }
}
