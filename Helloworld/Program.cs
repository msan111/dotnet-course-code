using System.Data;
using System.Text.Json;
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

//DESERIALIZATION

string computersJson = File.ReadAllText("Computers.json");


JsonSerializerOptions options = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

//Newtonsoft.Json
IEnumerable<Computer>? computersNewtonSoft = JsonConvert.DeserializeObject<IEnumerable<Computer>>(computersJson);

//System.Text.Json
IEnumerable<Computer>? computersSystem = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Computer>>(computersJson, options);


if (computersNewtonSoft != null)
{
    foreach (Computer computer in computersNewtonSoft)
    {
        string sql = @"INSERT INTO TutorialAppSchema.Computer (
            Motherboard, 
            CPUCores,
            HasWifi,
            HasLTE, 
            ReleaseDate, 
            Price, 
            VideoCard
            ) VALUES(   @Motherboard, 
                        @CPUCores, 
                        @HasWifi,  
                        @HasLTE, 
                        @ReleaseDate, 
                        @Price, 
                        @VideoCard
                    )";
        dapper.ExecuteSql(sql, computer);
    }
}

//SERIALIZATION

JsonSerializerSettings settings = new()
{
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
};

string computersCopyNewtonsoft = JsonConvert.SerializeObject(computersNewtonSoft, settings);

File.WriteAllText("computersCopyNewtonsoft.txt", computersCopyNewtonsoft);



string computersCopySystem = System.Text.Json.JsonSerializer.Serialize(computersSystem, options);

File.WriteAllText("computersCopySystem.txt", computersCopySystem);

 






