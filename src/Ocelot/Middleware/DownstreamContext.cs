using Ocelot.Configuration;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Errors;
using Ocelot.Request.Middleware;
using System.Collections.Generic;

namespace Ocelot.Middleware
{
    public class DownstreamContext : IDownstreamContext
    {
        public DownstreamContext()
        {
            Errors = new List<Error>();
        }

        public List<PlaceholderNameAndValue> TemplatePlaceholderNameAndValues { get; set; }

        public DownstreamReRoute DownstreamReRoute { get; set; }

        public DownstreamRequest DownstreamRequest { get; set; }

        public DownstreamResponse DownstreamResponse { get; set; }

        public List<Error> Errors { get; }

        public IInternalConfiguration Configuration { get; set; }

        public bool IsError => Errors.Count > 0;
    }

    public interface IDownstreamContext
    {
        public List<PlaceholderNameAndValue> TemplatePlaceholderNameAndValues { get; set; }

        public DownstreamReRoute DownstreamReRoute { get; set; }

        public DownstreamRequest DownstreamRequest { get; set; }

        public DownstreamResponse DownstreamResponse { get; set; }

        public List<Error> Errors { get; }

        public IInternalConfiguration Configuration { get; set; }

        public bool IsError => Errors.Count > 0;
    }
}
