### Api Gateway Action Filters

The library provides interfaces your Gateway API can implement to hook into the endpoint action filters.

Here you can do things like **validation**, **rate limiting**, **logging** etc, if you want.

In your Gateway API project,

### you can hook into a common action filter by implementing the below interface.

```C#
Task OnActionExecutionAsync(ActionExecutingContext context, string api, string key, string verb);
```

See **ActionExecutingContext** [here](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.filters.actionexecutingcontext?view=aspnetcore-6.0).

### Example

In your Gateway API project,

*	Create a service like below

```C#
    public class ValidationActionFilterService : IGatewayActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, string api, string key, string verb)
        {
            //do your validation here

            //set the result, eg below commented line
            //context.Result = new BadRequestObjectResult(context.ModelState);

            await Task.CompletedTask;
        }
    }
```

*	Wire it up for dependency injection in Startup.cs

```C#
services.AddScoped<IGatewayActionFilter, ValidationActionFilterService>();
.
.
services.AddApiGateway();
```

### you can hook into each verb's endpoint action filter by implementing the below interfaces.

```C#
Task OnActionExecutionAsync(ActionExecutingContext context, string api, string key);
```

### GET / HEAD

*	IGetOrHeadGatewayActionFilter

### POST

*	IPostGatewayActionFilter

### PUT

*	IPutGatewayActionFilter

### PATCH

*	IPatchGatewayActionFilter

### DELETE

*	IDeleteGatewayActionFilter

### GET Orchestration

*	IGetOrchestrationGatewayActionFilter

### POST Hub

*	IHubPostGatewayActionFilter


### Example

In your Gateway API project,

*	Create a service like below

```C#
    public class PostValidationActionFilterService : IPostGatewayActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, string api, string key)
        {
            //do your validation here

            //set the result, eg below commented line
            //context.Result = new BadRequestObjectResult(context.ModelState);

            await Task.CompletedTask;
        }
    }
```

*	Wire it up for dependency injection in Startup.cs

```C#
services.AddScoped<IPostGatewayActionFilter, PostValidationActionFilterService>();
.
.
services.AddApiGateway();
```