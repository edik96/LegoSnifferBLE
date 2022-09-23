using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WebUIVisualizerLEGO.Data;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".obj"] = "text/plain";
provider.Mappings[".mtl"] = "text/plain";
provider.Mappings[".jpg"] = "image/jpeg";

builder.Services.Configure<StaticFileOptions>(options =>
{
    options.ContentTypeProvider = provider;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


builder.WebHost.UseUrls("http://*:5000", "http://localhost:5000");
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
