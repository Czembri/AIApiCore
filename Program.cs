using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AICoreService;

var builder = WebApplication.CreateBuilder(args);
var configBuilder = new ConfigurationBuilder().
    AddJsonFile("apisettings.json").Build();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
const string contentType = "application/json";
var configSection = configBuilder.GetSection("EdenAI");
var token = configSection["token"];
var baseUrl = configSection["url"];

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/Description", (List<ObjectParameter> objectParameters) =>
    {
        var url = baseUrl + "/text/generation";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
            token);

        var serialized = JsonSerializer.Serialize(objectParameters);
        
        HttpContent httpContent = new StringContent(JsonSerializer.Serialize(
            new {
                providers="google",
                text="Give me a lovely description of this object with properties like here: "+ serialized + "\n the description length should has max. 1000 characters.",
                resolution="1024x1024"
            }), Encoding.UTF8, contentType);
        return client.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync().Result;
    })
.WithName("GetDescription")
.WithOpenApi();

app.MapPost("/Image", (ImageParameter imageParameter) =>
    {
        var url = baseUrl + "/image/generation";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
            token);

        HttpContent httpContent = new StringContent(JsonSerializer.Serialize(
            new {
                providers="openai", 
                text=imageParameter.Description,
                resolution="1024x1024"
            }), Encoding.UTF8, contentType);
        return client.PostAsync(url, httpContent).Result.Content.ReadAsStringAsync().Result;
    })
    .WithName("GetImage")
    .WithOpenApi();

app.Run();
