using Serilog.Events;
using Serilog;
using Serilog.Core;
using MDRCloudServices.Services.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Ganss.XSS;
using MDRCloudServices.Interfaces;
using MDRDB.Recordsets;
using Microsoft.AspNetCore.Mvc.Formatters;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using MDRCloudServices.Api.Models;
using MDRCloudServices.DataLayer.Models;
using Microsoft.OpenApi.Models;
using MDRCloudServices.Api.Filters;
using Microsoft.AspNetCore.Http.Features;
using MDRCloudServices.OgrEnvironmentalDataRetrieval;
using MDRCloudServices.Services;
using MediatR;
using MDRCloudServices.Services.Interfaces;
using MDRCloudServices.Services.Services;
using NPoco;
using MDRCloudServices.Helpers;
using Swashbuckle.AspNetCore.SwaggerUI;

static Database GetDatabase(IConfiguration configuration)
{
    if (configuration.GetSection(nameof(AppOptions)).Get<AppOptions>().Database == "postgres")
    {
        return new PgDatabseWithLogging(configuration.GetConnectionString(SecretKeys.Database), SqlHelper.GetNPocoPollyPolicy());
    }
    return new SqlServerDatabaseWithLogging(configuration.GetConnectionString(SecretKeys.Database), SqlHelper.GetNPocoPollyPolicy());
}

Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
   .Enrich.FromLogContext()
   .WriteTo.Console()
   .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((hostContext, services, loggingConfiguration) =>
    {
        var levelSwitch = new LoggingLevelSwitch();
        loggingConfiguration
            .ReadFrom.Configuration(hostContext.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Seq(hostContext.Configuration["Seq:ServerUrl"],
                apiKey: hostContext.Configuration["Seq:ApiKey"],
                controlLevelSwitch: levelSwitch)
            .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces);

        if (hostContext.Configuration.GetSection(nameof(AppOptions)).Exists())
        {
            string buildNumber = hostContext.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>().BuildNumber ?? "Developer Version";
            if (!string.IsNullOrWhiteSpace(buildNumber))
                loggingConfiguration.Enrich.WithProperty(nameof(AppOptions.BuildNumber), buildNumber);
        }
    });

    builder.Services.AddResponseCompression();
    builder.Services.AddCors(o =>
    {
        o.AddDefaultPolicy(p =>
        {
            p.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
        });
    });

    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddControllers(o =>
    {
        o.Conventions.Add(new ApiExplorerGroupByNamespace());
        o.RespectBrowserAcceptHeader = true;
        o.OutputFormatters.Insert(0, new XmlSerializerOutputFormatter());
    })
    .AddXmlDataContractSerializerFormatters()
    .AddJsonOptions(options =>
    {
        var geoJsonConverterFactory = new GeoJsonConverterFactory();
        options.JsonSerializerOptions.Converters.Add(geoJsonConverterFactory);
    })
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.Converters.Add(new ConcreteConverter<IField, Field>());
        o.SerializerSettings.Converters.Add(new ConcreteConverter<IFilterList, FilterList>());
        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        o.SerializerSettings.Error = (object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        {
            Log.Error("Json serialization errors {Errors}", args.ErrorContext.Error.Message);
        };
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v2", new OpenApiInfo
        {
            Title = "Cefas Data Portal API",
            Description = "<p>API for handling the complete dataset and data life-cycle through " +
            "the Cefas Data Portal.</p>" +
            "<p><a href=\"https://data.cefas.co.uk\">Cefas Data Portal</a> &nbsp; &nbsp; " +
            "<a href=\"https://www.cefas.co.uk/data-and-publications/cefas-data-hub-terms-and-conditions/\">" +
            "Terms and Conditions</a> &nbsp; &nbsp; " +
            "<a href=\"https://www.cefas.co.uk/about-us/policies-plans-reports-and-quality/policies/cookies-policy/\">" +
            "Cookies Policy</a></p>",
            Contact = new OpenApiContact
            {
                Name = "Cefas Data Manager",
                Email = builder.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>().AdminEmailAddress
            }
        });

        c.SwaggerDoc("ogc", new OpenApiInfo
        {
            Title = "OGC Environmental Data Retrieval API",
            Description = "<p>An Environmental Data Retrieval (EDR) API provides a family of lightweight interfaces " +
                "to access Environmental Data resources, as per the standard defined by the Open Geospatial " +
                "Consortium. Only a limited number of Cefas recordsets are available via this API.</p>" +
                "<p><a href=\"https://www.ogc.org/standards/ogcapi-edr\">OGC Environmental Data Retrieval Standard</a> &nbsp; &nbsp; " +
                "<a href=\"https://www.cefas.co.uk/data-and-publications/cefas-data-hub-terms-and-conditions/\">" +
                "Terms and Conditions</a> &nbsp; &nbsp; " +
                "<a href=\"https://www.cefas.co.uk/about-us/policies-plans-reports-and-quality/policies/cookies-policy/\">" +
                "Cookies Policy</a></p>",
            Contact = new OpenApiContact
            {
                Name = "Cefas Data Manager",
                Email = builder.Configuration.GetSection(nameof(AppOptions)).Get<AppOptions>().AdminEmailAddress
            }
        });

        var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
        xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile, true));

        c.CustomSchemaIds(x => x.FullName);
        c.EnableAnnotations();
        // Override defaults so that JSON is the default type in the UI.
        c.OperationFilter<AssignContentTypeFilter>();
    });

    builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(nameof(AppOptions)));
    builder.Services.Configure<FormOptions>(x =>
    {
        x.ValueLengthLimit = int.MaxValue;
        x.MultipartBodyLengthLimit = int.MaxValue;
        x.MultipartHeadersLengthLimit = int.MaxValue;
    });

    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString(SecretKeys.Database))
        .AddAzureBlobStorage(builder.Configuration.GetConnectionString(SecretKeys.BlobStorage))
        .AddApplicationInsightsPublisher()
        .AddSeqPublisher(x =>
        {
            x.Endpoint = builder.Configuration["Seq:ServerUrl"];
            x.ApiKey = builder.Configuration["Seq:ApiKey"];
        });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IDatabase, Database>(s => GetDatabase(builder.Configuration));
    builder.Services.AddTransient<IRecordsetService, RecordsetService>();
    builder.Services.AddTransient<IRecordsetTableService, RecordsetTableService>();
    builder.Services.AddTransient<IStorageDatabaseService, StorageDatabaseService>();
    builder.Services.AddTransient<IVocabularyService, VocabularyService>();
    builder.Services.AddMediatR(
        typeof(OgrEnvironmentalDataRetrievalAssembly).Assembly,
        typeof(ServicesAssembly).Assembly);

    var app = builder.Build();

    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();

    app.UseResponseCompression();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseCors();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Cefas Data Portal API");
        c.DocumentTitle = "Cefas Data APIs";
        c.SwaggerEndpoint("/swagger/ogc/swagger.json", "Cefas OGC Environmental Data Retrieval API");
        c.DocExpansion(DocExpansion.None);
        c.EnableDeepLinking();
        c.RoutePrefix = string.Empty;
        c.InjectStylesheet("custom.css");
        c.ConfigObject.AdditionalItems.Add("queryConfigEnabled", true);
    });

    app.UseStaticFiles();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
    });

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
