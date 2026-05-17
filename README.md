# Steam Game Hardware Checker
## Description
This is a simple desktop app that checks if your computer meets the requirements to run a given game on Steam. It uses the Steam API to fetch the system requirements for a game and compares them with your computer's hardware.

## How to Use
The app will gather your hardware information and display some basic info to you. You can either search up a game by name or copy and paste the game's steam store URL. 

It will return the results of whether your computer meets the minimum and recommended requirements for that game. It will also provide details on which specific requirements you do or do not meet.

Note: Steam game requirements do not have a standardized format beyond basic hardware sections. The listed requirements are parsed as best as possible, but may not be perfectly accurate for all games.

## Technologies Used
* WPF based desktop application - I just wanted to learn WPF and practice C#.
* Steam API - Used to fetch game information and requirements.
* [CsvHelper](https://joshclose.github.io/CsvHelper/)
* [FuzzySharp](https://github.com/JakeBayer/FuzzySharp)
* [Hardware.Info](https://www.nuget.org/packages/Hardware.Info)
* [Newtonsoft.Json](https://www.newtonsoft.com/json)
* [WPF-UI](https://github.com/lepoco/wpfui)
* [Blender CPU bechmarks](https://opendata.blender.org/benchmarks/query/?group_by=device_name&blender_version=5.1.1)
* [DBGPU benchmarks](https://github.com/painebenjamin/dbgpu)
## Future Improvements
Allow users to input the game requirements manually.
