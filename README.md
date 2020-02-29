# GameLauncherDock
The game launcher dock is a simple program which will scan the system for video games and store them in a JSON file. The program will allow the user to launch any of their games from a single location - without having to store the icons on the desktop, or launching a dedicated client such as Steam or Uplay. The program finds the games by scanning the registry on read-only mode. The following platforms are supported:
- Steam,
- GOG,
- Uplay,
- Origin,
-	Epic,
-	Bethesda,
-	Battlenet

The program will organise the games per platform, for convinient access, but it will also create a supergroup called "All" which will contain all stored games. Games which do not match the supported platforms will be stored in a group called "Custom" - this will allow you to manually edit the JSON file adding games which are not part of any platform (for example, games installed from disk). Of course, nothing stops you from adding non-video game programs to the JSON file.

## Console
At the moment only a console version of this program exists, a GUI version will be developed once the console version is 100% finished and tested (to make sure all back-end logic is working correctly). The console application allows the user to navigate the menu using arrows and select an option using enter (the program also supports a range of functions, using different keys), so there is no need for typing commands (this is also a planned feature).

At this date (1st March 2020), the console application perfectly suitable for use but some features, such as setting favourites, have not been implemented. I'd say that the version of the console app is v0.5, and I expect to get it finished by end of this year.

## GUI
GUI version will be in development after the console application reaches v1.0. This might change at any point if I decide that the back-end logic code is fully capable of being ported to a gui framework. 

## TODOs: list of stuff that I need to implement
- Add support for any missing platforms, such as Rockstar;
- Add support for favourites - setting and displaying;
- Add support for written commands and switching between navigation and writing - something like vim;
- Improve the way JSON document is updated;
- Create a GUI version of the application;
