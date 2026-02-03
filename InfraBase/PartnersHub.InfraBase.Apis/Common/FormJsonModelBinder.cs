using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PartnersHub.InfraBase.Apis.Common;

public sealed class FormJsonModelBinder : IModelBinder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new NullableGuidConverter(),
            new GuidConverter()
        }
    };

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var rawValue = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        try
        {
            var model = JsonSerializer.Deserialize(rawValue, bindingContext.ModelType, JsonOptions);
            if (model == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(model);
        }
        catch (JsonException ex)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}
