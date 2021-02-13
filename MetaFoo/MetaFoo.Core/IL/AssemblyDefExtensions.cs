using System.IO;
using System.Reflection;
using dnlib.DotNet;

namespace MetaFoo.Core.IL
{
    public static class AssemblyDefExtensions
    {
        public static Assembly ToLoadedAssembly(this AssemblyDef assemblyDef)
        {
            var stream = new MemoryStream();
            assemblyDef.Write(stream);

            var bytes = stream.ToArray();
            return Assembly.Load(bytes);
        }
    }
}