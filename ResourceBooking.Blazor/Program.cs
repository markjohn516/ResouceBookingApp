using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ResourceBooking.Blazor;
using ResourceBooking.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7000") });
builder.Services.AddScoped<IApiClient, ApiClient>();

builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ApiClient>>();
    return new ApiClient(new HttpClient { BaseAddress = new Uri("https://localhost:7000") }, logger);
});

await builder.Build().RunAsync();
