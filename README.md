# GameHub
A simple program which will scan the system for video games and store them in a JSON file. The program will allow the user to launch any of their games from a single location - without having to store the icons on the desktop, or launching a dedicated client such as Steam or Uplay. The program finds the games by scanning the registry in read-only mode. The program supports following platforms:
- Steam,
- GOG,
- Uplay,
- Origin,
-	Epic,
-	Bethesda,
-	Battlenet 

## Console

![](GLConsole.gif)

At the moment only a console version of this program exists. The console allows the user to navigate the menu using arrows and select an option using enter (the program also supports a range of functions, using different keys), so there is no need for typing commands (this is also a planned feature).

Console version has the following features:
- Support for the aforementioned platforms;
- Support for flagging games as favourites;
- Support for manually adding game binaries and shortcuts;

In order to manually add games and programs, place executables (.exe) or file shortcuts into the "customGames" folder, found in the same directory as the application and choose the rescan option to load them into the program. It is recommended to use file shortcuts instead as executables often require external assets and will not work properly.

## GUI
GUI version might be developed in the future. 

## Building / using
- Download the executable from one of the releases.
- Clone the repo and build. The program uses following nuGet packages:
-- System.Text.Json for JSON parsing;
-- Costure.Fody for merging the binary file with dlls.

The app does not need instalation, simply download the exe and run - I would recommend putting the executable in a folder, so that the JSON file, the log file and the custom game folder will be contained and out of the way.


## Contributing
You can support the project in the following ways:
- Fork the repo, make some changes and submit a PR;
- Identify and raise bugs and issues;
- Raise suggestions;
