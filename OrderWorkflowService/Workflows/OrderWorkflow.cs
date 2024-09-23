using Dapr.Workflow;
using OrderWorkflowService.Activities;
using Workflow.Shared.Dtos;
using Workflow.Shared.IntegrationEvents;

namespace OrderWorkflowService.Workflows;

// TODO: Hvordan hænger det sammen med Program.cs service oprettelse?
public class OrderWorkflow : Workflow<OrderDto, OrderResultDto>
{
    public override async Task<OrderResultDto> RunAsync(WorkflowContext context, OrderDto order)
    {
        /*--------------------------------------------------------------------------------------*/
        // Opretter ny ordre med status Received (0)
        var newOrder = order with { Status = OrderStatusDto.Received };

        // Sender besked om ordre modtaget
        await context.CallActivityAsync(
            nameof(NotifyActivity),
            new NotificationDto($"Received order {order.ShortId} from {order.CustomerDto.Name}.", newOrder));

        // CreateOrderActivity gemmer den nyligt oprettede ordre i state
        await context.CallActivityAsync(
            nameof(CreateOrderActivity), newOrder);

        /*--------------------------------------------------------------------------------------*/

        // Opdaterer ordre med status CheckingPayment (4)
        newOrder = order with { Status = OrderStatusDto.CheckingPayment };

        // Sender besked om betalingsanmodning
        await context.CallActivityAsync(
            nameof(NotifyActivity),
            new NotificationDto($"Requesting payment for order {order.ShortId} from {order.CustomerDto.Name}.", newOrder));

        // Gemmer den opdaterede ordre(status) i state
        await context.CallActivityAsync(
            nameof(CreateOrderActivity), newOrder);

        /*--------------------------------------------------------------------------------------*/

        // Vi venter her på at betaling be-/afkræftes
        var orderPaymentSuccess = await context.WaitForExternalEventAsync<bool>(nameof(PaymentProcessedEvent));

        // TODO: Er FAIL FAST ikke også bedre her?
        /* TODO: Vil det give mening at lave metoder der indeholder hvert trin i workflow, 
         * som så bliver kaldt i nested statement? */
        if (orderPaymentSuccess)
        {
            // Opdaterer status til CheckingInventory (1)
            newOrder = newOrder with { Status = OrderStatusDto.CheckingInventory };

            // Sender besked om lagertjek
            await context.CallActivityAsync(
                nameof(NotifyActivity),
                new NotificationDto($"Checking inventory for order {order.ShortId}.", newOrder));

            // Gemmer den opdaterede ordre(status) i state
            await context.CallActivityAsync(
                nameof(CreateOrderActivity), newOrder);

            /*--------------------------------------------------------------------------------------*/

            // Tjekker om der er tilstrækkeligt med lagerbeholdning
            var inventoryResult = await context.CallActivityAsync<InventoryResultDto>(
                nameof(ReserveItemsActivity),
                new InventoryRequestDto(newOrder.OrderItems));

            if (inventoryResult.IsSufficientInventory)
            {
                // Opdaterer status til SufficientInventory (2)
                newOrder = newOrder with { Status = OrderStatusDto.SufficientInventory };

                // Sender notifikation om at der er nok på lager
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new NotificationDto($"Order {order.ShortId} has sufficient inventory. Proceeding to shipping.", newOrder));

                // Gemmer den opdaterede ordre(status) i state
                await context.CallActivityAsync(
                    nameof(CreateOrderActivity), newOrder);

                /*--------------------------------------------------------------------------------------*/

                // Kalder næste activity ShipItems
                await context.CallActivityAsync(nameof(ShipItemsActivity), newOrder);

                //TODO: Kald til ShipItems ligesom ved ReserveItems ovenover
            }
            else
            {
                // Opdaterer status til InsufficientInventory (3)
                newOrder = newOrder with { Status = OrderStatusDto.InsufficientInventory };

                // Sender notifikation om at der ikke er nok på lager
                await context.CallActivityAsync(
                    nameof(NotifyActivity),
                    new NotificationDto($"Order {order.ShortId} has insufficient inventory.", newOrder));

                // Gemmer den opdaterede ordre(status) i state
                await context.CallActivityAsync(
                    nameof(CreateOrderActivity), newOrder);
            }
        }
        else
        {
            /* TODO: Burde det ikke være PaymentFailing (5) her? 
             * Hvornår bruges error (8) egentligt? 
             * I en try catch? */

            // Opdaterer status til Error (8)
            newOrder = newOrder with { Status = OrderStatusDto.Error };

            // Gemmer den opdaterede ordre(status) i state
            await context.CallActivityAsync(
                nameof(CreateOrderActivity), newOrder);
        }

        // Lige meget hvad, så sendes ordren tilbage med en given status
        // TODO: er status ikke fra state???
        return new OrderResultDto(newOrder.Status, newOrder);
    }
}