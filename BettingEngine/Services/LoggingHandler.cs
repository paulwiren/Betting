using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BettingEngine.Services
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger = Log.ForContext<LoggingHandler>();

        public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Log Request Headers
            _logger.Information("HttpClient Request: {Method} {Url}", request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                _logger.Information("Request Header: {Key}: {Value}", header.Key, string.Join(", ", header.Value));
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Log Response Headers
           // _logger.Information("HttpClient Response: {StatusCode} from {Url}", response.StatusCode, request.RequestMessage?.RequestUri);
            //foreach (var header in response.Headers)
            //{
            //    _logger.Information("Response Header: {Key}: {Value}", header.Key, string.Join(", ", header.Value));
                
            //}

            return response;
        }
    }
}
