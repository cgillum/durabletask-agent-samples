using System.ComponentModel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocGenerationSample.Activities;

static class ProofreadActivity
{
    const string SystemPrompt =
        """
        Your job is to proofread customer facing documentation for a new product from Contoso.
        You will be provide with proposed documentation for a product and you must do the following things:

        1. Determine if the documentation is passes the following criteria:
            1. Documentation must use a professional tone.
            1. Documentation should be free of spelling or grammar mistakes.
            1. Documentation should be free of any offensive or inappropriate language.
            1. Documentation should be technically accurate.
        2. If the documentation does not pass 1, you must write detailed feedback of the changes that are needed to improve the documentation. 
        """;

    [Function(nameof(Proofread))]
    public static async Task<ProofreadingResponse> Proofread(
        [ActivityTrigger] string documentation,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(Proofread));
        logger.LogInformation("Proofreading provided documentation...");

        // Make an LLM request
        IChatClient chatClient = executionContext.InstanceServices.GetRequiredService<IChatClient>();
        List<ChatMessage> prompt =
        [
            new ChatMessage { Role = ChatRole.System, Text = SystemPrompt },
            new ChatMessage { Role = ChatRole.User, Text = documentation },
        ];

        // The LLM will return a JSON object that matches the ProofreadingResponse schema.
        ChatCompletion<ProofreadingResponse> response = await chatClient.CompleteAsync<ProofreadingResponse>(
            prompt,
            cancellationToken: executionContext.CancellationToken);

        return response.Result ?? throw new ApplicationException("LLM failed to return a text response.");
    }
}

public class ProofreadingResponse
{
    [Description("Specifies if the proposed documentation meets the expected standards for publishing.")]
    public bool MeetsExpectations { get; set; }

    [Description("An explanation of why the documentation does or does not meet expectations.")]
    public string Explanation { get; set; } = "";

    [Description("A list of suggestions, may be empty if there no suggestions for improvement.")]
    public List<string> Suggestions { get; set; } = [];
}
