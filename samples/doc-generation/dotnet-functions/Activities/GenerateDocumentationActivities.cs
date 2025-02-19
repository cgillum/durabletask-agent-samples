using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocGenerationSample.Activities;

/// <summary>
/// Input type for <see cref="GenerateDocumentationActivities"/>."/>
/// </summary>
/// <param name="ProductInfo">A text description of the product.</param>
public record GenerateDocumentationRequest(string ProductInfo);

/// <summary>
/// Input type for <see cref="GenerateDocumentationActivities"/>.
/// </summary>
/// <param name="ProductInfo">A text description of the product.</param>
/// <param name="Documentation">The previously generated documentation.</param>
/// <param name="Suggestions">Suggestions for improvements.</param>
public record ApplySuggestionsRequest(string ProductInfo, string Documentation, List<string> Suggestions);

/// <summary>
/// Activity that generates customer facing documentation for a product.
/// </summary>
static class GenerateDocumentationActivities
{
    const string SystemPrompt =
        """
        Your job is to write high quality and engaging customer facing documentation for a new product from Contoso.
        You will be provide with information about the product in the form of internal documentation, specs, and
        troubleshooting guides and you must use this information and nothing else to generate the documentation.
        If suggestions are provided on the documentation you create, take the suggestions into account and rewrite
        the documentation. Make sure the product sounds amazing.
        """;

    [Function(nameof(GenerateDocumentation))]
    public static async Task<string> GenerateDocumentation(
        [ActivityTrigger] GenerateDocumentationRequest request,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GenerateDocumentation));
        logger.LogInformation("Generating documentation for provided product info...");

        // Make an LLM request
        IChatClient chatClient = executionContext.InstanceServices.GetRequiredService<IChatClient>();
        List<ChatMessage> prompt =
        [
            new ChatMessage { Role = ChatRole.System, Text = SystemPrompt },
            new ChatMessage { Role = ChatRole.User, Text = request.ProductInfo },
        ];

        ChatCompletion response = await chatClient.CompleteAsync(prompt, null, executionContext.CancellationToken);
        string responseText = response.Choices[0].Text ??
            throw new ApplicationException("LLM failed to return a text response.");

        logger.LogDebug("Generated documentation: {responseText}", responseText);
        return responseText;
    }

    [Function(nameof(ApplySuggestions))]
    public static async Task<string> ApplySuggestions(
        [ActivityTrigger] ApplySuggestionsRequest request,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(ApplySuggestions));
        logger.LogInformation("Applying suggestions to provided documentation...");

        // Make an LLM request
        IChatClient chatClient = executionContext.InstanceServices.GetRequiredService<IChatClient>();
        List<ChatMessage> prompt =
        [
            new ChatMessage { Role = ChatRole.System, Text = SystemPrompt },
            new ChatMessage { Role = ChatRole.User, Text = request.ProductInfo },
            new ChatMessage { Role = ChatRole.Assistant, Text = request.Documentation },
            new ChatMessage { Role = ChatRole.User, Text = string.Join('\n', request.Suggestions) },
        ];

        ChatCompletion response = await chatClient.CompleteAsync(prompt, null, executionContext.CancellationToken);
        string responseText = response.Choices[0].Text ??
            throw new ApplicationException("LLM failed to return a text response.");

        logger.LogDebug("Generated documentation: {responseText}", responseText);
        return responseText;
    }
}
