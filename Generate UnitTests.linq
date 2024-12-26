<Query Kind="Program">
  <NuGetReference>Microsoft.Extensions.Configuration</NuGetReference>
  <NuGetReference>Microsoft.SemanticKernel</NuGetReference>
  <NuGetReference>Microsoft.SemanticKernel.Connectors.OpenAI</NuGetReference>
  <Namespace>Microsoft.Extensions.Configuration</Namespace>
  <Namespace>Microsoft.SemanticKernel</Namespace>
  <Namespace>Microsoft.SemanticKernel.Connectors.OpenAI</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Microsoft.SemanticKernel.ChatCompletion</Namespace>
</Query>

#nullable enable
public async Task Main()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
    var cancellationToken = cts.Token;

    // 1) Set the root folder where your analyzer code lives
    var rootDirectory = @"D:\Gendarme.Analyzers\Gendarme.Analyzers";

    // 2) Find all *.cs code files in the directory (recursive)
    var codeFiles = Directory.GetFiles(rootDirectory, "*Analyzer.cs", SearchOption.AllDirectories);

    // 3) Build the Kernel / agent
    //    Note: We assume the OpenAI API key is stored in your environment variable "OPENAI_API_KEY".
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new Exception("OpenAI API key was not found in environment variables (OPENAI_API_KEY).");
    }

    // Configure the kernel:
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion(
        modelId: "gpt-4o-mini", // OpenAI Model name
        apiKey: apiKey // OpenAI API Key
    );
    var kernel = builder.Build();

    // Retrieve the chat completion service
    var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

    // 4) Some custom instructions to pass to the agent about how the tests should be formatted.
    var userTestInstructions = @"You are a test generator.
Given a C# source file and an existing test file, generate new or updated unit tests. 
Output only the code in triple-backtick fences, like:

```csharp
// updated test code here
```
";

    var testTemplateInstructions = @"
Here is a template how the tests should be structured:
    
```csharp
[TestOf(typeof(AvoidAssemblyVersionMismatchAnalyzer))]
public sealed class AvoidAssemblyVersionMismatchAnalyzerTests
{
    [Fact]
    public async Task TestVersionMismatch()
    {
        const string testCode = ""
using System.Reflection;
    [assembly: AssemblyVersion(""1.0.0.0"")]
    [assembly: AssemblyFileVersion(""1.0.0.1"")]
    [assembly: AssemblyInformationalVersion(""1.0.0"")]

    public class MyClass { }
"";


        var context = new CSharpAnalyzerTest<AvoidAssemblyVersionMismatchAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AssemblyVersionMismatch)
            .WithSpan(6, 14, 6, 21)
            .WithArguments(""1.0.0.0"");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}
```";

    // 5) Loop through each discovered code file
    foreach (var codeFilePath in codeFiles)
    {
        // Transform the codeFilePath to find an equivalent test file path.
        var testFilePath = codeFilePath
            .Replace(rootDirectory, rootDirectory + ".Tests")
            .Replace(".cs", "Tests.cs");

        // Check if the test file exists
        bool testFileExists = File.Exists(testFilePath);

        // Read the code file and test file into memory
        var codeFileContent = File.ReadAllText(codeFilePath);
		var existingTestContent = testFileExists ? File.ReadAllText(testFilePath) : string.Empty;

		// Skip files where there already tests present
		if (existingTestContent.Length > 600)
		{
			continue;
		}

        // Set up the chat history
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(userTestInstructions);
		chatHistory.AddSystemMessage(testTemplateInstructions);
        chatHistory.AddUserMessage($@"#region C# SOURCE FILE START
{codeFileContent}
#endregion C# SOURCE FILE END

#region EXISTING TEST FILE START
{existingTestContent}
#endregion EXISTING TEST FILE END");

        // Stream the chat completion response
        Console.WriteLine($"Generating tests for {codeFilePath}...");
        Console.Write("Assistant: ");
        var responseBuilder = new System.Text.StringBuilder();

        await foreach (var chatMessage in chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory,
            executionSettings: (PromptExecutionSettings?)null,
            kernel: kernel,
            cancellationToken: cancellationToken))
        {
            if (chatMessage?.Content != null)
            {
                Console.Write(chatMessage.Content);
                responseBuilder.Append(chatMessage.Content);
            }
        }
        Console.WriteLine();

        // Extract the code from ```csharp ... ``` (triple-backtick fenced block)
        var agentResponse = responseBuilder.ToString();
        var newTestCode = ExtractCSharpCode(agentResponse);

        if (string.IsNullOrWhiteSpace(newTestCode))
        {
            Console.WriteLine($"No new tests were generated for {codeFilePath}");
            continue;
        }

        // Make sure the test directory exists (in case it doesn't)
        var testDir = Path.GetDirectoryName(testFilePath);
        Directory.CreateDirectory(testDir);

        // Overwrite the existing or newly created file with the test code
        File.WriteAllText(testFilePath, newTestCode);
        Console.WriteLine($"Updated test file: {testFilePath}");
    }

    Console.WriteLine("Done.");
}

/// <summary>
/// Utility method to extract content from the first triple-backtick csharp fence.
/// E.g. if agentResponse contains:
///   ...some text...
///   ```csharp
///   // test code
///   ```
///   ...some text...
/// </summary>
private static string ExtractCSharpCode(string agentResponse)
{
    // We'll look for ```csharp ... ``` blocks using a regex
    var pattern = @"```csharp([\s\S]*?)```";
    var match = Regex.Match(agentResponse, pattern);
    if (match.Success && match.Groups.Count > 1)
    {
        return match.Groups[1].Value.Trim();
    }
    return string.Empty;
}
