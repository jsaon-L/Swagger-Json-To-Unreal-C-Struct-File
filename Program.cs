using System.CommandLine;
using System.CommandLine.Invocation;
using System.Security.Principal;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using SUS;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Welcome to SUSU (Swagger.json to Unreal Struct utility v0.1)");

foreach (var arg in args)
{
    Console.WriteLine(arg);
}

RootCommand rootCommand = new RootCommand("Because the WebAPI unreal plugin is still experimental and generates some buggy code / unmapped variable, a simple solution is needed to automate the generation of structs matching the API parameters, while leaving the HTTP code to be implemented with simple HttpModule calls");
rootCommand.TreatUnmatchedTokensAsErrors = false;

var inputOption = new Option<FileInfo>(new string[] { "--input", "-i" }, "Path of the swagger.json file to generate structs based on");

rootCommand.AddOption(inputOption);

var outputOption = new Option<string>(new string[] { "--output", "-o" }, "Output folder path");

rootCommand.AddOption(outputOption);

var singleFileOption = new Option<bool>(new string[] { "--single", "-s" }, 
    description: "Use a single file Models.h for output")
{
    AllowMultipleArgumentsPerToken = true,
    Arity = ArgumentArity.Zero,
};

rootCommand.AddOption(singleFileOption);

var clearOutputOption = new Option<bool>(
    new string[] {"--clear", "-c"}, "Clear output folder before generating, WARNING this may be dangerous")
{
    AllowMultipleArgumentsPerToken = true,
    Arity = ArgumentArity.Zero,
};

rootCommand.AddOption(clearOutputOption);

Action<FileInfo, string, bool, bool> handler = (input, output, single, clearOutput) => Generate(input, output, single, clearOutput);

rootCommand.SetHandler(handler,inputOption,outputOption,singleFileOption, clearOutputOption);

return rootCommand.InvokeAsync(args).Result;



static void Generate(FileInfo input, string outputPath, bool singleFile, bool clear)
{
    Console.WriteLine($"singleFile: {singleFile} clear: {clear}");
    if (input == null)
    {
        Console.Error.WriteLine("Invalid input file, use -h or --help for help");
        return;
    }

    if (!input.Exists)
    {
        Console.Error.WriteLine($"Could not open file at location {input.FullName}");
    }

    if (!Directory.Exists(outputPath))
    {
        Directory.CreateDirectory(outputPath);
    }
    
    if (!Directory.Exists(outputPath))
    {
        Console.Error.WriteLine("Invalid output directory, could not create");
    }

    if (clear)
    {
        Console.WriteLine("Clearing");
        foreach (var filePath in Directory.GetFiles(outputPath))
        {
            if (File.Exists(filePath) && filePath.EndsWith(".h"))
            {
                Console.WriteLine($"Deleting {filePath}");
                File.Delete(filePath);
            }
        }
    }


    List<ModelDefinition> Models = new List<ModelDefinition>();
    
    using (var fstream = new FileStream(input.FullName, FileMode.Open))
    {
        var openApiDocument = new OpenApiStreamReader().Read(fstream, out var diagnostic);

        foreach (var reqBody in openApiDocument.Components.Schemas)
        {
            //Console.WriteLine($"name: {reqBody.Key}, body: {reqBody.Value.ToString()}");
            
            Utils.RegisterKnownType(reqBody.Key);

            if (reqBody.Value.Enum.Count > 0)
            {
                continue;
            }
            
            var currentModel = new ModelDefinition()
            {
                ModelName = reqBody.Key,
                Properties = new List<PropDef>()
            };
            Models.Add(currentModel);
            
            foreach (var prop in reqBody.Value.Properties)
            {
                //Console.WriteLine($"prop: {prop.Key}, Type: {prop.Value.Type}, Format: {prop.Value.Format}");
                var newProp = new PropDef()
                {
                    Name = prop.Key,
                    DataType = prop.Value.Type,
                    StructType = Utils.DataTypeToStructType(prop.Value)
                };
                currentModel.Properties.Add(newProp);
                Console.WriteLine($"Parsed property {prop.Key} of type {newProp.StructType} for struct {currentModel.ModelName}");
            }
        }
    }
    
    Console.WriteLine("Finished OpenAPI doc pass");
    Console.WriteLine($"Writing model files in folder {Path.GetFullPath(outputPath)}");
    

    if (singleFile)
    {
        var modelPath = Path.Combine(outputPath, "Models.h");
        
        using(var fstream = File.Open(modelPath, FileMode.Create))
        {
            using (var writer = new StreamWriter(fstream, Encoding.UTF8))
            {
                foreach (var model in Models)
                {
                    writer.WriteLine(model.ToUstruct());   
                }    
            }
        }
    }
    else
    {
        foreach (var model in Models)
        {
            var modelPath = Path.Combine(outputPath, model.ModelName + ".h");
        
            using(var fstream = File.Open(modelPath, FileMode.Create))
            {
                using (var writer = new StreamWriter(fstream, Encoding.UTF8))
                {
                    writer.WriteLine(model.ToUstruct());
                }
            }
        }    
    }
}


    

