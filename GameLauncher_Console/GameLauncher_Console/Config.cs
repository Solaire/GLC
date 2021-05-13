using System;
using System.Diagnostics;

namespace GameLauncher_Console
{
	/// <summary>
	/// Contains the definition of the configuration data
	/// </summary>
	public class CConfig
	{
		// Config JSON field defaults
		public const string KEY_NONE		= "NoName";
		public const string COLBG_UNK1		= "Black";
		public const string COLBG_UNK2		= "White";
		public const string COLFG_UNK1		= "Gray";
		public const string COLFG_UNK2		= "Black";

		public const bool	DEF_NOQUIT		= false;
		public const bool	DEF_USEFILE		= false;
		public const bool	DEF_USESCAN		= false;
		public const bool	DEF_USECMD		= false;
		public const bool	DEF_USETYPE		= false;
		public const bool	DEF_USELIST		= false;
		public const bool	DEF_NOPAGE		= false;
		public const bool	DEF_USESIZE		= false;
		public const bool	DEF_USEALPH		= false;
		public const bool	DEF_USEFAVE		= false;
		public const bool	DEF_USELITE		= false;
		public const bool	DEF_USEALL		= false;
		public const bool	DEF_NOCFG		= true;
		public const bool	DEF_USECUST		= false;
		public const bool	DEF_USETEXT		= false;
		public const bool	DEF_IMGBORD		= false;
		public const bool	DEF_IMGCUST		= false;
		public const bool	DEF_IMGRTIO		= false;
		public const int	DEF_ICONSIZE	= 0; //= 2;
		public const int	DEF_ICONRES		= 16;
		public const int	DEF_IMGSIZE		= 16;
		public const int	DEF_IMGRES		= 96;
		public const int	DEF_IMGPOS		= 10;
		public const int	DEF_COLSIZE		= 25;
		public const string DEF_COLBG1		= "Black";
		public const string DEF_COLBG2		= "White";
		public const string DEF_COLTITLE1	= "White";
		public const string DEF_COLTITLE2	= "DarkBlue";
		public const string DEF_COLSUB1		= "DarkGray";
		public const string DEF_COLSUB2		= "Gray";
		public const string DEF_COLENTRY1	= "Gray";
		public const string DEF_COLENTRY2	= "Black";
		public const string DEF_COLHIBG1	= "DarkGray";
		public const string DEF_COLHIBG2	= "Blue";
		public const string DEF_COLHIFG1	= "Red";
		public const string DEF_COLHIFG2	= "White";
		public const string DEF_COLERRBG1	= "Black";
		public const string DEF_COLERRBG2	= "White";
		public const string DEF_COLERRFG1	= "Yellow";
		public const string DEF_COLERRFG2	= "Red";
		public const string DEF_KEYLT1		= "LeftArrow";
		public const string DEF_KEYLT2		= "NumPad4";
		public const string DEF_KEYUP1		= "UpArrow";
		public const string DEF_KEYUP2		= "NumPad8";
		public const string DEF_KEYRT1		= "RightArrow";
		public const string DEF_KEYRT2		= "NumPad6";
		public const string DEF_KEYDN1		= "DownArrow";
		public const string DEF_KEYDN2		= "NumPad2";
		public const string DEF_KEYSEL1		= "Enter";
		public const string DEF_KEYSEL2		= "NumPad5";
		public const string DEF_KEYQUIT1	= "Escape";
		public const string DEF_KEYQUIT2	= "Q";
		public const string DEF_KEYSCAN1	= "F5";
		public const string DEF_KEYSCAN2	= "S";
		public const string DEF_KEYHELP1	= "F1";
		public const string DEF_KEYHELP2	= "H";
		public const string DEF_KEYBACK1	= "Backspace";
		public const string DEF_KEYBACK2	= "R";
		public const string DEF_KEYPGUP1	= "PageUp";
		public const string DEF_KEYPGUP2	= "NumPad9";
		public const string DEF_KEYPGDN1	= "PageDown";
		public const string DEF_KEYPGDN2	= "NumPad3";
		public const string DEF_KEYHOME1	= "Home";
		public const string DEF_KEYHOME2	= "NumPad7";
		public const string DEF_KEYEND1		= "End";
		public const string DEF_KEYEND2		= "NumPad1";
		public const string DEF_KEYFIND1	= "F3";
		public const string DEF_KEYFIND2	= "/";			// ToConsoleKeyFormat() converts to "Oem2"
		public const string DEF_KEYTAB1		= "Tab";
		public const string DEF_KEYTAB2		= KEY_NONE;
		public const string DEF_KEYESC1		= "Escape";
		public const string DEF_KEYESC2		= KEY_NONE;
		public const string DEF_KEYUNIN1	= "F9";
		public const string DEF_KEYUNIN2	= KEY_NONE;
		public const string DEF_KEYDESK1	= "F8";
		public const string DEF_KEYDESK2	= KEY_NONE;
		public const string DEF_KEYHIDE1	= "F10";
		public const string DEF_KEYHIDE2	= KEY_NONE;
		public const string DEF_KEYFAVE1	= "F12";
		public const string DEF_KEYFAVE2	= "F";
		public const string DEF_KEYALIAS1	= "F2";
		public const string DEF_KEYALIAS2	= "'";			// ToConsoleKeyFormat() converts to "Oem6"
		public const string DEF_KEYTYPE1	= @"\";         // ToConsoleKeyFormat() converts to "Oem5"
		public const string DEF_KEYTYPE2	= KEY_NONE;
		public const string DEF_KEYVIEW1	= "[";			// ToConsoleKeyFormat() converts to "Oem4"
		public const string DEF_KEYVIEW2	= "]";			// ToConsoleKeyFormat() converts to "Oem6"
		public const string DEF_KEYMODE1	= "F6";
		public const string DEF_KEYMODE2	= KEY_NONE;
		public const string DEF_KEYIMG1		= "F4";
		public const string DEF_KEYIMG2		= KEY_NONE;
		public const string DEF_KEYSORT1	= "F7";
		public const string DEF_KEYSORT2	= KEY_NONE;
		//									  0|-------|---------|---------|---------|40
		public const string DEF_TXTMAINT	= "Main Menu";
		public const string DEF_TXTCFGT		= "Settings";
		public const string DEF_TXTNMAIN1	= " Select a platform.";
		public const string DEF_TXTNMAIN2	= " Press [Q] to exit;\n" +
											  " Press [S] to rescan game collection;\n" +
											  " Press H for help.";
		public const string DEF_TXTNSUB1	= " Select a game.";
		public const string DEF_TXTNSUB2	= " Press [Q] to exit;\n" +
											  " Press [R] to return to previous menu;\n" +
											  " Press [F] to add/remove from favourites.";
		public const string DEF_TXTIMAIN1	= " Type name of a platform.";
		public const string DEF_TXTIMAIN2	= " Enter \'/exit\' to quit;\n" +
											  " Enter \'/help\' for more commands.";
		public const string DEF_TXTISUB1	= " Type title of a game.";
		public const string DEF_TXTISUB2	= " Enter \'/exit\' to quit;\n" +
											  " Enter \'/help\' for more commands.";

