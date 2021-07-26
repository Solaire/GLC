# GLC (GameLauncher Console)

This is a simple console program that will scan the system for video games, then allow the user to quickly launch any of these games from a single location, without having to store the icons on the desktop or to remember which client (Steam or Epic, etc.) to launch. The program currently supports the following platforms:
- Amazon Games;
- Battle&period;net;
- Bethesda&period;net Launcher;
- Big Fish Games;
- Epic Games Launcher;
- GOG Galaxy;
- Indiegala Client;
- itch;
- Origin (soon to be EA Desktop);
- Paradox Launcher;
- Steam;
- Ubisoft Connect (formerly Uplay).

You can also use it to launch custom programs (see [below](#Using) for instructions).

<br/>

![](GLConsole.gif)

<br/>

## Features
An interactive console allows the user to navigate via menus using the arrows and Enter (by default). An in-app help (H or F1 by default) provides a list of other keys for other functions, e.g.:
- Launch a game or platform client;
- Search for games;
- Flag favourites;
- Create a Desktop shortcut, or export a group of shortcuts to a given location;
- Uninstall a game;
  - Uninstall not currently available for some platforms (e.g., Epic, Indiagala, itch).
- Change between grid/list mode;
- Change between light/dark mode.
  - Colours can be further customised; see below.

## Using
***[Click here to download the latest binary release.](../../releases/latest/download/glc.exe)*** The app is portable (does not require installation). However, we'd recommend moving it to a dedicated folder before running it because several support files and folders are automatically created in the same location.

You can edit glc.ini (or glc-cfg.json in v1.1) to change the default configuration options, including custom keys, colours, text, input, layout, sorting, images, etc.

To manually add programs, place file shortcuts or .exe executables in the ".\customGames" folder found in the same directory as the application, and use the rescan feature to load them into the program. It is recommended to use shortcuts instead of executables, as .exe files often require external assets and therefore may not work properly.

To customise an image, place files with the same titles in the ".\customImages" folder (supports ICO, BMP, PNG, GIF, JPEG, and TIFF formats).

## [GLC Wiki](../../wiki)
***See the [wiki](../../wiki) for [Known Issues](../../wiki/Known-Issues), [Changelog](../../wiki/Changelog), and more information.***

## Building from source
Clone the repo and build. The program uses following nuGet packages:
- System.Text.Json for JSON parsing;
- System.Data.SQLite for database parsing (Amazon and itch);
- HtmlAgilityPack for HTML parsing (Steam not-installed games);
- Costura.Fody for merging the binary file with dlls.

## Contributing
You can support the project in the following ways:
- Go to [Issues](/Solaire/GLC/issues) to identify bugs or make suggestions;
- Fork the repo, make some changes, and submit a pull request.

## [License](LICENSE)
GNU General Public License v3.0

See [LICENSE](LICENSE) file.
