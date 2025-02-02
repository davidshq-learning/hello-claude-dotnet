using System.Net.Http.Headers;
using System.Text;
using dotenv.net;
using System.Text.Json;

class Program
{
    const string Model = "claude-3-5-sonnet-20241022";
    const int MaxTokens = 1000;
    static string? ApiKey;
    static async Task Main(string[] args)
    {
        DotEnv.Load();
        ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        // string prompt = "What is the capital of France?";
        
        try
        {
            // Being lazy here, commenting out what we aren't using atm
            // var response = await CreateClaudeCompletion(prompt);
            // Console.WriteLine($"Claude's response: {response}");
            var response = await SendPdfToClaudeDirectly("paradise_lost.pdf");
            Console.WriteLine(response);

        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    public static async Task<string> CreateClaudeCompletion(string prompt, string model = Model, int maxTokens = MaxTokens)
    {
        // Load environment variables from .env file
        DotEnv.Load();

        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new InvalidOperationException("Please set the ANTHROPIC_API_KEY environment variable");
        }

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            // This is the API version
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var data = new
            {
                model = model,
                max_tokens = maxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            var responseData = await response.Content.ReadAsStringAsync();

            // For our purposes, lets show raw, not as parsed JSON
            return responseData;
        }
    }

    public static async Task<string?> SendPdfToClaudeDirectly(string pdfFilePath)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            // Read the PDF file and encode it in base64
            byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);
            string pdfBase64 = Convert.ToBase64String(pdfBytes);

            var data = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "document",
                                source = new
                                {
                                    type = "base64",
                                    media_type = "application/pdf",
                                    data = pdfBase64
                                }
                            },
                            new
                            {
                                type = "text",
                                text = "Rewrite the contents of this file in modern English"
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}