		public struct Configuration
		{
			public bool?  preventQuit;
			public bool?  ignoreChanges;
			public bool?  alwaysScan;
			public bool?  onlyCmdLine;
			public bool?  typeInput;
			public bool?  listOutput;
			public bool?  noPageSplit;
			public bool?  sizeToFit;
			public bool?  alphaSort;
			public bool?  faveSort;
			public bool?  lightMode;
			public bool?  goToAll;
			public bool?  hideSettings;
			public bool?  onlyCustom;
			public bool?  msgCustom;
			public bool?  imageBorder;
			public bool?  noImageCustom;
			public bool?  imageIgnoreRatio;
			public int?   iconSize;
			public int?   iconRes;
			public int?	  imageSize;
			public int?	  imageRes;
			public int?   imagePosition;
			public int?	  columnSize;
			public string bgCol;
			public string bgLtCol;
			public string titleCol;
			public string titleLtCol;
			public string subCol;
			public string subLtCol;
			public string entryCol;
			public string entryLtCol;
			public string highlightCol;
			public string highlightLtCol;
			public string highbgCol;
			public string highbgLtCol;
			public string errbgCol;
			public string errbgLtCol;
			public string errorCol;
			public string errorLtCol;
			public string leftKey1;
			public string leftKey2;
			public string upKey1;
			public string upKey2;
			public string rightKey1;
			public string rightKey2;
			public string downKey1;
			public string downKey2;
			public string selectKey1;
			public string selectKey2;
			public string quitKey1;
			public string quitKey2;
			public string scanKey1;
			public string scanKey2;
			public string helpKey1;
			public string helpKey2;
			public string backKey1;
			public string backKey2;
			public string pageUpKey1;
			public string pageUpKey2;
			public string pageDownKey1;
			public string pageDownKey2;
			public string firstKey1;
			public string firstKey2;
			public string lastKey1;
			public string lastKey2;
			public string searchKey1;
			public string searchKey2;
			public string completeKey1;
			public string completeKey2;
			public string cancelKey1;
			public string cancelKey2;
			public string uninstKey1;
			public string uninstKey2;
			public string desktopKey1;
			public string desktopKey2;
			public string hideKey1;
			public string hideKey2;
			public string faveKey1;
			public string faveKey2;
			public string aliasKey1;
			public string aliasKey2;
			public string typeKey1;
			public string typeKey2;
			public string viewKey1;
			public string viewKey2;
			public string modeKey1;
			public string modeKey2;
			public string imageKey1;
			public string imageKey2;
			public string sortKey1;
			public string sortKey2;
			public string mainTitle;
			public string settingsTitle;
			public string mainTextNav1;
			public string mainTextNav2;
			public string mainTextIns1;
			public string mainTextIns2;
			public string subTextNav1;
			public string subTextNav2;
			public string subTextIns1;
			public string subTextIns2;
		}
		public struct Hotkeys
        {
			public ConsoleKey leftCK1;
			public ConsoleKey leftCK2;
			public ConsoleKey upCK1;
			public ConsoleKey upCK2;
			public ConsoleKey rightCK1;
			public ConsoleKey rightCK2;
			public ConsoleKey downCK1;
			public ConsoleKey downCK2;
			public ConsoleKey quitCK1;
			public ConsoleKey quitCK2;
			public ConsoleKey selectCK1;
			public ConsoleKey selectCK2;
			public ConsoleKey scanCK1;
			public ConsoleKey scanCK2;
			public ConsoleKey helpCK1;
			public ConsoleKey helpCK2;
			public ConsoleKey backCK1;
			public ConsoleKey backCK2;
			public ConsoleKey pageUpCK1;
			public ConsoleKey pageUpCK2;
			public ConsoleKey pageDownCK1;
			public ConsoleKey pageDownCK2;
			public ConsoleKey firstCK1;
			public ConsoleKey firstCK2;
			public ConsoleKey lastCK1;
			public ConsoleKey lastCK2;
			public ConsoleKey searchCK1;
			public ConsoleKey searchCK2;
			public ConsoleKey completeCK1;
			public ConsoleKey completeCK2;
			public ConsoleKey cancelCK1;
			public ConsoleKey cancelCK2;
			public ConsoleKey uninstCK1;
			public ConsoleKey uninstCK2;
			public ConsoleKey desktopCK1;
			public ConsoleKey desktopCK2;
			public ConsoleKey hideCK1;
			public ConsoleKey hideCK2;
			public ConsoleKey faveCK1;
			public ConsoleKey faveCK2;
			public ConsoleKey aliasCK1;
			public ConsoleKey aliasCK2;
			public ConsoleKey typeCK1;
			public ConsoleKey typeCK2;
			public ConsoleKey viewCK1;
			public ConsoleKey viewCK2;
			public ConsoleKey modeCK1;
			public ConsoleKey modeCK2;
			public ConsoleKey imageCK1;
			public ConsoleKey imageCK2;
			public ConsoleKey sortCK1;
			public ConsoleKey sortCK2;
		}
		public struct Colours
        {
			public ConsoleColor bgCC;
			public ConsoleColor bgLtCC;
			public ConsoleColor titleCC;
			public ConsoleColor titleLtCC;
			public ConsoleColor subCC;
			public ConsoleColor subLtCC;
			public ConsoleColor entryCC;
			public ConsoleColor entryLtCC;
			public ConsoleColor highlightCC;
			public ConsoleColor highlightLtCC;
			public ConsoleColor highbgCC;
			public ConsoleColor highbgLtCC;
			public ConsoleColor errbgCC;
			public ConsoleColor errbgLtCC;
			public ConsoleColor errorCC;
			public ConsoleColor errorLtCC;
		}

