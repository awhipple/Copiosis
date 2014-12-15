CREATE TABLE location (
locationID int IDENTITY(1,1) NOT NULL PRIMARY KEY, 
country varchar(55), 
state varchar(55), 
city varchar(55), 
neighborhood varchar(55), 
signupKey varchar(55)
);

CREATE TABLE [user] (
userID int IDENTITY(1,1) NOT NULL PRIMARY KEY, 
username varchar(55), 
password varchar(255), 
email varchar(155), 
firstName varchar(55), 
lastName varchar(55), 
status int NOT NULL,	
nbr float,
created datetime,
lastLogin datetime,
locationID int FOREIGN KEY REFERENCES location(locationID)
);

CREATE TABLE itemClass (
classID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
name varchar(55),
suggestedGateway int,
cPdb float,
a float,
aMax int,
d int,
aPrime int,
cCb float,
m1 float,
pO int,
m2 float,
cEb float,
s int,
m3 float,
sE smallint,
m4 float,
sH smallint
);

CREATE TABLE product (
	productID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
name varchar(155),
description varchar(255),
gateway int,
itemClass int FOREIGN KEY REFERENCES itemClass(classID),
createdDate datetime, 
deletedDate datetime
);

CREATE TABLE [transaction] (
transactionID uniqueidentifier NOT NULL DEFAULT newid() PRIMARY KEY, 
providerID int FOREIGN KEY REFERENCES [user](userID),
providerNotes text,
receiverID int FOREIGN KEY REFERENCES [user](userID),
receiverNotes text,
date date, 
productID int FOREIGN KEY REFERENCES product(productID),
productDesc varchar(255),
status varchar(16),
dateAdded datetime,
createdBy int FOREIGN KEY REFERENCES [user](userID),
dateClosed datetime,
nbr float,
satisfaction smallint
);

CREATE TABLE [Version] (
	Id varchar(25),
	VersionNumber int
);

INSERT INTO Version VALUES('Copiosis', 1)