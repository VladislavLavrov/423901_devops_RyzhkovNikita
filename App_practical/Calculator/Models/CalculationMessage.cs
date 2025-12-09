using System.Text.Json;
using System.Text.Json.Serialization;

namespace Calculator.Models
{
    public class CalculationMessage
    {
        public string Operation { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double Operand1 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double? Operand2 { get; set; }

        public double Result { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        public static CalculationMessage? FromJson(string json)
        {
            return JsonSerializer.Deserialize<CalculationMessage>(json);
        }
    }
}