		public const string CFG_NOQUIT = "prevent_exit";
		public const string CFG_USEFILE = "dont_save_interface_changes";
		public const string CFG_USESCAN = "always_scan_for_new_games";
		public const string CFG_USECMD = "only_allow_command_line";
		public const string CFG_USETYPE = "typing_input_is_default";
		public const string CFG_USELIST = "single_column_list_is_default";
		public const string CFG_NOPAGE = "do_not_split_over_pages";
		public const string CFG_USESIZE = "enlarge_height_to_fit";
		public const string CFG_USEALPH = "sort_alphabetically";
		public const string CFG_USEFAVE = "sort_favourites_on_top";
		public const string CFG_USELITE = "light_colour_is_default";
		public const string CFG_USEALL = "always_show_all_games";
		public const string CFG_NOCFG = "dont_show_settings_in_platform_list";		// TODO
		public const string CFG_USECUST = "only_scan_custom_games";
		public const string CFG_USETEXT = "text_use_custom_text_below";
		public const string CFG_IMGBORD = "image_draw_border_characters";
		public const string CFG_IMGCUST = "image_dont_use_custom";
		public const string CFG_IMGRTIO = "image_ignore_custom_aspect_ratio";
		// images only work in conhost (cmd or PowerShell and some 3rd party shells), but not in others, e.g., Windows Terminal, TCC, etc.
		public const string CFG_ICONSIZE = "list_icons_max_size_in_characters";     // only in list mode, icons for all games on left; set to 0 to disable
		public const string CFG_ICONRES = "list_icons_resolution";                  // up to 256, but setting higher than 48 causes icons with 32x32 max size to have a border and become smaller by comparison
		public const string CFG_IMGSIZE = "selected_image_max_size_in_characters";  // the icon for the selected game on right; set to 0 to disable
		public const string CFG_IMGRES = "selected_image_icon_resolution";          // up to 256 [same as above]
		public const string CFG_IMGPOS = "selected_image_y_location_percent";
		public const string CFG_COLSIZE = "grid_text_min_column_characters";
		public const string CFG_COLBG1 = "colour_background";
		public const string CFG_COLBG2 = "colour_background_lightmode";
		public const string CFG_COLTITLE1 = "colour_title";
		public const string CFG_COLTITLE2 = "colour_title_lightmode";
		public const string CFG_COLSUB1 = "colour_subtitle";
		public const string CFG_COLSUB2 = "colour_subtitle_lightmode";
		public const string CFG_COLENTRY1 = "colour_entries";
		public const string CFG_COLENTRY2 = "colour_entries_lightmode";
		public const string CFG_COLHIBG1 = "colour_highlight_background";
		public const string CFG_COLHIBG2 = "colour_highlight_background_lightmode";
		public const string CFG_COLHIFG1 = "colour_highlight_text";
		public const string CFG_COLHIFG2 = "colour_highlight_text_lightmode";
		public const string CFG_COLERRBG1 = "colour_error_background";
		public const string CFG_COLERRBG2 = "colour_error_background_lightmode";
		public const string CFG_COLERRFG1 = "colour_error_text";
		public const string CFG_COLERRFG2 = "colour_error_text_lightmode";
		public const string CFG_KEYLT1 = "key_left_1";
		public const string CFG_KEYLT2 = "key_left_2";
		public const string CFG_KEYUP1 = "key_up_1";
		public const string CFG_KEYUP2 = "key_up_2";
		public const string CFG_KEYRT1 = "key_right_1";
		public const string CFG_KEYRT2 = "key_right_2";
		public const string CFG_KEYDN1 = "key_down_1";
		public const string CFG_KEYDN2 = "key_down_2";
		public const string CFG_KEYSEL1 = "key_select_1";
		public const string CFG_KEYSEL2 = "key_select_2";
		public const string CFG_KEYQUIT1 = "key_exit_1";
		public const string CFG_KEYQUIT2 = "key_exit_2";
		public const string CFG_KEYSCAN1 = "key_rescan_1";
		public const string CFG_KEYSCAN2 = "key_rescan_2";
		public const string CFG_KEYHELP1 = "key_help_1";
		public const string CFG_KEYHELP2 = "key_help_2";
		public const string CFG_KEYBACK1 = "key_back_1";
		public const string CFG_KEYBACK2 = "key_back_2";
		public const string CFG_KEYPGUP1 = "key_pageup_1";
		public const string CFG_KEYPGUP2 = "key_pageup_2";
		public const string CFG_KEYPGDN1 = "key_pagedown_1";
		public const string CFG_KEYPGDN2 = "key_pagedown_2";
		public const string CFG_KEYHOME1 = "key_first_1";
		public const string CFG_KEYHOME2 = "key_first_2";
		public const string CFG_KEYEND1 = "key_last_1";
		public const string CFG_KEYEND2 = "key_last_2";
		public const string CFG_KEYFIND1 = "key_search_1";							// TODO
		public const string CFG_KEYFIND2 = "key_search_2";
		public const string CFG_KEYTAB1 = "key_autocomplete_1";						// TODO
		public const string CFG_KEYTAB2 = "key_autocomplete_2";
		public const string CFG_KEYESC1 = "key_cancel_1";							// TODO
		public const string CFG_KEYESC2 = "key_cancel_2";
		public const string CFG_KEYUNIN1 = "key_uninstall_1";
		public const string CFG_KEYUNIN2 = "key_uninstall_2";
		public const string CFG_KEYDESK1 = "key_desktop_shortcut_1";
		public const string CFG_KEYDESK2 = "key_desktop_shortcut_2";
		public const string CFG_KEYHIDE1 = "key_hide_game_1";
		public const string CFG_KEYHIDE2 = "key_hide_game_2";
		public const string CFG_KEYFAVE1 = "key_favourite_1";
		public const string CFG_KEYFAVE2 = "key_favourite_2";
		public const string CFG_KEYALIAS1 = "key_set_alias_1";
		public const string CFG_KEYALIAS2 = "key_set_alias_2";
		public const string CFG_KEYTYPE1 = "key_input_type_1";						// toggle Navigate/Insert
		public const string CFG_KEYTYPE2 = "key_input_type_2";
		public const string CFG_KEYVIEW1 = "key_grid_view_1";						// toggle Grid/List
		public const string CFG_KEYVIEW2 = "key_grid_view_2";
		public const string CFG_KEYMODE1 = "key_light_mode_1";						// toggle Dark/Light
		public const string CFG_KEYMODE2 = "key_light_mode_2";
		public const string CFG_KEYIMG1 = "key_image_display_1";					// toggle Images/Icons
		public const string CFG_KEYIMG2 = "key_image_display_2";
		public const string CFG_KEYSORT1 = "key_sort_method_1";						// toggle Freq/Alpha
		public const string CFG_KEYSORT2 = "key_sort_method_2";
		public const string CFG_TXTMAINT = "text_main_menu_title";
		public const string CFG_TXTCFGT = "text_settings_title";
		public const string CFG_TXTNMAIN1 = "text_instruct1_platforms_nav_custom_msg";
		public const string CFG_TXTNMAIN2 = "text_instruct2_platforms_nav_custom_msg";
		public const string CFG_TXTNSUB1 = "text_instruct1_games_nav_custom_msg";
		public const string CFG_TXTNSUB2 = "text_instruct2_games_nav_custom_msg";
		public const string CFG_TXTIMAIN1 = "text_instruct1_platforms_type_custom_msg";
		public const string CFG_TXTIMAIN2 = "text_instruct2_platforms_type_custom_msg";
		public const string CFG_TXTISUB1 = "text_instruct1_games_type_custom_msg";
		public const string CFG_TXTISUB2 = "text_instruct2_games_type_custom_msg";

