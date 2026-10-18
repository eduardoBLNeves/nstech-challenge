using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Nstech.OrderService.Api;
using Nstech.OrderService.Api.ExceptionHandling;
using Nstech.OrderService.Application.Auth;
using Nstech.OrderService.Application.Orders;
using Nstech.OrderService.Application.Products;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Infrastructure;
using Nstech.OrderService.Infrastructure.Auth;
using Nstech.OrderService.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing!");
if (jwt.Key.Length < 32 || string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
    throw new InvalidOperationException("Jwt key is invalid!");

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var message = context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error));

            var response = ApiResponse<object>.Fail(message ?? Messages.ValidationError, StatusCodes.Status400BadRequest);
            return new BadRequestObjectResult(response);
        };
    });

#region DI
builder.Services.AddScoped<OrderAppService>();
builder.Services.AddScoped<ProductAppService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddInfrastructure(builder.Configuration);
#endregion

builder.Services.AddSingleton<ApiExceptionHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Service",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

await DatabaseInitializer.Apply(app.Services);

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is null)
            return;

        var handler = context.RequestServices.GetRequiredService<ApiExceptionHandler>();
        await handler.TryHandleAsync(context, exception, context.RequestAborted);
    }
});
app.UseSwagger();
app.UseSwaggerUI(options => options.DisplayRequestDuration());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
