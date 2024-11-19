using Microsoft.OpenApi.Models;

namespace SUS;


public static class Utils
{

    public static HashSet<string> KnownTypes { get; set; } = new HashSet<string>();

    public static void RegisterKnownType(string type)
    {
        KnownTypes.Add(type);
    }

    public static string DataTypeToStructType(OpenApiSchema prop, ModelDefinition model)
    {
        if (prop.Type == "object")
        {
            //fix null
            if (prop.Reference != null)
            {
                model.DependsOn.Add(prop.Reference.Id);
                if (KnownTypes.Contains(prop.Reference.Id))
                {
                    return "F" + prop.Reference.Id;
                }
                else
                {
                    Console.WriteLine("Encountered type reference before type definition, shouldn't be a problem");
                    return "F" + prop.Reference.Id;
                }
            }
        }

        if (prop.Type == "array")
        {
            return $"TArray<{DataTypeToStructType(prop.Items, model)}>";
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
        if (prop.Type == "float" || (prop.Type == "number" && prop.Format == "float"))
        {
            return "float";
        }

        //in my project number is string type
        if (prop.Type == "number")
        {
            return "FString";
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
    public string StructType { get; set; }
}

public class ModelDefinition
{
    public HashSet<string> DependsOn { get; set; }
    public string ModelName { get; set; }
    public List<PropDef> Properties { get; set; }
    public string API { get; set; }

    public string ToUstruct()
    {
        string ustructDefinition = $"USTRUCT(BlueprintType)\n" +
                                   $"struct {API} F{ModelName}\n" +
                                   $"{{\n" +
                                   $"\tGENERATED_BODY()\n" +
                                   $"\n";
        foreach (var prop in Properties)
        {
            if (prop.Name == "transMap" || prop.StructType == "UNKOWN")
            {
                ustructDefinition += $"\t// UPROPERTY(EditAnywhere, BlueprintReadWrite, Category=\"StructData\")\r\n\n" +
                    $"\t// {prop.StructType} {prop.Name};\n" +
                    $"\n";
            }
            else
            {
                ustructDefinition += $"\tUPROPERTY(EditAnywhere, BlueprintReadWrite, Category=\"StructData\")\r\n\n" +
                    $"\t{prop.StructType} {prop.Name};\n" +
                    $"\n";
            }


        }

        ustructDefinition += "};\n\n";

        return ustructDefinition;
    }
}