		// Read in a value from the Setting.settings file
		public static void ImportConfig(Configuration config)
		{
			config.preventQuit		= Properties.Settings.Default.prevent_exit;
			config.ignoreChanges	= Properties.Settings.Default.dont_save_interface_changes;
			config.alwaysScan		= Properties.Settings.Default.always_scan_for_new_games;
			config.onlyCmdLine		= Properties.Settings.Default.only_allow_command_line;
			config.typeInput		= Properties.Settings.Default.typing_input_is_default;
			config.listOutput		= Properties.Settings.Default.single_column_list_is_default;
			config.noPageSplit		= Properties.Settings.Default.do_not_split_over_pages;
			config.sizeToFit		= Properties.Settings.Default.enlarge_height_to_fit;
			config.alphaSort		= Properties.Settings.Default.sort_alphabetically;
			config.faveSort			= Properties.Settings.Default.sort_favourites_on_top;
			config.lightMode		= Properties.Settings.Default.light_colour_is_default;
			config.goToAll			= Properties.Settings.Default.always_show_all_games;
			config.hideSettings		= Properties.Settings.Default.dont_show_settings_in_platform_list;
			config.onlyCustom		= Properties.Settings.Default.only_scan_custom_games;
			config.msgCustom		= Properties.Settings.Default.text_use_custom_text_values;
			config.imageBorder		= Properties.Settings.Default.image_draw_border_characters;
			config.noImageCustom	= Properties.Settings.Default.image_dont_use_custom;
			config.imageIgnoreRatio	= Properties.Settings.Default.image_ignore_custom_aspect_ratio;
			config.iconSize			= Properties.Settings.Default.list_icons_max_size_in_characters;
			config.iconRes			= Properties.Settings.Default.list_icons_resolution;
			config.imageSize		= Properties.Settings.Default.selected_image_max_size_in_characters;
			config.imageRes			= Properties.Settings.Default.selected_image_icon_resolution;
			config.imagePosition	= Properties.Settings.Default.selected_image_y_location_percent;
			config.columnSize		= Properties.Settings.Default.grid_text_min_column_characters;
			config.bgCol			= Properties.Settings.Default.colour_background;
			config.bgLtCol			= Properties.Settings.Default.colour_background_lightmode;
			config.titleCol			= Properties.Settings.Default.colour_title;
			config.titleLtCol		= Properties.Settings.Default.colour_title_lightmode;
			config.subCol			= Properties.Settings.Default.colour_subtitle;
			config.subLtCol			= Properties.Settings.Default.colour_subtitle_lightmode;
			config.entryCol			= Properties.Settings.Default.colour_entries;
			config.entryLtCol		= Properties.Settings.Default.colour_entries_lightmode;
			config.highlightCol		= Properties.Settings.Default.colour_highlight_background;
			config.highlightLtCol	= Properties.Settings.Default.colour_highlight_background_lightmode;
			config.highbgCol		= Properties.Settings.Default.colour_highlight_text;
			config.highbgLtCol		= Properties.Settings.Default.colour_highlight_text_lightmode;
			config.errbgCol			= Properties.Settings.Default.colour_error_background;
			config.errbgLtCol		= Properties.Settings.Default.colour_error_background_lightmode;
			config.errorCol			= Properties.Settings.Default.colour_error_text;
			config.errorLtCol		= Properties.Settings.Default.colour_error_text_lightmode;
			config.leftKey1			= Properties.Settings.Default.key_left_1;
			config.leftKey2			= Properties.Settings.Default.key_left_2;
			config.upKey1			= Properties.Settings.Default.key_up_1;
			config.upKey2			= Properties.Settings.Default.key_up_2;
			config.rightKey1		= Properties.Settings.Default.key_right_1;
			config.rightKey2		= Properties.Settings.Default.key_right_2;
			config.downKey1			= Properties.Settings.Default.key_down_1;
			config.downKey2			= Properties.Settings.Default.key_down_2;
			config.selectKey1		= Properties.Settings.Default.key_select_1;
			config.selectKey2		= Properties.Settings.Default.key_select_2;
			config.quitKey1			= Properties.Settings.Default.key_exit_1;
			config.quitKey2			= Properties.Settings.Default.key_exit_2;
			config.scanKey1			= Properties.Settings.Default.key_rescan_1;
			config.scanKey2			= Properties.Settings.Default.key_rescan_2;
			config.helpKey1			= Properties.Settings.Default.key_help_1;
			config.helpKey2			= Properties.Settings.Default.key_help_2;
			config.backKey1			= Properties.Settings.Default.key_back_1;
			config.backKey2			= Properties.Settings.Default.key_back_2;
			config.pageUpKey1		= Properties.Settings.Default.key_pageup_1;
			config.pageUpKey2		= Properties.Settings.Default.key_pageup_2;
			config.pageDownKey1		= Properties.Settings.Default.key_pagedown_1;
			config.pageDownKey2		= Properties.Settings.Default.key_pagedown_2;
			config.firstKey1		= Properties.Settings.Default.key_first_1;
			config.firstKey2		= Properties.Settings.Default.key_first_2;
			config.lastKey1			= Properties.Settings.Default.key_last_1;
			config.lastKey2			= Properties.Settings.Default.key_last_2;
			config.searchKey1		= Properties.Settings.Default.key_search_1;
			config.searchKey2		= Properties.Settings.Default.key_search_2;
			config.completeKey1		= Properties.Settings.Default.key_autocomplete_1;
			config.completeKey2		= Properties.Settings.Default.key_autocomplete_2;
			config.cancelKey1		= Properties.Settings.Default.key_cancel_1;
			config.cancelKey2		= Properties.Settings.Default.key_cancel_2;
			config.uninstKey1		= Properties.Settings.Default.key_uninstall_1;
			config.uninstKey2		= Properties.Settings.Default.key_uninstall_2;
			config.desktopKey1		= Properties.Settings.Default.key_desktop_shortcut_1;
			config.desktopKey2		= Properties.Settings.Default.key_desktop_shortcut_2;
			config.hideKey1			= Properties.Settings.Default.key_hide_game_1;
			config.hideKey2			= Properties.Settings.Default.key_hide_game_2;
			config.faveKey1			= Properties.Settings.Default.key_favourite_1;
			config.faveKey2			= Properties.Settings.Default.key_favourite_2;
			config.aliasKey1		= Properties.Settings.Default.key_set_alias_1;
			config.aliasKey2		= Properties.Settings.Default.key_set_alias_2;
			config.typeKey1			= Properties.Settings.Default.key_input_type_1;
			config.typeKey2			= Properties.Settings.Default.key_input_type_2;
			config.viewKey1			= Properties.Settings.Default.key_grid_view_1;
			config.viewKey2			= Properties.Settings.Default.key_grid_view_2;
			config.modeKey1			= Properties.Settings.Default.key_light_mode_1;
			config.modeKey2			= Properties.Settings.Default.key_light_mode_2;
			config.imageKey1		= Properties.Settings.Default.key_image_display_1;
			config.imageKey2		= Properties.Settings.Default.key_image_display_2;
			config.sortKey1			= Properties.Settings.Default.key_sort_method_1;
			config.sortKey2			= Properties.Settings.Default.key_sort_method_2;
			config.mainTitle		= Properties.Settings.Default.text_main_menu_title;
			config.settingsTitle	= Properties.Settings.Default.text_settings_title;
			config.mainTextNav1		= Properties.Settings.Default.text_instruct1_platforms_nav_custom_msg;
			config.mainTextNav2		= Properties.Settings.Default.text_instruct2_platforms_nav_custom_msg;
			config.mainTextIns1		= Properties.Settings.Default.text_instruct1_games_nav_custom_msg;
			config.mainTextIns2		= Properties.Settings.Default.text_instruct2_games_nav_custom_msg;
			config.subTextNav1		= Properties.Settings.Default.text_instruct1_platforms_type_custom_msg;
			config.subTextNav2		= Properties.Settings.Default.text_instruct2_platforms_type_custom_msg;
			config.subTextIns1		= Properties.Settings.Default.text_instruct1_games_type_custom_msg;
			config.subTextIns2		= Properties.Settings.Default.text_instruct2_games_type_custom_msg;
		}

