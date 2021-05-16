-- Create tables --

CREATE TABLE "SystemAttribute" (
	"AttributeType"	INTEGER,
	"AttributeIndex"	INTEGER,
	"AttributeValue"	varchar(255)
);

CREATE TABLE "Platform" (
	"PlatformID"	INTEGER NOT NULL UNIQUE,
	"Name"	VARCHAR(50),
	"GameCount"	INTEGER,
	PRIMARY KEY("PlatformID" AUTOINCREMENT)
);

CREATE TABLE "Game" (
	"GameID"	INTEGER NOT NULL UNIQUE,
	"PlatformFK"	INTEGER,
	"Identifier"	varchar(50),
	"Title"	varchar(50),
	"Launch"	varchar(255),
	"Uninstaller"	varchar(255),
	"IsFavourite"	bit DEFAULT 0,
	"IsHidden"	bit DEFAULT 0,
	"Frequency"	NUMERIC DEFAULT 0,
	PRIMARY KEY("GameID" AUTOINCREMENT)
);

CREATE TABLE "GameAttribute" (
	"GameFK"	INTEGER,
	"AttributeType"	INTEGER,
	"AttributeIndex"	INTEGER,
	"AttributeValue"	varchar(255),
	FOREIGN KEY("GameFK") REFERENCES "Game"("GameID")
);

-- Add any data --

INSERT INTO "SystemAttribute" 
	("AttributeType", "AttributeIndex", "AttributeValue")
VALUES
	(1, 0, '1.0.0');