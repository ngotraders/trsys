using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Trsys.Infrastructure;
using Trsys.Infrastructure.ReadModel.UserNotification;
using Trsys.Web.Formatters;
using Trsys.Web.Identity;
using Trsys.Web.Middlewares;
using Trsys.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddTrsysIdentity();
builder.Services.AddControllers(options =>
{
    options.InputFormatters.Add(new TextPlainInputFormatter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"), null);
builder.Services.AddEmailSender(builder.Configuration.GetSection("Trsys.Web:EmailSenderConfiguration").Get<EmailSenderConfiguration>());
builder.Services.AddDbContext<TrsysContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(options =>
    {
        options.AllowAnyHeader();
        options.AllowAnyMethod();
        options.SetIsOriginAllowed(origin => true);
        options.AllowCredentials();
    });
}

app.UseHttpsRedirection();
app.UseInitialization();
app.MapIdentityApi<TrsysUser>();
app.MapControllers().WithOpenApi();

app.Run();
