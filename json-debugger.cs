using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class JsonDebugger
{
    private readonly JToken _jsonData;
    private readonly string _rawJson;

    public JsonDebugger(string jsonContent)
    {
        _rawJson = jsonContent;
        try
        {
            _jsonData = JToken.Parse(jsonContent);
            Console.WriteLine("JSON parsed successfully!\n");
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"Invalid JSON: {ex.Message}");
            throw;
        }
    }

    public void ExploreStructure(string path = "$", int depth = 0)
    {
        var token = string.IsNullOrEmpty(path) || path == "$" 
            ? _jsonData 
            : _jsonData.SelectToken(path);

        if (token == null)
        {
            Console.WriteLine("Path not found!");
            return;
        }

        switch (token.Type)
        {
            case JTokenType.Object:
                foreach (var property in token.Children<JProperty>())
                {
                    PrintIndented($"{property.Name}: {GetTypeDescription(property.Value)}", depth);
                    if (depth < 3) // Limit recursion depth
                        ExploreStructure($"{path}.{property.Name}", depth + 1);
                }
                break;

            case JTokenType.Array:
                PrintIndented($"Array with {token.Count()} items", depth);
                if (token.Any() && depth < 3)
                {
                    ExploreStructure($"{path}[0]", depth + 1); // Show structure of first item
                }
                break;

            default:
                PrintIndented($"Value: {token}", depth);
                break;
        }
    }

    public void TryJsonPath(string jsonPath)
    {
        try
        {
            var results = _jsonData.SelectTokens(jsonPath).ToList();
            Console.WriteLine($"\nFound {results.Count} matches:");
            
            foreach (var result in results.Take(5)) // Limit output
            {
                Console.WriteLine("\nMatch:");
                Console.WriteLine(result.ToString(Formatting.Indented));
            }

            if (results.Count > 5)
            {
                Console.WriteLine($"\n... and {results.Count - 5} more matches");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error in JSON path: {ex.Message}");
        }
    }

    public void ShowCommonPatterns()
    {
        Console.WriteLine("Common JSONPath Patterns:");
        Console.WriteLine("1. Direct property access: $.propertyName");
        Console.WriteLine("2. Nested property: $.parent.child");
        Console.WriteLine("3. Array access: $.items[0]");
        Console.WriteLine("4. All array items: $.items[*]");
        Console.WriteLine("5. Array filter: $.items[?(@.price > 10)]");
        Console.WriteLine("6. Multiple paths: $..price");
        Console.WriteLine("7. Array slice: $.items[0:3]");
        Console.WriteLine("8. Complex filter: $.items[?(@.price > 10 && @.category == 'books')]");
    }

    private string GetTypeDescription(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => "Object",
            JTokenType.Array => $"Array ({token.Count()} items)",
            JTokenType.Integer => "Number",
            JTokenType.Float => "Decimal",
            JTokenType.String => "String",
            JTokenType.Boolean => "Boolean",
            JTokenType.Null => "Null",
            _ => token.Type.ToString()
        };
    }

    private void PrintIndented(string message, int depth)
    {
        Console.WriteLine($"{new string(' ', depth * 2)}{message}");
    }
}

// Usage example:
public class Program
{
    public static void Main()
    {
        var json = @"{
            'store': {
                'books': [
                    {
                        'title': 'Book 1',
                        'price': 12.99,
                        'categories': ['fiction', 'mystery']
                    },
                    {
                        'title': 'Book 2',
                        'price': 8.99,
                        'categories': ['non-fiction']
                    }
                ],
                'electronics': [
                    {
                        'name': 'Laptop',
                        'price': 999.99
                    }
                ]
            }
        }";

        var debugger = new JsonDebugger(json);

        // Show structure
        Console.WriteLine("JSON Structure:");
        debugger.ExploreStructure();

        // Show common patterns
        Console.WriteLine("\nCommon Patterns:");
        debugger.ShowCommonPatterns();

        // Try some queries
        Console.WriteLine("\nTrying some queries:");
        debugger.TryJsonPath("$..price");  // Find all prices
        debugger.TryJsonPath("$.store.books[?(@.price < 10)]");  // Books under $10
        debugger.TryJsonPath("$.store.books[*].categories[*]");  // All categories
    }
}
