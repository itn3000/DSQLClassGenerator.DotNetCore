# DSQLClassGenerator

C# class generator for DeclarativeSQL( https://github.com/xin9le/DeclarativeSql )

## Prerequisits

* [dotnet-cli](https://www.microsoft.com/net/core#windows)
* Target Database
    * SQL Server and PostgreSQL are currently supported
* Visual Studio 2015(community is ok)
    * for compile

## Usage

1. run ```[solution dir]/build.bat```
2. create new project with ```dotnet new```
3. create NuGet.config and add ```[solution dir]/build/``` to packageSources entry
4. do ```dotnet restore```
5. run command

## Commandline options

Basic usage:`dotnet dsqlclassgenerator [options]`
options are:
* `-a` or `--amalgamation`
    * generate classes into one file
    * default false
* `-d [dirpath]` or `--outputdir=[dirpath]`
    * output directory for separete files
    * default is `out`
    * if `-a` is specified,just ignored this option
* `-f [filepath]` or `--outputfile=[filepath]`
    * output file path for amalgamated file
    * default is `TableDefinition.cs`
* `-n [namespace]` or `--namespace=[namespace]`
    * generated class namespace
    * default is `Example`
* `-s` or `noschema`
    * flag for ignore schema
    * when it's on,`Schema` attribute will be omitted
* `-c [config filepath]` or `--config [filepath]`
    * external configuration file
* `-h` or `--help`
    * just print help then exit

## config file format

config file format is json,example is in src/DSQLClassGenerator.DotNet/dbsetting.json
