using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using OSPlatform = System.Runtime.InteropServices.OSPlatform;

namespace Vaetech.Shell
{
    public static class PShell
    {
        public static PShellResult Execute(string cmd, Action<PShellResult>? dataReceivedCallback = null, CancellationToken cancellationToken = default) => Execute(Environment.CurrentDirectory, cmd, dataReceivedCallback);
        public static PShellResult Execute(string fileFullPath, string cmd, Action<PShellResult>? dataReceivedCallback = null, CancellationToken cancellationToken = default)
        {
            bool ibException = false;
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var outputBuilder = new StringBuilder();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = string.IsNullOrEmpty(fileFullPath) ? PShell.GetFileName() : fileFullPath,
                    Arguments = $"{escapedArgs}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            int line = 0;
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    dataReceivedCallback?.Invoke(PShellResult.Fail(line++,e.Data));
                    ibException = true;
                }
                outputBuilder.AppendLine(e.Data);
            };
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    dataReceivedCallback?.Invoke(PShellResult.Success(line++,e.Data));
                }
                outputBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();

            return new PShellResult()
            {
                IbException = ibException,
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString()
            };
        }
        public static Task<PShellResult> ExecuteAsync(string cmd, Action<PShellResult>? dataReceivedCallback = null, CancellationToken cancellationToken = default) => ExecuteAsync(Environment.CurrentDirectory, cmd, dataReceivedCallback);
        public static async Task<PShellResult> ExecuteAsync(string fileFullPath, string cmd, Action<PShellResult>? dataReceivedCallback = null, CancellationToken cancellationToken = default)
        {
            bool ibException = false;
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var outputBuilder = new StringBuilder();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = string.IsNullOrEmpty(fileFullPath) ? PShell.GetFileName(): fileFullPath,
                    Arguments = $"{escapedArgs}",                    
                    RedirectStandardOutput = true,  
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };
            int line = 0;
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    dataReceivedCallback?.Invoke(PShellResult.Fail(line++, e.Data));
                    ibException = true;
                }
                outputBuilder.AppendLine(e.Data);
            };
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    dataReceivedCallback?.Invoke(PShellResult.Success(line++, e.Data));
                }                
                outputBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);
            process.CancelOutputRead();
            process.CancelErrorRead();            

            return new PShellResult()
            {
                IbException = ibException,
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString()
            };
        }
        private static string GetFileName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "powershell";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "/bin/bash";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "/bin/zsh";
            throw new InvalidOperationException("Unsupported platform.");
        }
    }
}
