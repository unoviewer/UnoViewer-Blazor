using UnoViewer_Blazor.Components;
using Uno.Files.Options.Viewer;
using Uno.Files.Viewer.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Required UnoViewer Start

builder.Services.AddHttpContextAccessor();     
builder.Services.AddMemoryCache();          
builder.Services.AddSingleton<ViewerService>(); 

// Required UnoViewer End

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Register UnoViewer Start
app.MapWhen(context => context.Request.Path.ToString().EndsWith("UnoImage.axd"),
    appBranch =>
    {
        appBranch.UseUnoViewer(new UnoViewOptions { UnSafe = false, ShowInfo = false });
    }
);
// Register UnoViewer End

app.Run();
