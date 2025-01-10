using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;

namespace Enrich.IMS.RestApi.Library.Infrastructure
{
    public static class KestrelOptionExtensions
    {
        public static void ConfigureKestrelOptions(this KestrelServerOptions opts)
        {
            opts.AddServerHeader = false;

            opts.ConfigureEndpointDefaults(x => x.Protocols = HttpProtocols.Http1AndHttp2);

            // Limits
            //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits.minrequestbodydatarate?view=aspnetcore-2.2#Microsoft_AspNetCore_Server_Kestrel_Core_KestrelServerLimits_MinRequestBodyDataRate
            opts.Limits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(30));
            opts.Limits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(30));

            opts.Limits.MaxConcurrentConnections = null;
            opts.Limits.MaxConcurrentUpgradedConnections = null;
            opts.Limits.MaxRequestBufferSize = null;
            opts.Limits.MaxRequestHeaderCount = 4096;
            opts.Limits.MaxRequestHeadersTotalSize = 32768 * opts.Limits.MaxRequestHeaderCount;

            // <see cref="https://stackoverflow.com/a/47112438"/> 
            opts.Limits.MaxRequestBodySize = null;

            // <see cref="https://stackoverflow.com/a/47809150"/>
            opts.Limits.KeepAliveTimeout = TimeSpan.FromHours(1);

            opts.Limits.RequestHeadersTimeout = TimeSpan.FromHours(1);
            opts.Limits.MaxResponseBufferSize = null;
        }
    }
}
