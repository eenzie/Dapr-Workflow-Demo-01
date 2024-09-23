using Dapr.Workflow;
using OrderWorkflowService.Activities;
using OrderWorkflowService.Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Dapr Client Service
builder.Services.AddDaprClient();

// Add Dapr Workflow
builder.Services.AddDaprWorkflow(options =>
{
    // Register workflows
    options.RegisterWorkflow<OrderWorkflow>();

    // Register activities
    options.RegisterActivity<NotifyActivity>();
    options.RegisterActivity<CreateOrderActivity>();
    options.RegisterActivity<ProcessPaymentActivity>();
    options.RegisterActivity<ReserveItemsActivity>();
    options.RegisterActivity<ShipItemsActivity>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
