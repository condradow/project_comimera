CREATE TABLE [dbo].[TemplateElements] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [TemplateId] UNIQUEIDENTIFIER NULL,
    [JSONData]   NVARCHAR (MAX)   NULL,
    [CreatedOn]  DATETIME         NULL,
    [CreatedBy]  NVARCHAR (100)   NULL,
    [ModifiedOn] DATETIME         NULL,
    [ModifiedBy] NVARCHAR (100)   NULL,
    CONSTRAINT [PK_TemplateElements] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TemplateElements_Templates] FOREIGN KEY ([TemplateId]) REFERENCES [dbo].[Templates] ([Id])
);

