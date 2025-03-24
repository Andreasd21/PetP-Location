using PetP_Location.Model;
using PetP_Location.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<InfluxDbService>();

builder.Services.Configure<InfluxDbSettings>(settings =>
{
    settings.Token = builder.Configuration["Influxdb_Token"] ??  "nawl0Ot1SfHhY_0P8q4AIojN_UqE_cBHr432mG-C-7N6PmzbX_PQ5Vr5brMbe8leinQIGZ1osl8dH4V-rJThBg==";
    settings.IP = builder.Configuration["Influxdb_IP"] ?? "localhost:8086";
});

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
