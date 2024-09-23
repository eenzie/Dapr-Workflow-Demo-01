using Dapr.Client;
using Dapr.Workflow;
using Workflow.Shared.Dtos;

namespace OrderWorkflowService.Activities
{
    public class ProcessPaymentActivity : WorkflowActivity<PaymentRequestDto, object?>
    {
        private readonly DaprClient _daprClient;

        public ProcessPaymentActivity(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public override Task<object?> RunAsync(WorkflowActivityContext context, PaymentRequestDto input)
        {
            return null;
        }
    }
}
