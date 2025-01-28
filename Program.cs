using MudBlazor.Services;
using ImageHunter.Components;
using ImageHunter.Services;
using ImageHunter.Services.VectorDatabase;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services.AddHttpClient("vision", options =>
{
    options.BaseAddress = new Uri("your-azure-vision-endpoint");
    options.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "");
});

builder.Services.AddScoped<IVisionService, VisionService>();
builder.Services.AddScoped<IVectorDatabaseService, VectorDatabaseService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
