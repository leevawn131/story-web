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
            var piperExe = @"C:\Users\DINH VAN\AppData\Local\Programs\Python\Python313\Scripts\piper.exe";
            var modelPath = @"C:\ASPNET\story-web\vi_VN-vais1000-medium.onnx";
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = 
                    $"/c type \"{tempFile}\" | " +
                    $"\"{piperExe}\" " +
                    $"--model \"{modelPath}\" "+
                    $"--output_file \"{outputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            if(process == null)
            {
                return null;
            }
            await process.WaitForExitAsync();
            var error = await process.StandardError.ReadToEndAsync();
            File.Delete(tempFile);
            if (process.ExitCode != 0)
            {
                throw new Exception("piper error: " + error);
            }
            return "/audio/" + filename;
        }
    }
}