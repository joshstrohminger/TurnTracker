using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TurnTracker.Data
{
    public static class ValueConverterExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        {
            var options = (JsonSerializerOptions)null;
            var converter = new ValueConverter<T, string>(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<T>(v, options));

            var comparer = new ValueComparer<T>(
                (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
                v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
                v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, options), options));

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }
    }
}