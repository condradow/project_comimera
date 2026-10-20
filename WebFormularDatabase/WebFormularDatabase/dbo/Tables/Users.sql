CREATE TABLE [dbo].[Users] (
    [Id]           UNIQUEIDENTIFIER CONSTRAINT [DF_Users_Id] DEFAULT (newsequentialid()) NOT NULL,
    [Username]     NVARCHAR (50)    NOT NULL,
    [PasswordHash] NVARCHAR (256)   NOT NULL,
    [Email]        NVARCHAR (256)   NULL,
    [IsActive]     BIT              CONSTRAINT [DF_Users_IsActive] DEFAULT ((1)) NOT NULL,
    [CreatedAt]    DATETIME2 (0)    CONSTRAINT [DF_Users_CreatedAt] DEFAULT (sysutcdatetime()) NOT NULL,
    [LastLoginAt]  DATETIME2 (0)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Users_Username] UNIQUE NONCLUSTERED ([Username] ASC)
);

