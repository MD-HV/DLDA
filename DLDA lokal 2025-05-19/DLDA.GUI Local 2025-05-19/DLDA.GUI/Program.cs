using DLDA.GUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Lägg till sessionshantering
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrera namngiven HttpClient för API-kommunikation
builder.Services.AddHttpClient("DLDA", client =>
{
    client.BaseAddress = new Uri("https://localhost:7166/api/");
});

// Registrera alla services som använder IHttpClientFactory
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<QuestionAdminService>();
builder.Services.AddScoped<UserAdminService>();
builder.Services.AddScoped<PatientAssessmentService>();
builder.Services.AddScoped<PatientQuizService>();
builder.Services.AddScoped<PatientResultService>();
builder.Services.AddScoped<PatientStatisticsService>();
builder.Services.AddScoped<StaffAssessmentService>();
builder.Services.AddScoped<StaffQuizService>();
builder.Services.AddScoped<StaffResultService>();
builder.Services.AddScoped<StaffStatisticsService>();

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
app.UseSession(); // aktiverar sessioner

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
