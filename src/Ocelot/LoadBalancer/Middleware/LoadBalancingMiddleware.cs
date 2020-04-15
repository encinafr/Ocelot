﻿namespace Ocelot.LoadBalancer.Middleware
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System;
    using System.Threading.Tasks;

    public class LoadBalancingMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoadBalancerHouse _loadBalancerHouse;

        public LoadBalancingMiddleware(RequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            ILoadBalancerHouse loadBalancerHouse)
                : base(loggerFactory.CreateLogger<LoadBalancingMiddleware>())
        {
            _next = next;
            _loadBalancerHouse = loadBalancerHouse;
        }

        public async Task Invoke(HttpContext httpContext, IDownstreamContext downstreamContext)
        {

            var loadBalancer = _loadBalancerHouse.Get(downstreamContext.DownstreamReRoute, downstreamContext.Configuration.ServiceProviderConfiguration);

            if (loadBalancer.IsError)
            {
                Logger.LogDebug("there was an error retriving the loadbalancer, setting pipeline error");
                SetPipelineError(downstreamContext, loadBalancer.Errors);
                return;
            }

            var hostAndPort = await loadBalancer.Data.Lease(downstreamContext, httpContext);
            if (hostAndPort.IsError)
            {
                Logger.LogDebug("there was an error leasing the loadbalancer, setting pipeline error");
                SetPipelineError(downstreamContext, hostAndPort.Errors);
                return;
            }

            //todo check downstreamRequest is ok
            downstreamContext.DownstreamRequest.Host = hostAndPort.Data.DownstreamHost;

            if (hostAndPort.Data.DownstreamPort > 0)
            {
                downstreamContext.DownstreamRequest.Port = hostAndPort.Data.DownstreamPort;
            }

            if (!string.IsNullOrEmpty(hostAndPort.Data.Scheme))
            {
                downstreamContext.DownstreamRequest.Scheme = hostAndPort.Data.Scheme;
            }

            try
            {
                await _next.Invoke(httpContext);
            }
            catch (Exception)
            {
                Logger.LogDebug("Exception calling next middleware, exception will be thrown to global handler");
                throw;
            }
            finally
            {
                loadBalancer.Data.Release(hostAndPort.Data);
            }
        }
    }
}
