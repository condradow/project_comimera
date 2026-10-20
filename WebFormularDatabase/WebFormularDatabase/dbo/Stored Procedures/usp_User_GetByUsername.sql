
CREATE   PROCEDURE dbo.usp_User_GetByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Resultset 1: User inkl. Hash (Vergleich erfolgt in der Anwendung)
    SELECT Id, Username, Email, PasswordHash, IsActive
    FROM dbo.Users
    WHERE Username = @Username;

    -- Resultset 2: Rollen
    SELECT r.Name
    FROM dbo.Users u
    JOIN dbo.UserRoles ur ON ur.UserId = u.Id
    JOIN dbo.Roles r      ON r.Id = ur.RoleId
    WHERE u.Username = @Username;
END