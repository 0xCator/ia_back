using ia_back.Data;
using ia_back.Data.Custom_Repositories;
using ia_back.Models;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING"));
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDataRepository<User>, UserRepository>();
builder.Services.AddScoped<IDataRepository<Project>, ProjectRepository>();
builder.Services.AddScoped<IDataRepository<ProjectTask>, TaskRepository>();
builder.Services.AddScoped<IDataRepository<Comment>, CommentRepository>();
builder.Services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
