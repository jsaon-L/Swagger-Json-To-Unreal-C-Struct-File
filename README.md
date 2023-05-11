# SUSU
SUSU - Swagger.json to Unreal Struct Utility

## What is this

This is a quick and dirty hack solution to generate Unreal USTRUCTS based on swagger.json Open API specification for API servers written in C#

Because the Unreal WebAPI plugin is experimental, generates very complicated code and sometimes just doesn't work, or forgets to map many members of the DTOs, I have decided to forgo using it, instead just writing API requests using the HttpModule and some generic functions.

However, I realized having to write all the parameters and responses by hand can get quite tedious, so I created this simple utility to parse a given *swagger.json* file and generate unreal struct header files.

YMMV - this is only tested for my specific use cases. 

This software comes with no warranty and you may use it at your own risk.

## Usage
### For Help
    SUS.exe -h 
### Example
    SUS.exe -i path/to/swagger.json -o folder/to/output/headers/to -e GAME_API
    
### Options
-i --input input file path

-o --output output folder path

-e --export module api specifier to place between the struct and struct name i.e struct GAME_API MyStruct 

-s --single place all the struct files in a single Models.h file in the target folder ( without this option the utility will generate one header file per struct )

-c --clear *DANGER!* deletes all files in the folder before output, use with caution

