CREATE TABLE [dbo].[Templates] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL,
    [CreatedOn]           DATETIME         NULL,
    [CreatedBy]           NVARCHAR (100)   NULL,
    [ModifiedOn]          DATETIME         NULL,
    [ModifiedBy]          NVARCHAR (100)   NULL,
    [InUntis]             BIT              NULL,
    [ApprovedBySubstitu]  BIT              NULL,
    [ApprovedByPrincipal] BIT              NULL,
    [Title]               NVARCHAR (200)   NULL,
    [Description]         NVARCHAR (200)   NULL,
    CONSTRAINT [PK_Templates] PRIMARY KEY CLUSTERED ([Id] ASC)
);

