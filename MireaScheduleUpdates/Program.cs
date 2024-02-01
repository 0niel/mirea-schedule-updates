using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string path = @"date.txt";
string text = DateTime.Now.ToString();
using (FileStream fstream = new(path, FileMode.OpenOrCreate))
{
    byte[] buffer = Encoding.Default.GetBytes(text);
    await fstream.WriteAsync(buffer);
}


app.MapGet("/", () => "Hello World!");

app.Run();

await app.StopAsync();