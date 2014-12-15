
use [Copiosis];

GO

PRINT 'Preparing to upgrade, checking version ...';

IF OBJECT_ID ('dbo.Version', 'U') IS NULL
	RAISERROR ('Error updating - Database does not exist!', 16, 1);

DECLARE @CurrentVersion AS int;
DECLARE @ExpectedVersion AS int;
DECLARE @StopOnVersionMismatch AS bit;

set @StopOnVersionMismatch = 0;

SELECT @CurrentVersion=VersionNumber FROM [dbo].[Version] WHERE [Id] = 'Copiosis';
SET @ExpectedVersion = $(Version)-1

IF @CurrentVersion <> @ExpectedVersion and @CurrentVersion <> $(Version  and isnull(@StopOnVersionMismatch, 1) = 1)
begin
	RAISERROR ('Error updating - Wrong database version! (expected %d, found %d)', 16, 1, @ExpectedVersion, @CurrentVersion);
	SET NOEXEC ON;
end

PRINT 'Updating Copiosis database (version ' + cast($(Version) as varchar(5)) + ')...';

GO

ALTER DATABASE [Copiosis]
SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

GO

/* END COMMON HEADER */



/* START COMMON FOOTER */

GO

ALTER DATABASE [Copiosis]
SET MULTI_USER

GO

PRINT 'Registering version ' + cast($(Version) as varchar(5));

SET NOCOUNT ON
UPDATE [Version] SET [VersionNumber] = $(Version) WHERE [Id] = 'Copiosis'

PRINT 'Done.'