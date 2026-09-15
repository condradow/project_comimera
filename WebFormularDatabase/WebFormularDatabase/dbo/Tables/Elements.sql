CREATE TABLE [dbo].[Elements] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [DocumentId] UNIQUEIDENTIFIER NULL,
    [JSONData]   NVARCHAR (MAX)   NULL,
    [CreatedOn]  DATETIME         NULL,
    [CreatedBy]  NVARCHAR (100)   NULL,
    [ModifiedOn] DATETIME         NULL,
    [ModifiedBy] NVARCHAR (100)   NULL,
    CONSTRAINT [PK_Elements] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Elements_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id])
);

