using Example3.DTOs;
using Example3.Endpoints;
using Example3.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbServiceDapper, DbServiceDapper>();
builder.Services.AddValidatorsFromAssemblyContaining<PrescriptionDTO.CreatePrescription>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterGroupDapperEndpoint();

app.Run();
