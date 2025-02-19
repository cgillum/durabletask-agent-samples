using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights integration
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

string? azureOpenAiEndpoint = builder.Configuration.GetSection("AZURE_OPENAI_ENDPOINT").Value?.Trim();
if (string.IsNullOrEmpty(azureOpenAiEndpoint))
{
    throw new ApplicationException("AZURE_OPENAI_ENDPOINT is not set.");
}

string? modelId = builder.Configuration.GetSection("AZURE_OPENAI_MODEL_ID").Value?.Trim();
if (string.IsNullOrEmpty(modelId))
{
    throw new ApplicationException("AZURE_OPENAI_MODEL_ID is not set.");
}

TokenCredential credential = new DefaultAzureCredential();

// Azure OpenAI integration
builder.Services
    .AddChatClient(new AzureOpenAIClient(new Uri(azureOpenAiEndpoint), credential).AsChatClient(modelId))
    .UseFunctionInvocation()
    .UseOpenTelemetry();

builder.Build().Run();
