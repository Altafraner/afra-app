using System.Text.Json.Serialization;
using Afra_App;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
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

// TODO Add login service
builder.Services.AddAuthentication()
    .AddCookie();
builder.Services.AddAuthorization();

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