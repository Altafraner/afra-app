using System.Text.Json.Serialization;
using Afra_App;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AfraAppContext>();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.DefaultPolicyName = "default";
    options.AddPolicy("default", corsPolicyBuilder => corsPolicyBuilder.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();