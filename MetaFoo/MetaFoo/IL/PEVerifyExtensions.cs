using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MetaFoo.IL
{
    public static class PEVerifyExtensions
    {
        public static void PEVerify(this string assemblyLocation, string sdkDirectory,
            Action<string, int, string> resultHandler, params uint[] peVerifyErrorCodesToIgnore)
        {
            var peVerifyLocation = Path.Combine(sdkDirectory, "PEVerify.exe");

            if (!File.Exists(peVerifyLocation))
                throw new FileNotFoundException($"Unable to find PEVerify at file location '{peVerifyLocation}'");

            var values = peVerifyErrorCodesToIgnore.Select(errorCode => $"0x{errorCode:x8}");
            var ignoreList = string.Join(",", values);
            var ignoredArguments = peVerifyErrorCodesToIgnore.Length > 0 ? $" /IGNORE={ignoreList}" : string.Empty;
            var argumentList = new[]
            {
                $"\"{assemblyLocation}\"",
                "/VERBOSE",
                "/IL",
                ignoredArguments
            };

            var arguments = string.Join(" ", argumentList.Where(arg => !string.IsNullOrWhiteSpace(arg)));
            var writer = new StringWriter();
            var oldWriter = Console.Out;

            Console.SetOut(writer);
            var startInfo = new ProcessStartInfo(peVerifyLocation, arguments)
            {
                RedirectStandardOutput = true, 
                CreateNoWindow = true, 
                UseShellExecute = false
            };
            
            var process = Process.Start(startInfo);

            if (process == null)
            {
                writer.Write($"Error: Unable to start PEVerify");
            }
            else
            {
                process.WaitForExit();
                writer.Write(process.StandardOutput.ReadToEnd());
                resultHandler(assemblyLocation, process.ExitCode, writer.ToString());
            }
            
            Console.SetOut(oldWriter);
        }
    }
}