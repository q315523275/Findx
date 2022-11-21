// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Findx.Extensions;

Console.WriteLine("Hello, World!");

var a = Findx.Utils.RegexUtil.GetValue("abc4d5e6hh5654", @"\d+");
Console.WriteLine($"a的值为：{a};");
var b = Findx.Utils.RegexUtil.GetValues("abc4d5e6hh5654", @"\d+");
Console.WriteLine($"b的值为：{JsonSerializer.Serialize(b)}");