		/// <summary>
		/// Convert user-inputted entries for colour names to appropriate ConsoleColor name
		/// </summary>
		/// <param name="strKeyName">Name of the user-inputted colour</param>
		/// <returns>Value of the translated colour or original colour if not found</returns>
		public static string ToConsoleColorFormat(string strCCName)
		{
			switch (strCCName)
            {
				case "DarkBlack":
					return "Black";
				case "BrightBlue":
				case "LightBlue":
				case "DeepBlue":
				case "RoyalBlue":
					return "Blue";
				case "BlueGreen":
				case "BrightBlueGreen":
				case "LightBlueGreen":
				case "BrightCyan":
				case "LightCyan":
				case "GreenBlue":
				case "BrightGreenBlue":
				case "LightGreenBlue":
				case "Teal":
				case "LightTeal":
				case "BrightTeal":
				case "Turquoise":
				case "BrightTurquoise":
				case "LightTurquoise":
					return "Cyan";
				case "Navy":
				case "NavyBlue":
					return "DarkBlue";
				case "DarkBlueGreen":
				case "DarkGreenBlue":
				case "DarkTurquoise":
					return "DarkCyan";
				case "BrightBlack":
				case "LightBlack":
				case "DarkGrey":
					return "DarkGray";
				case "ForestGreen":
					return "DarkGreen";
				case "Purple":
				case "DarkPurple":
				case "DarkPurpleRed":
				case "DarkRedPurple":
				case "DarkViolet":
					return "DarkMagenta";
				case "Maroon":
				case "DarkMaroon":
					return "DarkRed";
				case "Brown":
				case "BrightBrown":
				case "DarkBrown":
				case "LightBrown":
				case "Ochre":
				case "Tan":
				case "DarkTan":
					return "DarkYellow";
				case "BrightGray":
				case "LightGray":
				case "Grey":
				case "BrightGrey":
				case "LightGrey":
				case "DarkWhite":
					return "Gray";
				case "BrightGreen":
				case "LightGreen":
					return "Green";
				case "Fuchsia":
				case "BrightMagenta":
				case "LightMagenta":
				case "Pink":
				case "BrightPurple":
				case "LightPurple":
				case "PurpleRed":
				case "BrightPurpleRed":
				case "LightPurpleRed":
				case "RedPurple":
				case "BrightRedPurple":
				case "LightRedPurple":
					return "Magenta";
				case "BrightRed":
				case "LightRed":
				case "Salmon":
					return "Red";
				case "BrightWhite":
				case "LightWhite":
					return "White";
				case "BrightYellow":
				case "LightYellow":
					return "Yellow";
				default:
					break;
			}
			return strCCName;
		}

