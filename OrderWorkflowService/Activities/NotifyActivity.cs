using Dapr.Workflow;
using Workflow.Shared.Dtos;

namespace OrderWorkflowService.Activities;

public class NotifyActivity : WorkflowActivity<NotificationDto, object?>
{
    private readonly ILogger _logger;

    public NotifyActivity(ILogger<NotifyActivity> logger)
    {
        _logger = logger;
    }

    public override async Task<object?> RunAsync(WorkflowActivityContext context, NotificationDto notificationDto)
    {
        _logger.LogInformation(notificationDto.Message);

        await Task.CompletedTask;
        return null;
    }
}