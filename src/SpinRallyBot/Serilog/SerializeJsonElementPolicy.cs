using System.Text.Json;
using Serilog.Core;
using Serilog.Events;

namespace SpinRallyBot.Serilog;

internal class SerializeJsonElementPolicy : IDestructuringPolicy {
    public bool TryDestructure(
        object value,
        ILogEventPropertyValueFactory propertyValueFactory,
        out LogEventPropertyValue result) {
        if (value is not JsonElement jsonElement) {
            result = null!;
            return false;
        }

        result = new ScalarValue(jsonElement);
        return true;
    }
}
