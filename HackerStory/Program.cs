using Microsoft.Extensions.Caching.Memory;
using StoryLibrary;

using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

var startup = new HackerStory.Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Initial Cache load
HackerNews hn = new HackerNews();
hn.LoadCache(true);

var cache = app.Services.GetRequiredService<IMemoryCache>();
cache.Set("Stories", hn.GetCache());

hn.StoriesRefreshed += Hn_StoriesRefreshed;
//Polling for new stories
hn.LoadCache(false);
void Hn_StoriesRefreshed(object? sender, EventArgs e)
{
    cache.Set("Stories", hn.GetCache());
}


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

