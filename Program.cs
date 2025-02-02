using System.Net.Http.Headers;
using System.Text;
using dotenv.net;
using System.Text.Json;

class Program
{
    const string Model = "claude-3-5-sonnet-20241022";
    const int MaxTokens = 1000;

    const string ApiUrl = "https://api.anthropic.com/v1/messages";
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

            // var response = await SendPdfToClaudeDirectly("paradise_lost.pdf");
            // Console.WriteLine(response);

            var response = await GetNamedCharactersFromPdf("paradise_lost.pdf");
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

            var response = await client.PostAsync(ApiUrl, content);

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

            var response = await client.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }

    public static async Task<string?> GetNamedCharactersFromPdf(string pdfFilePath)
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
                                },
                                title = "Document Title", // optional
                                context = "Context about the document that will not be cited from", // optional
                                citations = new
                                {
                                    enabled = true
                                }
                            },
                            new
                            {
                                type = "text",
                                // Annoyingly, Claude isn't listening to this prompt, but I don't feel like spending more time on this example atm.
                                text = "List all named characters in this document. This document uses '\\n' to delineate new line characters. When providing citations, ensure that you include the text of the line where the character is first named, considering the '\\n' characters to accurately reflect the line breaks. Do not clump the text together; instead, treat each '\\n' as a new line. For example, if the document contains 'Line 1\\nLine 2', treat it as two separate lines."
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}