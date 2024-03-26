using Yriclium.LlmApi.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Yriclium.LlmApi.Controllers {
    [ApiController]
    [Route("auth")]
    public class LlmAuthController : ControllerBase {
        private readonly HttpContext    context;
        private readonly bool           useExternalKey;
        public LlmAuthController(IHttpContextAccessor context, IConfiguration configuration) {
            if (context.HttpContext == null)
                throw new Exception("Something went wrong creating the ApiController");
            this.context = context.HttpContext;
            this.context.Response.Headers.AccessControlAllowOrigin = "*";
            useExternalKey = configuration.GetValue<bool?>("ExternalAuth") ?? false;
        }
        
        [HttpGet("key")]    //This key will be valid for 2 hours due to latency mitigations
        public string? Key(
            [FromQuery]    string          key,
            [FromServices] APIKeyValidator validator,
            [FromServices] ApiKeyStore     keyStore
        ) => validator.WithApiKey(context, key, useExternalKey ? keyStore.GetApiKey() : "");
    }
}