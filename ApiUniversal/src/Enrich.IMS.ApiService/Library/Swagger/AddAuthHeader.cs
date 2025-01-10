using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Enrich.IMS.RestApi.Library.Swagger
{
    public class AddAuthHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();


            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                //Type = "string",
                Required = true,
                Description = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                //Type = "string",
                Required = true,
                Description = "EN",
            });           
        }
    }
}
