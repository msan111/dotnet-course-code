using System.Data;
using Dapper;
using Helloworld.Data;
using Helloworld.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

DataContextDapper dapper = new DataContextDapper(config);
DataContextEF entityFramework = new DataContextEF(config);


Computer myComputer = new Computer
{
    Motherboard = "Asus",
    CPUCores = 8,
    HasWifi = true,
    HasLTE = false,
    ReleaseDate = DateTime.Now,
    Price = 1200.99M,
    VideoCard = "NVIDIA RTX 3080"
};

entityFramework.Add(myComputer);
entityFramework.SaveChanges();

string sql = @"
INSERT INTO TutorialAppSchema.Computer (
    Motherboard, 
    CPUCores,
    HasWifi,
    HasLTE, 
    ReleaseDate, 
    Price, 
    VideoCard
    ) VALUES (
    @Motherboard, 
    @CPUCores, 
    @HasWifi, 
    @HasLTE, 
    @ReleaseDate, 
    @Price, 
    @VideoCard
    )";
Console.WriteLine(sql);

//int result = dapper.ExecuteSqlWithRowCount(sql);
bool result = dapper.ExecuteSql(sql, myComputer);

Console.WriteLine(result);

string sqlSelect = @"
    SELECT 
        Computer.ComputerId,
        Computer.Motherboard, 
        Computer.CPUCores,
        Computer.HasWifi,
        Computer.HasLTE, 
        Computer.ReleaseDate, 
        Computer.Price, 
        Computer.VideoCard
     FROM TutorialAppSchema.Computer";

IEnumerable<Computer>? computersEf = entityFramework.Computer?.ToList<Computer>();

if (computersEf != null)
{
    foreach (Computer singleComputer in computersEf)
    {
        Console.WriteLine($"ComputerId: {singleComputer.ComputerId}");
        Console.WriteLine($"Motherboard: {singleComputer.Motherboard}");
        Console.WriteLine($"CPUCores: {singleComputer.CPUCores}");
        Console.WriteLine($"HasWifi: {singleComputer.HasWifi}");
        Console.WriteLine($"HasLTE: {singleComputer.HasLTE}");
        Console.WriteLine($"ReleaseDate: {singleComputer.ReleaseDate}");
        Console.WriteLine($"Price: {singleComputer.Price}");
        Console.WriteLine($"VideoCard: {singleComputer.VideoCard}");
        Console.WriteLine();
        Console.WriteLine("===================================");
    }

}

IEnumerable<Computer> computers = dapper.LoadData<Computer>(sqlSelect);
foreach (Computer singleComputer in computers)
{
    Console.WriteLine($"Motherboard: {singleComputer.Motherboard}");
    Console.WriteLine($"CPUCores: {singleComputer.CPUCores}");
    Console.WriteLine($"HasWifi: {singleComputer.HasWifi}");
    Console.WriteLine($"HasLTE: {singleComputer.HasLTE}");
    Console.WriteLine($"ReleaseDate: {singleComputer.ReleaseDate}");
    Console.WriteLine($"Price: {singleComputer.Price}");
    Console.WriteLine($"VideoCard: {singleComputer.VideoCard}");
    Console.WriteLine();
    Console.WriteLine("===================================");
}