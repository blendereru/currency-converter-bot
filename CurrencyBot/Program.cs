var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient();
var app = builder.Build();
app.MapControllers();
app.Run();