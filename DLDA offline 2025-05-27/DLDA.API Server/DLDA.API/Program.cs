using DLDA.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ========================
// ? Tjänsteregistrering
// ========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DLDA API",
        Version = "v1"
    });
});

// ========================
// ? DbContext
// ========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// ========================
// ? CORS-policy
// ========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGUI", policy =>
    {
        policy.WithOrigins("https://informatik3.ei.hv.se")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ========================
// ? Middleware
// ========================
app.UseHttpsRedirection();
app.UseCors("AllowGUI");
app.UseAuthorization();
app.MapControllers();

// ========================
// ? Swagger-konfiguration (fungerar på server med subpath)
// ========================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // OBS: Absolut URL till swagger.json – inte relativ!
    c.SwaggerEndpoint("/DLDA.API/swagger/v1/swagger.json", "DLDA API v1");
    c.RoutePrefix = "swagger"; // så /DLDA.API/swagger visar UI:t
});


app.Run();
