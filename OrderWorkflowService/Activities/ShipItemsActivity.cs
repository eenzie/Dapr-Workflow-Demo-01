using Dapr.Client;
using Dapr.Workflow;
using Workflow.Shared.Dtos;

namespace OrderWorkflowService.Activities
{
    public class ShipItemsActivity : WorkflowActivity<OrderDto, object?>
    {
        private readonly DaprClient _daprClient;
        private static readonly string PubSubName = "pubsub";
        private static readonly string TopicName = "pizza-orders";

        public ShipItemsActivity(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, OrderDto input)
        {
            await _daprClient.PublishEventAsync(PubSubName, TopicName, input);

            return null;
        }
    }
}
