using System;
using System.Reflection;

var asm = Assembly.LoadFrom(@"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\9.0.17\PresentationCore.dll");
Console.WriteLine($"FullName: {asm.FullName}");
Console.WriteLine($"CodeBase: {asm.CodeBase}");
Console.WriteLine($"Location: {asm.Location}");

// 模拟 FontCache.Util.cctor 的逻辑
var codeBase = asm.CodeBase;
Console.WriteLine($"CodeBase为空: {string.IsNullOrEmpty(codeBase)}");
Console.WriteLine($"CodeBase是null: {codeBase == null}");

if (!string.IsNullOrEmpty(codeBase))
{
    var uri = new Uri(codeBase);
    Console.WriteLine($"URI: {uri}");
    var localPath = uri.LocalPath;
    Console.WriteLine($"LocalPath: {localPath}");
    var dir = System.IO.Path.GetDirectoryName(localPath);
    Console.WriteLine($"Directory: {dir}");
}
