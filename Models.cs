using Microsoft.OpenApi.Models;

namespace SUS;


public static class Utils
{

    public static HashSet<string> KnownTypes { get; set; } = new HashSet<string>();

    public static void RegisterKnownType(string type)
    {
        KnownTypes.Add(type);
    }
    
    public static string DataTypeToStructType(OpenApiSchema prop)
    {
        if (prop.Type == "object")
        {
            if (KnownTypes.Contains(prop.Reference.Id))
            {
                return "F"+prop.Reference.Id;
            }
            else
            {
                Console.WriteLine("Encountered type reference before type definition, shouldn't be a problem");
                return "F"+prop.Reference.Id;
            }
        }

        if (prop.Type == "array")
        {
            return $"TArray<{DataTypeToStructType(prop.Items)}>";
        }
        
        if (prop.Type == "integer")
        {
            return "int";
        }

        if (prop.Type == "boolean")
        {
            return "bool";
        }
        
        //floats?
        if (prop.Type == "float")
        {
            return "float";
        }
        //strings
        if (prop.Type == "string" && string.IsNullOrWhiteSpace(prop.Format))
        {
            return "FString";
        }
        
        //date-time
        if (prop.Type == "string" && prop.Format == "date-time")
        {
            return "FDateTime";
        }

        return "UNKOWN";
    }
    
}

public class PropDef
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public string StructType { get; set;}
}

public class ModelDefinition
{
    public string ModelName { get; set; }
    public List<PropDef> Properties { get; set; }

    public string ToUstruct()
    {
        string ustructDefinition = $"USTRUCT(BlueprintType)\n" +
                                   $"struct F{ModelName}\n" +
                                   $"{{\n" +
                                   $"GENERATED_BODY()\n" +
                                   $"\n";
        foreach (var prop in Properties)
        {
            ustructDefinition += $"UPROPERTY(BlueprintReadWrite)\n" +
                                 $"{prop.StructType} {prop.Name};\n" +
                                 $"\n";
        }

        ustructDefinition += "};\n\n";

        return ustructDefinition;
    }
}