using System.Text;

string path = @"../../../date.txt";
string text = DateTime.Now.ToString();
using (FileStream fstream = new(path, FileMode.OpenOrCreate))
{
    Console.WriteLine(text);
    byte[] buffer = Encoding.Default.GetBytes(text);
    await fstream.WriteAsync(buffer);
}

Task.Delay(1000).Wait();