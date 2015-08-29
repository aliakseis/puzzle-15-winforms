using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany(ThisAssembly.Company)]
[assembly: AssemblyProduct(ThisAssembly.Product)]

[assembly: AssemblyTitle(ThisAssembly.Title)]
[assembly: AssemblyDescription(ThisAssembly.Description)]

//[assembly: AssemblyCopyright(ThisAssembly.Copyright)]
[assembly: AssemblyTrademark(ThisAssembly.Trademark)]

[assembly: AssemblyVersion(ThisAssembly.Version)]

//[assembly: AssemblyKeyName("mi")]
[assembly: Guid("DB576FA7-531D-4041-8149-36CA944A1045")]

sealed class ThisAssembly
{
    ThisAssembly(){}

    public const string Company="";
    public const string Product="Mi15";
    public const string Title="Mi15";
    public const string Description="15 cell game.";
    public const string Copyright="O. Mihailik, marth 2004";
    public const string Trademark="Mi15";
    public const string Version="0.5.*";
}
