
CREATE   PROCEDURE dbo.usp_User_AddRole
    @UserId   UNIQUEIDENTIFIER,
    @RoleName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.UserRoles (UserId, RoleId)
    SELECT @UserId, r.Id
    FROM dbo.Roles r
    WHERE r.Name = @RoleName
      AND NOT EXISTS (SELECT 1 FROM dbo.UserRoles ur WHERE ur.UserId = @UserId AND ur.RoleId = r.Id);
END