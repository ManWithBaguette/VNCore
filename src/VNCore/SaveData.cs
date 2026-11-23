using System.Text.RegularExpressions;
using System.Globalization;
using System.Text.Json;

namespace VNCore;

public class SaveData
{
    public Dictionary<string, string> Data { get; set; } = new();

    public void Update(Dictionary<string, string> newValues)
    {
        foreach (var item in newValues)
        {
            Update(item.Key, item.Value);
        }
        Console.WriteLine("Data updated");
        if (Data.ContainsKey("anger"))
        {
            Console.WriteLine($"New anger value: {Data["anger"]}");
        }
        if (Data.ContainsKey("x"))
        {
            Console.WriteLine($"New x value: {Data["x"]}");
        }
    }

    public void Update(string key, string value)
    {
        value = value.Trim();

        if (!Data.ContainsKey(key))
        {
            Data[key] = "0";
            Console.WriteLine("entry added");
        }

        // Direct assignment
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double directValue))
        {
            Data[key] = directValue.ToString(CultureInfo.InvariantCulture);
            return;
        }

        // String Assignment
        if (!"+-*/".Contains(value[0]))
        {
            Data[key] = value;
            return;
        }

        // Parse operation
        if (value.Length < 2 || !"+-*/".Contains(value[0]) || !double.TryParse(value.Substring(1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double operand))
        {
            Console.WriteLine("Invalid operation or value");
            return;
        }

        if (!double.TryParse(Data[key], NumberStyles.Float, CultureInfo.InvariantCulture, out double currentValue))
        {
            Console.WriteLine("Dictionary value is not numeric");
            return;
        }

        Console.WriteLine($"Key: {key}");
        Console.WriteLine($"Input: {value}");
        Console.WriteLine($"Current value: {currentValue}");
        Console.WriteLine($"Operand: {operand}");

        double result = value[0] switch
        {
            '+' => currentValue + operand,
            '-' => currentValue - operand,
            '*' => currentValue * operand,
            '/' when operand != 0 => currentValue / operand,
            '/' => throw new DivideByZeroException(),
            _ => throw new ArgumentException($"Unknown operation: {value[0]}")
        };
        Console.WriteLine($"In update: {result}");
        Data[key] = result.ToString(CultureInfo.InvariantCulture);
        Console.WriteLine("Data updated");
    }

    public Boolean Compare(string value)
    {   
        var match = Regex.Match(value, @"^\s*(\w+|[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)\s*(<=|>=|==|!=|<|>)\s*(\w+|[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?)\s*$");

        if (!match.Success)
        {
            throw new ArgumentException($"Invalid comparison expression: {value}");
        }

        string left = match.Groups[1].Value;
        string operation = match.Groups[2].Value;
        string right = match.Groups[3].Value;

        string GetValue(String operand)
        {
            if (Data.TryGetValue(operand, out string? storedValue))
            {
                return storedValue;
            }

            return operand;
        }

        string a = GetValue(left);
        string b = GetValue(right);

        bool aNumeric = double.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, out double aNumber);
        bool bNumeric = double.TryParse(b, NumberStyles.Float, CultureInfo.InvariantCulture, out double bNumber);

        // Numeric comparison
        if (aNumeric && bNumeric)
        {
            return operation switch
            {
                "<"  => aNumber < bNumber,
                ">"  => aNumber > bNumber,
                "<=" => aNumber <= bNumber,
                ">=" => aNumber >= bNumber,
                "==" => aNumber == bNumber,
                "!=" => aNumber != bNumber,
                _ => false
            };
        }

        // String comparison
        return operation switch
        {
            "==" => a == b,
            "!=" => a != b,
            _ => throw new ArgumentException("Strings only support == and != comparisons.")
        };
    }


}