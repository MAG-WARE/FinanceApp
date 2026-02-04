using System.ComponentModel;
using System.Globalization;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Converters;

public class ViewContextTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            // Try to parse as integer first
            if (int.TryParse(stringValue, out int intValue))
            {
                if (Enum.IsDefined(typeof(ViewContext), intValue))
                {
                    return (ViewContext)intValue;
                }
            }

            // Try to parse as enum name
            if (Enum.TryParse<ViewContext>(stringValue, ignoreCase: true, out ViewContext enumValue))
            {
                return enumValue;
            }
        }

        return base.ConvertFrom(context, culture, value);
    }
}