		/// <summary>
		/// Convert "Oem" console keys to user-readable form (US-only)
		/// </summary>
		/// <param name="strCKeyName">Name of the console key name</param>
		/// <returns>Value of the translated key or original key if not found</returns>
		public static string KeyToUSFormat(string strCKeyName)
		{
			if (System.Windows.Forms.InputLanguage.CurrentInputLanguage.LayoutName == "US")
			{
				switch (strCKeyName)
				{
					case "Oem1":
						return ";";
					case "OemPlus":
						return "=";
					case "OemComma":
						return ",";
					case "OemMinus":
						return "-";
					case "OemPeriod":
						return ".";
					case "Oem2":
						return "/";
					case "Oem3":
						return "`";
					case "Oem4":
						return "[";
					case "Oem5":
						return "\\";
					case "Oem6":
						return "]";
					case "Oem7":
						return "'";
					default:
						break;
				}
			}
			return strCKeyName;
		}

		/// <summary>
		/// Abbreviate console keys descriptions
		/// </summary>
		/// <param name="strCKeyName">Name of the console key name</param>
		/// <returns>Value of the translated key or original key if not found</returns>
		public static string ShortenKeyName(string strCKeyName)
		{
			if (System.Windows.Forms.InputLanguage.CurrentInputLanguage.LayoutName == "US")
				strCKeyName = KeyToUSFormat(strCKeyName);
			switch (strCKeyName)
			{
				case "Backspace":
					return "Bksp";
				//case "Control":
				//	return "Ctrl";
				//case "CapsLock":
				//	return "Caps";
				case "Escape":
					return "Esc";
				case "Spacebar":
					return "Space";
				case "PageUp":
					return "PgUp";
				case "PageDown":
					return "PgDn";
				case "LeftArrow":
					return "←";
				case "UpArrow":
					return "↑";
				case "RightArrow":
					return "→";
				case "DownArrow":
					return "↓";
				case "PrintScreen":
					return "PrtScn";
				case "Insert":
					return "Ins";
				case "Delete":
					return "Del";
				case "D0":
					return "0";
				case "D1":
					return "1";
				case "D2":
					return "2";
				case "D3":
					return "3";
				case "D4":
					return "4";
				case "D5":
					return "5";
				case "D6":
					return "6";
				case "D7":
					return "7";
				case "D8":
					return "8";
				case "D9":
					return "9";
				case "LeftWindows":
					return "LWin";
				case "RightWindows":
					return "RWin";
				case "Applications":
					return "Menu";
				case "NumPad0":
					return "Pad0";
				case "NumPad1":
					return "Pad1";
				case "NumPad2":
					return "Pad2";
				case "NumPad3":
					return "Pad3";
				case "NumPad4":
					return "Pad4";
				case "NumPad5":
					return "Pad5";
				case "NumPad6":
					return "Pad6";
				case "NumPad7":
					return "Pad7";
				case "NumPad8":
					return "Pad8";
				case "NumPad9":
					return "Pad9";
				case "Multiply":
					return "Pad*";
				case "Add":
					return "Pad+";
				case "Subtract":
					return "Pad-";
				case "Decimal":
					return "Pad.";
				case "Divide":
					return "Pad/";
				case "NumberLock":
					return "NumLock";
				case "ScrollLock":
					return "Scroll";
				/*
				case "LeftShift":
					return "LShift";
				case "RightShift":
					return "RShift";
				case "LeftControl":
					return "LCtrl";
				case "RightControl":
					return "RCtrl";
				case "LeftAlt":
					return "LAlt";
				case "RightAlt":
					return "RAlt";
				*/
				//case "BrowserHome":
				//	return "Browser⌂";
				case "VolumeMute":
					return "Mute";
				case "VolumeDown":
					return "Vol-";
				case "VolumeUp":
					return "Vol+";
				case "MediaNext":
					return "►|";
				case "MediaPrevious":
					return "|◄";
				case "MediaStop":
					return "■";
				case "MediaPlay":
					return "►";
				// These are renamed by KeyToUSFormat() if applicable
				case "OemPlus":
					return "Oem+";
				case "OemComma":
					return "Oem,";
				case "OemMinus":
					return "Oem-";
				case "OemPeriod":
					return "Oem.";
				default:
					break;
			}
			return strCKeyName;
		}

