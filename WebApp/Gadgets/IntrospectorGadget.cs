using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using InspectorGadget.WebApp.Controllers;
using InspectorGadget.WebApp.Infrastructure;
using InspectorGadget.WebApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InspectorGadget.WebApp.Gadgets
{
    public class IntrospectorGadget : GadgetBase<IntrospectorGadget.Request, IntrospectorGadget.Result>
    {
        public class Request : GadgetRequest
        {
            public string Group { get; set; }
            public string Key { get; set; }
        }

        public class Result
        {
            public string ResponseBody { get; set; }
        }

        private readonly HttpRequest httpRequest;
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;

        public IntrospectorGadget(ILogger logger, IHttpClientFactory httpClientFactory, IUrlHelper url, HttpRequest httpRequest, IWebHostEnvironment environment, IConfiguration configuration, AppSettings appSettings)
            : base(logger, httpClientFactory, url.GetRelativeApiUrl(nameof(ApiController.Introspector)), appSettings.DisableIntrospector)
        {
            this.httpRequest = httpRequest;
            this.environment = environment;
            this.configuration = configuration;
        }

        protected override Task<Result> ExecuteCoreAsync(Request request)
        {
            this.Logger.LogInformation("Executing Introspector request for Group {Group} and Key {Key}", request.Group, request.Key);
            var info = InspectorInfo.Create(this.environment, this.configuration, this.httpRequest, false);
            var value = info.GetPart(request.Group, request.Key);
            var result = new Result();
            if (value is string)
            {
                result.ResponseBody = value.ToString();
            }
            else
            {
                result.ResponseBody = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            return Task.FromResult(result);
        }
    }
}