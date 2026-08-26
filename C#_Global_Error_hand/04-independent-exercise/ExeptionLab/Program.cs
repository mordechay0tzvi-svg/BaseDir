using TaskManeger.Repositories;
using TaskManeger.ErrorHandling;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<TaskManeger.ErrorHandling.GlobalExeptionHnadler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<ITaskManeger, TaskManegerRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
