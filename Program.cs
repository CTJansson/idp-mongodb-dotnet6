using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

string _databaseUri = "mongodb://localhost:27017";
string _database = "accounts";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddIdentityMongoDbProvider<MongoUser, MongoRole>(identity =>
{
    identity.Password.RequiredLength = 5;
},
mongo => 
{
    mongo.ConnectionString = _databaseUri;
});

// TODO: Find the correct properties for this
// builder.Services.Configure<IdentityOptions>(options =>
// {
//     options.ClaimsIdentity.UserNameClaimType = OpenIddict.Claims.Name;
//     options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
//     options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
// });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services
    .AddOpenIddict()
    .AddCore(options =>
    {
        options.UseMongoDb()
            .UseDatabase(new MongoClient(_databaseUri)
            .GetDatabase(_database));
    }).AddServer(options =>
    {
        options.SetAccessTokenLifetime(TimeSpan.FromDays(5));
    
        options.SetTokenEndpointUris("/api/token");

        options.SetUserinfoEndpointUris("/api/userinfo");
    
        options.AllowPasswordFlow()
            .AllowRefreshTokenFlow();
    
        options.AcceptAnonymousClients();
    }).AddValidation();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
