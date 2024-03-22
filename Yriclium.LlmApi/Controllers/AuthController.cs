using Yriclium.LlmApi.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Yriclium.LlmApi.Controllers {
    [ApiController]
    [Route("auth")]
    public class LlmAuthController : ControllerBase {
        private readonly HttpContext    context;
        public LlmAuthController(IHttpContextAccessor context) {
            if (context.HttpContext == null)
                throw new Exception("Something went wrong creating the ApiController");
            this.context = context.HttpContext;
            this.context.Response.Headers.AccessControlAllowOrigin = "*";
        }
        
        [HttpGet("key")]
        public string? Key(
            [FromQuery]    string          key,
            [FromServices] APIKeyValidator validator,
            [FromServices] ApiKeyStore     keyStore
        ) => validator.WithApiKey(context, key, keyStore.GetApiKey());
    }
}