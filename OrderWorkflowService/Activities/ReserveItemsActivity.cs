using Dapr.Workflow;
using Workflow.Shared.Dtos;

namespace OrderWorkflowService.Activities
{
    public class ReserveItemsActivity : WorkflowActivity<InventoryRequestDto, InventoryResultDto>
    {
        readonly StateManagement _stateManagement;

        public ReserveItemsActivity(StateManagement stateManagement)
        {
            _stateManagement = stateManagement;
        }

        public override async Task<InventoryResultDto> RunAsync(WorkflowActivityContext context, InventoryRequestDto input)
        {
            var pizzaToCheck = input.PizzasRequested.Select(p => p.PizzaType).ToList();

            var pizzaInStock = await _stateManagement.GetItemsAsync(
                            input.PizzasRequested.Select(p => p.PizzaType).ToArray());

            bool isSufficientInventory = true;

            foreach (var item in pizzaInStock)
            {
                if (isSufficientInventory && (item.Quantity < input.PizzasRequested
                            .First(p => p.PizzaType == item.PizzaType)
                            .Quantity))
                {
                    isSufficientInventory = false;
                }
            }
            return new InventoryResultDto(isSufficientInventory, pizzaInStock.ToArray());
        }
    }
}
