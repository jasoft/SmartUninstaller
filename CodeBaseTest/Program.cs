using System;

Console.WriteLine($"windir: [{Environment.GetEnvironmentVariable("windir")}]");
Console.WriteLine($"SystemRoot: [{Environment.GetEnvironmentVariable("SystemRoot")}]");
Console.WriteLine($"WINDIR: [{Environment.GetEnvironmentVariable("WINDIR")}]");

// 模拟 FontCache.Util.cctor 的逻辑
var winDir = Environment.GetEnvironmentVariable("windir") ?? "";
Console.WriteLine($"winDir值: [{winDir}]");

if (string.IsNullOrEmpty(winDir))
{
    Console.WriteLine("windir 为空! 这就是崩溃原因!");
}
else
{
    // FontCache.Util 用 windir 构造字体路径的URI
    var fontPath = winDir + @"\Fonts";
    Console.WriteLine($"fontPath: [{fontPath}]");
    try
    {
        var uri = new Uri(fontPath);
        Console.WriteLine($"URI: [{uri}]");
    }
    catch (UriFormatException ex)
    {
        Console.WriteLine($"URI 构造失败: {ex.Message}");
        Console.WriteLine("windir 的值不合法导致 URI 解析失败!");
    }
}
