using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MusicStreamingService.Binders;

public class JsonFormBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        if (string.IsNullOrWhiteSpace(value))
            return Task.CompletedTask;

        try
        {
            var model = JsonSerializer.Deserialize(value, bindingContext.ModelType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            bindingContext.Result = ModelBindingResult.Success(model);
        }
        catch (JsonException ex)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
        }

        return Task.CompletedTask;
    }
}