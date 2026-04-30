using Job.Marketplace.API.Features.Contractors.Search;
using Job.Marketplace.API.Features.Customers.GetById;
using Job.Marketplace.API.Features.Customers.Search;
using Job.Marketplace.API.Features.JobOffers.Accept;
using Job.Marketplace.API.Features.JobOffers.Create;
using Job.Marketplace.API.Features.Jobs.Create;
using Job.Marketplace.API.Features.Jobs.Delete;
using Job.Marketplace.API.Features.Jobs.GetById;
using Job.Marketplace.API.Features.Jobs.Update;
using Job.Marketplace.Infrastructure;
using Job.Marketplace.Infrastructure.Migrations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ISearchCustomersQueries, SearchCustomersQueries>();
builder.Services.AddScoped<SearchCustomersHandler>();
builder.Services.AddScoped<IGetCustomerByIdQueries, GetCustomerByIdQueries>();
builder.Services.AddScoped<GetCustomerByIdHandler>();
builder.Services.AddScoped<ISearchContractorsQueries, SearchContractorsQueries>();
builder.Services.AddScoped<SearchContractorsHandler>();
builder.Services.AddScoped<ICreateJobQueries, CreateJobQueries>();
builder.Services.AddScoped<CreateJobHandler>();
builder.Services.AddScoped<IGetJobByIdQueries, GetJobByIdQueries>();
builder.Services.AddScoped<GetJobByIdHandler>();
builder.Services.AddScoped<IUpdateJobQueries, UpdateJobQueries>();
builder.Services.AddScoped<UpdateJobHandler>();
builder.Services.AddScoped<IDeleteJobQueries, DeleteJobQueries>();
builder.Services.AddScoped<DeleteJobHandler>();
builder.Services.AddScoped<ICreateJobOfferQueries, CreateJobOfferQueries>();
builder.Services.AddScoped<CreateJobOfferHandler>();
builder.Services.AddScoped<IAcceptJobOfferQueries, AcceptJobOfferQueries>();
builder.Services.AddScoped<AcceptJobOfferHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

DbMigrator.Run(app.Configuration.GetConnectionString("Marketplace")!);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

SearchCustomersEndpoint.Map(app);
GetCustomerByIdEndpoint.Map(app);
SearchContractorsEndpoint.Map(app);
CreateJobEndpoint.Map(app);
GetJobByIdEndpoint.Map(app);
UpdateJobEndpoint.Map(app);
DeleteJobEndpoint.Map(app);
CreateJobOfferEndpoint.Map(app);
AcceptJobOfferEndpoint.Map(app);

app.Run();

public partial class Program { }
