CREATE TABLE Services
(
    ServiceUID NVARCHAR(36) NOT NULL,
    ServiceName TEXT NOT NULL,
    ServiceLocation TEXT NOT NULL,
    ServiceVersion NVARCHAR(20) NOT NULL,
    AgentUID NVARCHAR(36) NOT NULL,
    CONSTRAINT [PK_Service] PRIMARY KEY ([ServiceUID])
    CONSTRAINT [FK_Service_Agent] FOREIGN KEY ([AgentUID]) REFERENCES [Services] ([ServiceUID]) ON DELETE NO ACTION ON UPDATE NO ACTION
);


CREATE TABLE ServiceDependences
(
	ServiceUID NVARCHAR(36) NOT NULL,
	DependenceServiceUID NVARCHAR(36) NOT NULL,
	CONSTRAINT [PK_ServiceDependence] PRIMARY KEY ([ServiceUID],[DependenceServiceUID])
);

CREATE TABLE ServiceAddresses
(		
	Address TEXT NOT NULL,
	ServiceUID NVARCHAR(36) NOT NULL,
	IsUse BIT,
	CONSTRAINT [PK_ServiceAddress] PRIMARY KEY ([Address]),
	CONSTRAINT [FK_ServiceAddress_Services] FOREIGN KEY ([ServiceUID]) REFERENCES [Services] ([ServiceUID]) ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE ServiceInstances
(
	ServiceUID NVARCHAR(36) NOT NULL, 
	ServiceName TEXT NOT NULL,
	ServiceLocation TEXT NOT NULL,
	ServiceVersion NVARCHAR(20) NOT NULL,
	ServiceAddress TEXT NOT NULL,
	ProcessName TEXT NOT NULL,
	PID INTEGER NOT NULL,
	ServiceStatus INTEGER NOT NULL,
	LastSession DATETIME NOT NULL,
	CONSTRAINT [PK_ServiceInstances] PRIMARY KEY ([ServiceUID])
);