		/// <summary>
		/// Convert user-inputted entries for key names to appropriate ConsoleKey name
		/// </summary>
		/// <param name="strKeyName">Name of the user-inputted key name</param>
		/// <returns>Value of the translated key or original key if not found</returns>
		public static string ToConsoleKeyFormat(string strKeyName)
		{
			switch (strKeyName.ToUpper())
			{
				case "RETURN":
				case "NEWLINE":
					return "Enter";
				case "BREAK":
					return "Pause";
				case "ESC":
					return "Escape";
				case " ":
				case "SPACE":
					return "Spacebar";
				case "PGUP":
					return "PageUp";
				case "PGDN":
					return "PageDown";
				case "LEFT":
					return "LeftArrow";
				case "UP":
					return "UpArrow";
				case "RIGHT":
					return "RightArrow";
				case "DOWN":
					return "DownArrow";
				case "INS":
					return "Insert";
				case "DEL":
					return "Delete";
				case "0":
					return "D0";
				case "1":
					return "D1";
				case "2":
					return "D2";
				case "3":
					return "D3";
				case "4":
					return "D4";
				case "5":
					return "D5";
				case "6":
					return "D6";
				case "7":
					return "D7";
				case "8":
					return "D8";
				case "9":
					return "D9";
				case ";":
				case ":":
				case "SEMICOLON":
				case "COLON":
				case "OEMCOLON":
				case "OEMSEMICOLON":
					return "Oem1";
				//return "OemSemicolon";
				case "PAD0":
				case "NUM0":
					return "NumPad0";
				case "PAD1":
				case "NUM1":
					return "NumPad1";
				case "PAD2":
				case "NUM2":
					return "NumPad2";
				case "PAD3":
				case "NUM3":
					return "NumPad3";
				case "PAD4":
				case "NUM4":
					return "NumPad4";
				case "PAD5":
				case "NUM5":
					return "NumPad5";
				case "PAD6":
				case "NUM6":
					return "NumPad6";
				case "PAD7":
				case "NUM7":
					return "NumPad7";
				case "PAD8":
				case "NUM8":
					return "NumPad8";
				case "PAD9":
				case "NUM9":
					return "NumPad9";
				case "ASTERISK":
				case "STAR":
				case "SPLAT":
				case "NUM*":
				case "NUMASTERISK":
				case "NUMSPLAT":
				case "NUMSTAR":
				case "NUMMULTIPLY":
				case "PAD*":
				case "PADASTERISK":
				case "PADSPLAT":
				case "PADSTAR":
				case "PADMULTIPLY":
				case "NUMPAD*":
				case "NUMPADASTERISK":
				case "NUMPADSPLAT":
				case "NUMPADSTAR":
				case "NUMPADMULTIPLY":
					return "Multiply";
				case "NUM+":
				case "NUMPLUS":
				case "NUMSUM":
				case "NUMADD":
				case "PAD+":
				case "PADPLUS":
				case "PADSUM":
				case "PADADD":
				case "NUMPAD+":
				case "NUMPADPLUS":
				case "NUMPADSUM":
				case "NUMPADADD":
					return "Add";
				case "NUM-":
				case "NUMHYPHEN":
				case "NUMDASH":
				case "NUMMINUS":
				case "NUMSUBTRACT":
				case "PAD-":
				case "PADHYPHEN":
				case "PADDASH":
				case "PADMINUS":
				case "PADSUBTRACT":
				case "NUMPAD-":
				case "NUMPADHYPHEN":
				case "NUMPADDASH":
				case "NUMPADMINUS":
				case "NUMPADSUBTRACT":
					return "Subtract";
				case "NUM.":
				case "NUMPERIOD":
				case "NUMDOT":
				case "NUMPOINT":
				case "NUMDECIMAL":
				case "PAD.":
				case "PADPERIOD":
				case "PADDOT":
				case "PADPOINT":
				case "PADDECIMAL":
				case "NUMPAD.":
				case "NUMPADPERIOD":
				case "NUMPADDOT":
				case "NUMPADPOINT":
				case "NUMPADDECIMAL":
					return "Decimal";
				case "NUM/":
				case "NUMSLASH":
				case "NUMFWDSLASH":
				case "NUMFORWARDSLASH":
				case "NUMDIVIDE":
				case "PAD/":
				case "PADSLASH":
				case "PADFWDSLASH":
				case "PADFORWARDSLASH":
				case "PADDIVIDE":
				case "NUMPAD/":
				case "NUMPADSLASH":
				case "NUMPADFWDSLASH":
				case "NUMPADFORWARDSLASH":
				case "NUMPADDIVIDE":
					return "Divide";
				case "=":						// US standard
				case "+":						// US standard
				case "SUM":
				case "PLUS":
					return "OemPlus";
				case ",":						// US standard
				case "<":						// US standard
				case "COMMA":
				case "ANGLE":
				case "ANGLEBRACKET":
				case "WAKA":
				case "WACA":
				case "LESS":
				case "LESSTHAN":
				case "LEFTWAKA":
				case "LEFTWACA":
				case "LEFTANGLE":
				case "LEFTANGLEBRACKET":
				case "OPENWAKA":
				case "OPENWACA":
				case "OPENANGLE":
				case "OPENANGLEBRACKET":
					return "OemComma";
				case "-":						// US standard
				case "_":						// US standard
				case "HYPHEN":
				case "DASH":
				case "UNDERSCORE":
				case "UNDERLINE":
				case "MINUS":
					return "OemMinus";
				case ".":						// US standard
				case ">":						// US standard
				case "DOT":
				case "POINT":
				case "PERIOD":
				case "GREATER":
				case "GREATERTHAN":
				case "RIGHTWAKA":
				case "RIGHTWACA":
				case "RIGHTANGLE":
				case "RIGHTANGLEBRACKET":
				case "RIGHTANGLEBRACKETS":
				case "CLOSEWAKA":
				case "CLOSEWACA":
				case "CLOSEANGLE":
				case "CLOSEANGLEBRACKET":
				case "CLOSEANGLEBRACKETS":
					return "OemPeriod";
				case "/":						// US standard
				case "?":						// US standard
				case "SLASH":
				case "FWDSLASH":
				case "FORWARDSLASH":
				case "QUESTION":
				case "QUESTIONMARK":
				case "OEMQUESTION":
					return "Oem2";
				//return "OemQuestion";
				case "`":						// US standard
				case "~":						// US standard
				case "BACKTICK":
				case "TILDE":
				case "OEMTILDE":
					return "Oem3";
				//return "Oemtilde";
				case "[":						// US standard
				case "{":						// US standard
				case "BRACKET":
				case "BRACKETS":
				case "SQUAREBRACKET":
				case "SQUAREBRACKETS":
				case "BRACE":
				case "BRACES":
				case "CURLY":
				case "CURLYS":
				case "CURLIES":
				case "CURLYBRACE:":
				case "CURLYBRACES:":
				case "CURLYBRACKET:":
				case "CURLYBRACKETS:":
				case "LEFTBRACKET":
				case "LEFTBRACKETS":
				case "LEFTSQUAREBRACKET":
				case "LEFTSQUAREBRACKETS":
				case "LEFTBRACE":
				case "LEFTBRACES":
				case "LEFTCURLY":
				case "LEFTCURLYS":
				case "LEFTCURLIES":
				case "LEFTCURLYBRACE":
				case "LEFTCURLYBRACES":
				case "LEFTCURLYBRACKET":
				case "LEFTCURLYBRACKETS":
				case "OPENBRACKET":
				case "OPENBRACKETS":
				case "OPENSQUAREBRACKET":
				case "OPENSQUAREBRACKETS":
				case "OPENBRACE":
				case "OPENBRACES":
				case "OPENCURLY":
				case "OPENCURLYS":
				case "OPENCURLYBRACE":
				case "OPENCURLYBRACES":
				case "OPENCURLYBRACKET":
				case "OPENCURLYBRACKETS":
				case "OEMOPENBRACKET":
				case "OEMOPENBRACKETS":
					return "Oem4";
				//return "OemOpenBrackets";
				case "\\":						// US standard
				case "|":						// US standard
				case "BACKSLASH":
				case "BKSLASH":
				case "WHACK":
				case "PIPE":
				case "BAR":
				case "STICK":
				case "VBAR":
				case "VERTBAR":
				case "VERTICAL":
				case "VERTICALBAR":
				case "OEMPIPE":
					return "Oem5";
				//return "OemPipe";
				case "OEMBACKSLASH":
					return "Oem102";
				case "]":						// US standard
				case "}":						// US standard
				case "RIGHTBRACKET":
				case "RIGHTBRACKETS":
				case "RIGHTSQUAREBRACKET":
				case "RIGHTSQUAREBRACKETS":
				case "RIGHTBRACE":
				case "RIGHTBRACES":
				case "RIGHTCURLY":
				case "RIGHTCURLYS":
				case "RIGHTCURLIES":
				case "RIGHTCURLYBRACE":
				case "RIGHTCURLYBRACES":
				case "RIGHTCURLYBRACKET":
				case "RIGHTCURLYBRACKETS":
				case "CLOSEBRACKET":
				case "CLOSEBRACKETS":
				case "CLOSESQUAREBRACKET":
				case "CLOSESQUAREBRACKETS":
				case "CLOSEBRACE":
				case "CLOSEBRACES":
				case "CLOSECURLY":
				case "CLOSECURLYS":
				case "CLOSECURLIES":
				case "CLOSECURLYBRACE":
				case "CLOSECURLYBRACES":
				case "CLOSECURLYBRACKET":
				case "CLOSECURLYBRACKETS":
				case "OEMCLOSEBRACKET":
				case "OEMCLOSEBRACKETS":
					return "Oem6";
				//return "OemCloseBrackets";
				case "'":						// US standard
				case "\"":						// US standard
				case "APOSTROPHE":
				case "APOSTROPHES":
				case "TICK":
				case "QUOTE":
				case "QUOTES":
				case "DOUBLEQUOTE":
				case "DOUBLEQUOTES":
				case "QUOTEMARK":
				case "QUOTEMARKS":
				case "QUOTATIONMARK":
				case "QUOTATIONMARKS":
				case "OEMQUOTE:":
				case "OEMQUOTES:":
					return "Oem7";
				//return "OemQuotes";
				case "":
					return "NoName";
				default:
					break;
			}
			return strKeyName;
		}
	}
}