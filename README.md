# Project Analyzer

Project Analyzer is a lightweight CLI tool that analyzes C# projects and
provides useful insights such as file counts, keyword usage, and general
project metrics.

## Installation

Install globally using .NET:

    dotnet tool install -g ProjectAnalyzer.Tool

## Usage

Run inside a project directory:

    projectanalyzer

Or specify a project path:

    projectanalyzer C:\Projects\MyApp

## Features

-   Analyze C# project structure
-   Count files and lines of code
-   Detect keywords and patterns
-   Generate simple project reports

## Example Output

    Project: MyApp
    Files analyzed: 42
    Lines of code: 12873
    Classes: 31
    Methods: 205

## Requirements

-   .NET 8 or later

## Author

Yajnesh Ramdonee

## License

This project is licensed under the MIT License.
