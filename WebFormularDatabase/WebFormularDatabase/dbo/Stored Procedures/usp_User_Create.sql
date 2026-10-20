
/* ============================================================
   4) Prozeduren (hier funktioniert CREATE OR ALTER)
   ============================================================ */
CREATE   PROCEDURE dbo.usp_User_Create
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(256),
    @Email        NVARCHAR(256) = NULL,
    @RoleName     NVARCHAR(50)  = N'User',
    @UserId       UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @RoleId INT = (SELECT Id FROM dbo.Roles WHERE Name = @RoleName);
    IF @RoleId IS NULL
        THROW 50001, 'Rolle existiert nicht.', 1;

    BEGIN TRAN;
        SET @UserId = NEWID();

        INSERT INTO dbo.Users (Id, Username, Email, PasswordHash)
        VALUES (@UserId, @Username, @Email, @PasswordHash);

        INSERT INTO dbo.UserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
    COMMIT;
END