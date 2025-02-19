using MudBlazor.Services;
using ImageHunter.Components;
using ImageHunter.Services;
using ImageHunter.Services.VectorDatabase;
using ChromaDB.Client;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services.AddHttpClient("vision", options =>
{
    options.BaseAddress = new Uri("");
    options.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "");
});

builder.Services.AddScoped<IVisionService, VisionService>();
builder.Services.AddScoped<IVectorDatabaseService, VectorDatabaseService>();

var configOptions = new ChromaConfigurationOptions(uri: "http://localhost:8000/api/v1/");
using var httpClient = new HttpClient();
var chroma = new ChromaClient(configOptions, httpClient);

var collection = await chroma.GetOrCreateCollection("images", metadata: new Dictionary<string, object>
{
    { "hnsw:space", "cosine" }
});
var chromaCollection = new ChromaCollectionClient(collection, configOptions, httpClient);

builder.Services.AddSingleton(chroma);
builder.Services.AddSingleton(chromaCollection);

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
