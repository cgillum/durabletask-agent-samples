using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocGenerationSample.Activities;

static class GatherProductInfoActivity
{
    [Function(nameof(GatherProductInfoActivity))]
    public static async Task<string> GatherProductInfo(
        [ActivityTrigger] string productName,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GatherProductInfo));
        logger.LogInformation("Gathering product info for {productName}.", productName);
        
        // Simulate a delay for gathering product information
        await Task.Delay(1000);

        // For example purposes we just return some fictional information.
        return
            """
            ### Product Description:
            GlowBrew is a revolutionary AI driven coffee machine with industry leading number of LEDs and programmable
            light shows. The machine is also capable of brewing coffee and has a built in grinder.

            ### Product Features:
            1. **Luminous Brew Technology**: Customize your morning ambiance with programmable LED lights that sync with your brewing process.
            2. **AI Taste Assistant**: Learns your taste preferences over time and suggests new brew combinations to explore.
            3. **Gourmet Aroma Diffusion**: Built-in aroma diffusers enhance your coffee's scent profile, energizing your senses before the first sip.

            ### Troubleshooting:
            - **Issue**: LED Lights Malfunctioning
            - **Solution**: Reset the lighting settings via the app. Ensure the LED connections inside the GlowBrew are secure. Perform a factory reset if necessary.
            """;
    }
}
