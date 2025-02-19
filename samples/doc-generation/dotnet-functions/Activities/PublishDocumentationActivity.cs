using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocGenerationSample.Activities;

static class PublishDocumentationActivity
{
    [Function(nameof(PublishDocumentation))]
    public static async Task<bool> PublishDocumentation(
        [ActivityTrigger] string documentation,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(PublishDocumentation));
        logger.LogInformation("Publishing documentation...");

        // Simulate a delay for publishing the documentation. Normally this would be a call to an external system
        // to publish the documentation.
        await Task.Delay(1000);
        return true;
    }
}
