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
    }

    public void Update(string key, string value)
        {
        if (!Data.ContainsKey(key))
        {
            Data[key] = "0";
            Console.WriteLine("entry added");
        }

        // Direct assignment only if the value isn't an operation
        if (value.Length > 0 && !"+-*/".Contains(value[0]) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double directValue))
        {
            Data[key] = directValue.ToString(CultureInfo.InvariantCulture);
            return;
        }

        // Parse operation, e.g. +3, -2, *4, /2
        if (value.Length < 2 || !double.TryParse(value.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out double operand))
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
        var match = Regex.Match(value, @"^\s*(\w+|-?\d+(?:\.\d+)?)\s*(<=|>=|==|!=|<|>)\s*(\w+|-?\d+(?:\.\d+)?)\s*$");

        string left = match.Groups[1].Value;
        string operation = match.Groups[2].Value;
        string right = match.Groups[3].Value;

        double GetValue(String operand)
        {
            if (double.TryParse(
            operand,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double number))
        {
            return number;
        }

        if (Data.TryGetValue(operand, out string value) &&
            double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out number))
        {
            return number;
        }

        throw new ArgumentException(
            $"Unknown variable or invalid number: {operand}");
        }
        double a = GetValue(left);
        double b = GetValue(right);

        return operation switch
        {
            "<"  => a < b,
            ">"  => a > b,
            "<=" => a <= b,
            ">=" => a >= b,
            "==" => a == b,
            "!=" => a != b,
            _ => false
        };
    }


}