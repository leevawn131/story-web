using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace story_web.Services
{
    public class PiperService
    {
        private readonly IWebHostEnvironment _env;
        public PiperService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string?> GenerateAudioAsync(string text, int chapterId)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            var audioFolder = Path.Combine(_env.WebRootPath, "audio");
            Directory.CreateDirectory(audioFolder);
            var filename = $"chapter_{chapterId}.wav";
            var outputPath = Path.Combine(audioFolder,filename);
            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile,text,Encoding.UTF8);
            var configuredPiper = @"C:\Users\DINH VAN\AppData\Local\Programs\Python\Python313\Scripts\piper.exe";
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "vi_VN-vais1000-medium.onnx");

            // Prefer configured full path, but fall back to 'piper' on PATH if not found
            var piperExe = File.Exists(configuredPiper) ? configuredPiper : "piper";

            // Validate model file exists
            if (!File.Exists(modelPath))
            {
                Console.Error.WriteLine($"Piper model not found at: {modelPath}");
                try { File.Delete(tempFile); } catch { }
                return null;
            }

            // If using 'piper' fallback, try to resolve it to a full path on PATH
            if (string.Equals(piperExe, "piper", StringComparison.OrdinalIgnoreCase))
            {
                var resolved = ResolveExecutableOnPath("piper.exe");
                if (resolved is null)
                {
                    Console.Error.WriteLine("piper executable was not found on PATH and configured path is missing.");
                    try { File.Delete(tempFile); } catch { }
                    return null;
                }

                piperExe = resolved;
            }

            if (!File.Exists(piperExe))
            {
                Console.Error.WriteLine($"piper executable not found: {piperExe}");
                try { File.Delete(tempFile); } catch { }
                return null;
            }

            var psi = new ProcessStartInfo
            {
                FileName = piperExe,
                Arguments = $"--model \"{modelPath}\" --input_file \"{tempFile}\" --output_file \"{outputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                try { File.Delete(tempFile); } catch { }
                return null;
            }

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            try { File.Delete(tempFile); } catch { }

            if (process.ExitCode != 0 || !File.Exists(outputPath))
            {
                // Log for debugging; don't throw to avoid crashing the request pipeline
                Console.Error.WriteLine($"piper error (exit={process.ExitCode}). stdout: {stdout} stderr: {stderr}");
                return null;
            }

            return "/audio/" + filename;
        }

        private static string? ResolveExecutableOnPath(string exeName)
        {
            var paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in paths)
            {
                try
                {
                    var candidate = Path.Combine(p, exeName);
                    if (File.Exists(candidate))
                        return candidate;
                }
                catch { }
            }

            return null;
        }
    }
}