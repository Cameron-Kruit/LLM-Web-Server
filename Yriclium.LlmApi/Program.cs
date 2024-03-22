using Yriclium.LlmApi.Middleware;
using Yriclium.LlmApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80", "https://0.0.0.0:443");
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddSingleton<StatelessChatService>()
    .AddSingleton<ApiKeyStore>()
    .AddSingleton<JobService>()
    .AddSingleton<ConnectionStoreService>()
    .AddSingleton<APIKeyValidator>()
    .AddSingleton<IHookService, ConfigurableWebHookService>()
    .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

Console.WriteLine("Selected environment: " + app.Environment.EnvironmentName);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    Console.WriteLine("🚧 DEVELOPMENT BACKEND SERVER STARTED 🚧");
    app
        .UseSwagger()
        .UseSwaggerUI(options => {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });
}

// Lock all API and websocket endpoints behind the api key.
app.UseWhen(context => 
    context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/ws"), 
    appBuilder => {
        appBuilder.UseMiddleware<ApiKeyMiddleware>();
});

app
    .UseAuthorization()
    .UseWebSockets();

app.MapControllers();
app.Run();
