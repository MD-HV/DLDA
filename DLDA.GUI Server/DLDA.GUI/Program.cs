var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ✅ Lägg till session
builder.Services.AddDistributedMemoryCache(); // (krävs för session)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // valfri timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrera HttpClient med rätt bas-URL till ditt DLDA.API
builder.Services.AddHttpClient("DLDA", client =>
{
    client.BaseAddress = new Uri("https://informatik3.ei.hv.se/api/");
});

var app = builder.Build();

// Middleware-pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.UseSession(); // ✅ Aktivera sessionshantering

// Login route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
