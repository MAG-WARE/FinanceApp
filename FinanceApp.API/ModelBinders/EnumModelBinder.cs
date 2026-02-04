using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FinanceApp.API.ModelBinders;

public class EnumModelBinder<T> : IModelBinder where T : struct, Enum
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        // Try to parse as integer first
        if (int.TryParse(value, out int intValue))
        {
            if (Enum.IsDefined(typeof(T), intValue))
            {
                bindingContext.Result = ModelBindingResult.Success((T)(object)intValue);
                return Task.CompletedTask;
            }
        }

        // Try to parse as enum name
        if (Enum.TryParse<T>(value, ignoreCase: true, out T enumValue))
        {
            bindingContext.Result = ModelBindingResult.Success(enumValue);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid value '{value}' for enum {typeof(T).Name}");
        return Task.CompletedTask;
    }
}

public class EnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType.IsEnum)
        {
            var binderType = typeof(EnumModelBinder<>).MakeGenericType(context.Metadata.ModelType);
            return (IModelBinder)Activator.CreateInstance(binderType)!;
        }

        // Handle nullable enums
        var nullableType = Nullable.GetUnderlyingType(context.Metadata.ModelType);
        if (nullableType != null && nullableType.IsEnum)
        {
            var binderType = typeof(EnumModelBinder<>).MakeGenericType(nullableType);
            return (IModelBinder)Activator.CreateInstance(binderType)!;
        }

        return null;
    }
}
