using Rest.Configurations;
using Rest.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ProductDBSettings>(builder.Configuration.GetSection("ProductDatabase"));
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<ITrainService, TrainService>();
builder.Services.AddSingleton<IScheduleService, ScheduleService>();
builder.Services.AddSingleton<IReservationService, ReservationService>();
builder.Services.AddControllers(); 
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
