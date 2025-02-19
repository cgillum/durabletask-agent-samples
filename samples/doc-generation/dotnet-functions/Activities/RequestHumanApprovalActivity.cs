using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace DocGenerationSample.Activities;

static class RequestHumanApprovalActivity
{
    [Function(nameof(RequestHumanApproval))]
    public static async Task<bool> RequestHumanApproval(
        [ActivityTrigger] string documentationToApprove,
        string instanceId,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(RequestHumanApproval));
        logger.LogInformation(
            "Requesting human approval for documentation for orchestration: {instanceId}...",
            instanceId);

        // Simulate a delay for requesting human approval. Normally this would be a call to an external system
        // to send an email or message to a human approver that includes the documentation to approve and a link
        // to submit the approval.
        await Task.Delay(1000);
        return true;
    }
}
