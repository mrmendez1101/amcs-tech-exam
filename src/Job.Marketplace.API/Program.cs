using Job.Marketplace.API.Features.Contractors.Search;
using Job.Marketplace.API.Features.Customers.GetById;
using Job.Marketplace.API.Features.Customers.Search;
using Job.Marketplace.API.Features.JobOffers.Accept;
using Job.Marketplace.API.Features.JobOffers.Create;
using Job.Marketplace.API.Features.JobOffers.Delete;
using Job.Marketplace.API.Features.JobOffers.GetByJobId;
using Job.Marketplace.API.Features.JobOffers.Update;
using Job.Marketplace.API.Features.Jobs.Create;
using Job.Marketplace.API.Features.Jobs.Delete;
using Job.Marketplace.API.Features.Jobs.GetById;
using Job.Marketplace.API.Features.Jobs.Search;
using Job.Marketplace.API.Features.Jobs.Update;
using Job.Marketplace.Infrastructure;
using Job.Marketplace.Infrastructure.Caching;
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
builder.Services.AddScoped<GetCustomerByIdQueries>();

// Cache Injection
builder.Services.AddSingleton(sp =>
    new LruCache<Guid, CustomerDetail>(
        sp.GetRequiredService<IConfiguration>().GetValue<int>("Cache:CustomerCapacity", 10_000)));
builder.Services.AddSingleton<CacheMetrics>();
builder.Services.AddScoped<IGetCustomerByIdQueries>(sp =>
    new CachedCustomerByIdLookup(
        sp.GetRequiredService<GetCustomerByIdQueries>(),
        sp.GetRequiredService<LruCache<Guid, CustomerDetail>>(),
        sp.GetRequiredService<CacheMetrics>()));

builder.Services.AddScoped<GetCustomerByIdHandler>();
builder.Services.AddScoped<ISearchContractorsQueries, SearchContractorsQueries>();
builder.Services.AddScoped<SearchContractorsHandler>();
builder.Services.AddScoped<ISearchJobsQueries, SearchJobsQueries>();
builder.Services.AddScoped<SearchJobsHandler>();
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
builder.Services.AddScoped<IGetJobOffersByJobIdQueries, GetJobOffersByJobIdQueries>();
builder.Services.AddScoped<GetJobOffersByJobIdHandler>();
builder.Services.AddScoped<IAcceptJobOfferQueries, AcceptJobOfferQueries>();
builder.Services.AddScoped<AcceptJobOfferHandler>();
builder.Services.AddScoped<IDeleteJobOfferQueries, DeleteJobOfferQueries>();
builder.Services.AddScoped<DeleteJobOfferHandler>();
builder.Services.AddScoped<IUpdateJobOfferQueries, UpdateJobOfferQueries>();
builder.Services.AddScoped<UpdateJobOfferHandler>();

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

var customers = app.MapGroup("/customers").WithTags("Customers");
SearchCustomersEndpoint.Map(customers);
GetCustomerByIdEndpoint.Map(customers);

var contractors = app.MapGroup("/contractors").WithTags("Contractors");
SearchContractorsEndpoint.Map(contractors);

var jobs = app.MapGroup("/jobs").WithTags("Jobs");
SearchJobsEndpoint.Map(jobs);
CreateJobEndpoint.Map(jobs);
GetJobByIdEndpoint.Map(jobs);
UpdateJobEndpoint.Map(jobs);
DeleteJobEndpoint.Map(jobs);

var jobOffers = app.MapGroup("/jobs/{jobId:guid}/offers").WithTags("Job Offers");
CreateJobOfferEndpoint.Map(jobOffers);
GetJobOffersByJobIdEndpoint.Map(jobOffers);
AcceptJobOfferEndpoint.Map(jobOffers);
DeleteJobOfferEndpoint.Map(jobOffers);
UpdateJobOfferEndpoint.Map(jobOffers);

app.MapGet("/internal/cache-metrics", (CacheMetrics m) =>
    Results.Ok(new { m.Hits, m.Misses, m.HitRatio }));

app.Run();

public partial class Program { }
