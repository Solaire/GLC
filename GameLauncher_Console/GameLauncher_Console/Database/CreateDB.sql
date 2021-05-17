-- Create tables --

CREATE TABLE "SystemAttribute" (
	"AttributeName"		varchar(50),
	"AttributeIndex"	INTEGER,
	"AttributeValue"	varchar(255)
);

CREATE TABLE "Platform" (
	"PlatformID"	INTEGER NOT NULL UNIQUE,
	"Name"			VARCHAR(50) NOT NULL UNIQUE,
	"Description" 	VARCHAR(255),
	PRIMARY KEY("PlatformID" AUTOINCREMENT)
);

CREATE TABLE "Game" (
	"GameID"		INTEGER NOT NULL UNIQUE,
	"PlatformFK"	INTEGER,
	"Identifier"	varchar(50) NOT NULL UNIQUE,
	"Title"			varchar(50) NOT NULL UNIQUE,
	"Launch"		varchar(255) NOT NULL,
	"Uninstall"		varchar(255),
	"IsFavourite"	bit NOT NULL DEFAULT 0,
	"IsHidden"		bit NOT NULL DEFAULT 0,
	"Frequency"		NUMERIC NOT NULL DEFAULT 0.0,
	"Rating"		INTEGER,
	"Description" 	VARCHAR(255),
	PRIMARY KEY("GameID" AUTOINCREMENT)
);

CREATE TABLE "GameAttribute" (
	"GameFK"	INTEGER,
	"AttributeName"		varchar(50),
	"AttributeIndex"	INTEGER,
	"AttributeValue"	varchar(255),
	FOREIGN KEY("GameFK") REFERENCES "Game"("GameID")
);

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

-- Add any data --

INSERT INTO "SystemAttribute" 
	("AttributeName", "AttributeIndex", "AttributeValue")
VALUES
	('SCHEMA_VERSION', 0, '1.0.0');