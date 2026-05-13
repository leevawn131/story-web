using System.Net.Mime;
using System.Runtime.Serialization.Json;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

namespace story_web.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private const int chunkSize = 5000;
        public OllamaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> SummarizeAsync(string content)
        {
            if(string.IsNullOrWhiteSpace(content))
                return string.Empty;

            var chunks = SplitText(content,chunkSize);
            if(chunks.Count == 1)
            {
                return await CallOllamaAsync($"""
                    Hãy tóm tắt đoạn văn sau bằng tiếng Việt trong 5 đến 8 câu.
                    Chỉ trả lời bằng nội dung tóm tắt, không thêm tiêu đề, ghi chú, giải thích hoặc câu tiếng Anh.

                    {chunks[0]}
                    """);
            }
            var partialSummarizes = new List<string>();
            foreach(var chunk in chunks)
            {
                partialSummarizes.Add(await SummarizeAsync(chunk));
            }
            var combined = string.Join("\n\n",partialSummarizes);
            return await FinalSummarizeAsync(combined);
        }
        private async Task<string> FinalSummarizeAsync(string parrtialsummarize)
        {
            var prompt = $"""
                Dưới đây là các bản tóm tắt của từng phần trong một chương truyện.
                Hãy tổng hợp thành một bản tóm tắt hoàn chỉnh bằng tiếng Việt trong vòng 5 đến 8 câu.
                Chỉ trả lời bằng nội dung tóm tắt, không thêm tiêu đề, ghi chú, giải thích hoặc câu tiếng Anh.

                {parrtialsummarize}
                """;
            return await CallOllamaAsync(prompt);
        }
        private async Task<string> CallOllamaAsync(string promt)
        {
            var requestBody = new
            {
                model = "llama3",
                prompt = promt,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);

            try
            {
                var response = await _httpClient.PostAsync(
                    "http://localhost:11434/api/generate",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseText);
                return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                // Ollama server is unavailable or connection refused. Log and return empty summary.
                Console.Error.WriteLine($"Ollama request failed: {ex.Message}");
                return string.Empty;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.Error.WriteLine($"Ollama socket error: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ollama unexpected error: {ex.Message}");
                return string.Empty;
            }
        }
        private List<string> SplitText(string text, int chunksize)
        {
            var result = new List<string>();
            for(int i = 0; i< text.Length; i += chunksize)
            {
                int length = Math.Min(chunksize, text.Length - i);
                result.Add(text.Substring(i,length));
            }
            return result;
        }
    }
}
