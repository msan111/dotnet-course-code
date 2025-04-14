using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using AutoMapper;
using Dapper;
using Helloworld.Data;
using Helloworld.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

DataContextDapper dapper = new(config);


string computersJson = File.ReadAllText("ComputersSnake.json");

// // With mapper options

Mapper mapper = new Mapper(new MapperConfiguration((cfg) =>
{
    cfg.CreateMap<ComputerSnake, Computer>()
    .ForMember(destination => destination.ComputerId, options =>
        options.MapFrom(source => source.computer_id))
    .ForMember(destination => destination.Motherboard, options =>
        options.MapFrom(source => source.motherboard))
    .ForMember(destination => destination.VideoCard, options =>
        options.MapFrom(source => source.video_card))
    .ForMember(destination => destination.HasLTE, options =>
        options.MapFrom(source => source.has_lte))
    .ForMember(destination => destination.HasWifi, options =>
        options.MapFrom(source => source.has_wifi))
    .ForMember(destination => destination.Price, options =>
        options.MapFrom(source => source.price))
    .ForMember(destination => destination.ReleaseDate, options =>
        options.MapFrom(source => source.release_date))
    .ForMember(destination => destination.CPUCores, options =>
        options.MapFrom(source => source.cpu_cores));
}));

IEnumerable<ComputerSnake>? computersSystem = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<ComputerSnake>>(computersJson);

if (computersSystem != null)
{
    IEnumerable<Computer> computerResult = mapper.Map<IEnumerable<Computer>>(computersSystem);


    Console.WriteLine("Automapper Count:" + computerResult.Count());

}

// // with  JsonPropertyName attribute
IEnumerable<Computer>? computersJsonPropertyMapping = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Computer>>(computersJson);

if (computersJsonPropertyMapping != null)
{
    Console.WriteLine("JSON Property Count:" + computersJsonPropertyMapping.Count());

}


