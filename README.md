# Durable Agent Samples

This repository contains samples for building durable agents using [Azure Durable Functions](https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-overview).

## Samples

- [Document Generation](samples/doc-generation/): Agentic workflow for generating documentation from a product description.

## Benefits of Durable Functions

Azure Durable Functions are one of many options for building AI agents. However, Durable Functions has some unique benefits that make it a great choice for building agents:

- **Durable**: You don't have to worry about them failing and losing progress due to transient failures, which is especially important when restarting a process from the beginning could be expensive.
- **Distributed**: You can scale out workflows across multiple machines and take advantage of scaled-out compute resources.
- **Stateful**: Execution state is managed for you, so you don't have to worry about persisting state to a database or other storage, simplifying the cost of development and maintenance.
- **Long-running**: Agents can run for days, weeks, or even months. You can even have the agent sleep for a while and wake up later to continue working without losing state.
- **Serverless**: You only pay for the compute resources you use, and you don't have to worry about managing servers or infrastructure. If an agent is sleeping, you don't pay for it.
- **Event-driven**: Agents can be triggered by events, such as messages from a queue or HTTP requests, allowing for flexible and responsive workflows.

See [this blog post](https://blog.cgillum.tech/building-serverless-ai-agents-using-durable-functions-on-azure-e1272882082c) for a longer-form discussion on Durable Functions and the agentic patterns that can be built with them.
