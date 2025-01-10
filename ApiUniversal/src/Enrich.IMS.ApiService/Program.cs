using Autofac;
using Autofac.Extensions.DependencyInjection;
using Enrich.Core.Infrastructure;
using Enrich.Dto.Base;
using Enrich.IMS.RestApi.Library.Filters;
using Enrich.IMS.RestApi.Library.Infrastructure;
using Enrich.Infrastructure.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;


//a variable to hold configuration
IConfiguration configuration;
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Directory.GetCurrentDirectory()
});
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.WebHost.UseKestrel(opts =>
{
    opts.ConfigureKestrelOptions();
}).UseIIS();

configuration = builder.Configuration;

// configure services
builder.Services.ConfigureApplicationServices();

builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    // register all services using autofac
    builder.ConfigureAutofacServices(configuration);
});

// configure logger
Log.Logger = SerilogHelper.CreateLogger(configuration.GetSection("Logs").Get<LogSetting>());

var app = builder.Build();

//Configure the application HTTP request pipeline
app.ConfigureRequestPipeline();

app.Run();

