using BasicAPI.Features.Commands;
using BasicAPI.Features.Queries;
using BasicAPI.Models;
using CQRSToolkit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient(typeof(IQueryHandler<GetDudeQuery, Dude>), typeof(GetDudeQueryHandler));
builder.Services.AddTransient(typeof(ICommandHandler<SetDudeCommand>), typeof(SetDudeCommandHandler));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

