-- Create tables --

CREATE TABLE IF NOT EXISTS "SystemAttribute" (
	"AttributeName"		varchar(50),
	"AttributeIndex"	INTEGER,
	"AttributeValue"	varchar(255)
);

CREATE TABLE IF NOT EXISTS "Platform" (
	"PlatformID"	INTEGER NOT NULL UNIQUE,
	"Name"			VARCHAR(50) NOT NULL UNIQUE,
	"Description" 	VARCHAR(255),
	"Path"			VARCHAR(255),
	"IsActive"		BIT NOT NULL DEFAULT 0,
	PRIMARY KEY("PlatformID" AUTOINCREMENT)
);

CREATE TABLE IF NOT EXISTS "Game" (
	"GameID"		INTEGER NOT NULL UNIQUE,
	"PlatformFK"	INTEGER,
	"Identifier"	varchar(50) NOT NULL UNIQUE,
	"Title"			varchar(50) NOT NULL,
	"Alias"			varchar(50) NOT NULL UNIQUE,
	"Launch"		varchar(255) NOT NULL,
	"Frequency"		NUMERIC NOT NULL DEFAULT 0.0,
	"IsFavourite"	bit NOT NULL DEFAULT 0,
	"IsHidden"		bit NOT NULL DEFAULT 0,
	"Group"			varchar(255),
	--"Icon" 			VARCHAR(255),
	PRIMARY KEY("GameID" AUTOINCREMENT),
	CONSTRAINT "Platform_Title" UNIQUE("PlatformFK", "Title")
);

CREATE TABLE IF NOT EXISTS "GameAttribute" (
	"GameFK"			INTEGER,
	"AttributeName"		varchar(50),
	"AttributeIndex"	INTEGER,
	"AttributeValue"	varchar(255),
	FOREIGN KEY("GameFK") REFERENCES "Game"("GameID")
);

CREATE TABLE IF NOT EXISTS "Extension" (
	"ExtensionID"		INTEGER NOT NULL UNIQUE,
	"Name"				varchar(255) NOT NULL UNIQUE,
	"IsActive"			BIT NOT NULL DEFAULT 0,
	"DllPath"			varchar(255) NOT NULL UNIQUE,
	PRIMARY KEY("ExtensionID" AUTOINCREMENT)
);

/*
CREATE TABLE "Player" (
	"PlayerID"		INTEGER NOT NULL UNIQUE,
	"Title"			varchar(50) NOT NULL UNIQUE,
	"Launch"		varchar(255) NOT NULL,
	"Param"			varchar(255),
	"Filepath"		varchar(255),
	"Filemask"		varchar(255),
	"FileIconPath"	varchar(255),
	"Description" 	VARCHAR(255),
	PRIMARY KEY("PlayerID" AUTOINCREMENT)
);

CREATE TABLE "PlayerAttribute" (
	"PlayerFK"			INTEGER,
	"AttributeName"		varchar(50),
	"AttributeIndex"	INTEGER,
	"AttributeValue"	varchar(255),
	FOREIGN KEY("PlayerFK") REFERENCES "Player"("PlayerID")
);
*/
-- Add any data --

-- Setting that controls if the app should show inactive platforms
IF NOT EXISTS (SELECT AttbitueValue FROM SystemAttribute WHERE AttributeName = 'SHOW_INACTIVE_PLATFORMS')
BEGIN
	INSERT INTO SystemAttribute('SHOW_INACTIVE_PLATFORMS', 0, 'N')
END

-- Setting controlling if the app should close when a game is selected
IF NOT EXISTS (SELECT AttbitueValue FROM SystemAttribute WHERE AttributeName = 'CLOSE_ON_LAUNCH')
BEGIN
	INSERT INTO SystemAttribute('CLOSE_ON_LAUNCH', 0, 'Y')
END