using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApp.Infrastructure;

public class InvariantDoubleModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var raw = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName).FirstValue;

        if (string.IsNullOrWhiteSpace(raw))
        {
            bindingContext.Result = ModelBindingResult.Success(0.0);
            return Task.CompletedTask;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"The value '{raw}' is not a valid number.");
        }

        return Task.CompletedTask;
    }
}

public class InvariantDoubleModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(double) ||
            context.Metadata.ModelType == typeof(double?))
            return new InvariantDoubleModelBinder();

        return null;
    }
}
