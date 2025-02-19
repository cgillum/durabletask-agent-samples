using System.Net;
using DocGenerationSample.Activities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DocGenerationSample.Orchestrations;

public static class GenerateDocumentationOrchestration
{
    const int MaxProofreadingAttempts = 10;

    const string ApproveDocumentationEventName = "ApproveDocumentation";

    // Default retry policy for all activities that invoke LLMs.
    static readonly RetryPolicy LanguageModelRetryPolicy = new(
        maxNumberOfAttempts: 5,
        firstRetryInterval: TimeSpan.FromSeconds(10),
        backoffCoefficient: 2.0);

    // The amount of time to wait for the human approver to approve the final documentation.
    static readonly TimeSpan ApprovalTimeout = TimeSpan.FromHours(1);

    [Function(nameof(GenerateDocumentation))]
    public static async Task<string> GenerateDocumentation(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(GenerateDocumentationOrchestration));
        logger.LogInformation(
            "Running new document generation orchestration with instance ID = {instanceId}",
            context.InstanceId);

        // Step 1: Gather product information
        string productInfo = await context.CallGatherProductInfoActivityAsync("Contoso GlowBrew");

        string documentation = string.Empty;

        // Documentation generation/feedback loop
        bool meetsExpectations = false;
        for (int i = 0; i < MaxProofreadingAttempts; i++)
        {
            // Step 2: Generate documentation
            documentation = await context.CallGenerateDocumentationAsync(
                request: new GenerateDocumentationRequest(productInfo),
                options: TaskOptions.FromRetryPolicy(LanguageModelRetryPolicy));

            // Step 3: Proofread documentation
            ProofreadingResponse proofreadingResponse = await context.CallProofreadAsync(
                documentation,
                options: TaskOptions.FromRetryPolicy(LanguageModelRetryPolicy));

            // Step 4: If the documentation meets expectations, return it.
            if (proofreadingResponse.MeetsExpectations)
            {
                logger.LogInformation("Documentation meets expectations.");
                meetsExpectations = true;
                break;
            }

            // Step 5: If the documentation does not meet expectations, apply suggestions and try again.
            logger.LogInformation("Documentation does not meet expectations. Applying suggestions and trying again.");
            documentation = await context.CallApplySuggestionsAsync(
                new ApplySuggestionsRequest(productInfo, documentation, proofreadingResponse.Suggestions),
                options: TaskOptions.FromRetryPolicy(LanguageModelRetryPolicy));
        }

        // Step 6: If the documentation does not meet expectations after 10 attempts, fail.
        if (!meetsExpectations)
        {
            throw new ApplicationException(
                $"Documentation does not meet expectations after {MaxProofreadingAttempts} attempts.");
        }

        // Step 7: Request approval from a human approver to publish the documentation
        //         and wait for the approval.
        await context.CallRequestHumanApprovalAsync(documentation);
        string approverId = await context.WaitForExternalEvent<string>(
            eventName: ApproveDocumentationEventName,
            timeout: ApprovalTimeout);

        logger.LogInformation("Received approval to publish from {approverId}", approverId);

        // Step 8: Publish the documentation
        await context.CallPublishDocumentationAsync(documentation);

        // Save the final documentation as the orchestration's output.
        return documentation;
    }

    [Function(nameof(GenerateDocumentationHttpDemo))]
    public static async Task<HttpResponseData> GenerateDocumentationHttpDemo(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GenerateDocumentationHttpDemo));

        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(GenerateDocumentationOrchestration));

        logger.LogInformation("Started documentation orchestration with ID = '{instanceId}'.", instanceId);

        // Respond with JSON in the form of { requestId: <instanceId> }
        HttpResponseData res = req.CreateResponse(HttpStatusCode.Accepted);
        await res.WriteAsJsonAsync(
            new { requestId = instanceId },
            executionContext.CancellationToken);
        return res;
    }

    [Function(nameof(ApproveDocumentation))]
    public static async Task ApproveDocumentation(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData request,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ApproveDocumentationRequest? approval = await request.ReadFromJsonAsync<ApproveDocumentationRequest>();
        if (approval is null)
        {
            request.CreateResponse(HttpStatusCode.BadRequest);
            return;
        }

        ILogger logger = executionContext.GetLogger(nameof(ApproveDocumentation));
        logger.LogInformation(
            "Approving documentation for orchestration with ID = '{requestId}'.",
            approval.RequestId);

        // Raise an event to the orchestration instance to approve the documentation.
        await client.RaiseEventAsync(
            instanceId: approval.RequestId,
            eventName: ApproveDocumentationEventName,
            approval.ApproverId,
            executionContext.CancellationToken);
    }

    public record ApproveDocumentationRequest(string RequestId, string ApproverId);
}
