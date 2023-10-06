using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Rest.Configurations;
using Rest.Repositories;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ProductDBSettings>(builder.Configuration.GetSection("ProductDatabase"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<ITrainService, TrainService>();
builder.Services.AddSingleton<IScheduleService, ScheduleService>();
builder.Services.AddSingleton<IReservationService, ReservationService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = key,
    };
});

// Cors

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin() // Replace with the allowed origin(s)
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});



var app = builder.Build();

// Middleware for access control based on userType, status, and isActive status
/*app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity.IsAuthenticated)
    {
        var userTypeClaim = user.FindFirst(ClaimTypes.Name);
        if (userTypeClaim != null)
        {
            string userType = userTypeClaim.Value;
            // Check the user's userType and make access control decisions.

            if (userType == "Admin")
            {
                // Allow access for Admin users.
                await next();
                return;
            }
            else if (userType == "Standard")
            {
                // Handle access control for Standard users.
                // You can implement additional checks here.
            }
            // Add more conditions as needed for different user types.
        }
    }

    // If access is denied, return an appropriate response or redirect.
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsync("Access denied.");
});*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowAll");


app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
