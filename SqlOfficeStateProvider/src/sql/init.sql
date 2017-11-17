USE [DXOfficeState]
GO

DROP TABLE [dbo].[States]
DROP TABLE [dbo].[Locks]
DROP TABLE [dbo].[DocIdToWorkSessionId]
DROP TABLE [dbo].[WorkSessionIdToDocId]
DROP TABLE [dbo].[KeyValue]

DROP PROCEDURE [dbo].[AddCheckedOut]
DROP PROCEDURE [dbo].[getWorkSessionIdFromDocumentId]
DROP PROCEDURE [dbo].[setState]
DROP PROCEDURE [dbo].[tryToLock]
DROP PROCEDURE [dbo].[Find]
DROP PROCEDURE [dbo].[HasWorkSessionId]
DROP PROCEDURE [dbo].[Remove]
DROP PROCEDURE [dbo].[CheckIn]
DROP PROCEDURE [dbo].[CheckOut]
DROP PROCEDURE [dbo].[UndoCheckOut]
DROP PROCEDURE [dbo].[Get]
DROP PROCEDURE [dbo].[Set]

CREATE TABLE [dbo].[States](
	[WorkSessionId] [nvarchar](50) NOT NULL,
	[State] [varbinary](max)
)
Go

CREATE TABLE [dbo].[Locks](
	[WorkSessionId] [nvarchar](50) NOT NULL,
	[LockerId] [nvarchar](1000) NOT NULL
);
GO

CREATE TABLE [dbo].[DocIdToWorkSessionId](
	[DocumentId] [nvarchar](max) NOT NULL,
	[WorkSessionId] [nvarchar](50) NOT NULL
);
GO

CREATE TABLE [dbo].[WorkSessionIdToDocId](
	[WorkSessionId] [nvarchar](50) NOT NULL,
	[DocumentId] [nvarchar](max) NOT NULL
);
GO

CREATE TABLE [dbo].[KeyValue](
	[Key] [nvarchar](50) NOT NULL,
	[Value] [varbinary](max)
);
GO

CREATE PROCEDURE [dbo].[setState]
@WorkSessionId NVARCHAR(50),
@DocumentId NVARCHAR(max),
@State VARBINARY(max)
AS
BEGIN
	UPDATE [dbo].[States] SET [State]=@State WHERE [WorkSessionId]=@WorkSessionId
    IF @@ROWCOUNT=0
        INSERT INTO [dbo].[States] ([WorkSessionId], [State]) VALUES (@WorkSessionId, @State)

	IF @DocumentId IS NOT NULL AND @DocumentId != ''
	BEGIN
		UPDATE [dbo].[WorkSessionIdToDocId] SET [DocumentId]=@DocumentId WHERE [WorkSessionId]=@WorkSessionId
		IF @@ROWCOUNT=0
			INSERT INTO [dbo].[WorkSessionIdToDocId] ([WorkSessionId], [DocumentId]) VALUES (@WorkSessionId, @DocumentId)

		UPDATE [dbo].[DocIdToWorkSessionId] SET [WorkSessionId]=@WorkSessionId WHERE [DocumentId]=@DocumentId
		IF @@ROWCOUNT=0
			INSERT INTO [dbo].[DocIdToWorkSessionId] ([DocumentId], [WorkSessionId]) VALUES (@DocumentId, @WorkSessionId)
	END
END
GO

CREATE PROCEDURE [dbo].[getWorkSessionIdFromDocumentId]
@DocumentId NVARCHAR(max),
@WorkSessionId NVARCHAR(50) OUTPUT
AS
	SET @WorkSessionId = (SELECT TOP(1) [WorkSessionId] FROM [dbo].[DocIdToWorkSessionId] WHERE [DocumentId] = @DocumentId)
GO

CREATE PROCEDURE [dbo].[tryToLock]
@WorkSessionId NVARCHAR(50),
@lockerId NVARCHAR(1000),
@WasLockedByAnother BIT = 0 OUTPUT
AS
BEGIN
	DECLARE @AlreadyLockedBy NVARCHAR(1000)
	SET @AlreadyLockedBy = (SELECT  TOP (1) [LockerId] FROM [dbo].[Locks] WHERE [WorkSessionId] = @WorkSessionId)

	IF @AlreadyLockedBy IS NULL
		INSERT INTO [dbo].[Locks] ([workSessionId], [LockerId]) VALUES (@WorkSessionId, @lockerId);
	ELSE
	BEGIN
		IF @AlreadyLockedBy != @lockerId
			SET @WasLockedByAnother = 1
	END

END
GO

CREATE PROCEDURE [dbo].[AddCheckedOut]
@WorkSessionId NVARCHAR(50),
@DocumentId NVARCHAR(max),
@lockerId NVARCHAR(1000),
@Success BIT = 0 OUTPUT,
@WorkSessionIdAlreadyExists BIT = 0 OUTPUT
AS
BEGIN
	SET @Success = 0

	DECLARE @State VARBINARY(max) = null
	DECLARE @WorkSessionIdForThisDoc NVARCHAR(50) = null

	EXEC [dbo].[getWorkSessionIdFromDocumentId] @DocumentId, @WorkSessionIdForThisDoc OUTPUT
	
	IF @WorkSessionIdForThisDoc IS NOT NULL
		SET @WorkSessionIdAlreadyExists = 1
	ELSE
	BEGIN
		SET @WorkSessionIdAlreadyExists = 0
		Declare @wasLockedByAnother BIT = 0
		EXEC [dbo].[tryToLock] @WorkSessionId, @lockerId, @wasLockedByAnother OUTPUT
		IF @wasLockedByAnother = 0
		BEGIN
			SET @Success = 1
			EXEC [dbo].[setState] @WorkSessionId, @DocumentId, @State
		END
	END
END
GO

CREATE PROCEDURE [dbo].[Remove]
@WorkSessionId NVARCHAR(50),
@lockerId NVARCHAR(1000),
@wasLockedByAnother BIT = 0 OUTPUT
AS
BEGIN
	DECLARE @DocumentId NVARCHAR(max)
	SET @wasLockedByAnother = 0
	EXEC [dbo].[tryToLock] @WorkSessionId, @lockerId, @wasLockedByAnother OUTPUT
	IF @wasLockedByAnother = 0
	BEGIN
		SET @DocumentId = (SELECT  TOP (1) [DocumentId] FROM [dbo].[WorkSessionIdToDocId] WHERE [WorkSessionId] = @WorkSessionId)
		IF @DocumentId IS NOT NULL
			DELETE FROM [dbo].[DocIdToWorkSessionId] WHERE [DocumentId]=@DocumentId
		DELETE FROM [dbo].[States] WHERE [WorkSessionId]=@WorkSessionId
		DELETE FROM [dbo].[WorkSessionIdToDocId] WHERE [WorkSessionId]=@WorkSessionId
		DELETE FROM [dbo].[Locks] WHERE [WorkSessionId]=@WorkSessionId
	END
END
GO

CREATE PROCEDURE [dbo].[Find]
@DocumentId NVARCHAR(max),
@WorkSessionId NVARCHAR(50) = NULL OUTPUT
AS
BEGIN
	EXEC [dbo].[getWorkSessionIdFromDocumentId] @DocumentId, @WorkSessionId OUTPUT
END
GO

CREATE PROCEDURE [dbo].[HasWorkSessionId]
@WorkSessionId NVARCHAR(50),
@Found BIT OUTPUT
AS
BEGIN
	SET @Found = 0
	IF EXISTS(SELECT [WorkSessionId] FROM [dbo].[States] WHERE [WorkSessionId]=@WorkSessionId)
		SET @Found = 1
END
GO

CREATE PROCEDURE [dbo].[CheckOut]
@WorkSessionId NVARCHAR(50),
@lockerId NVARCHAR(1000),
@State VARBINARY(max) = NULL OUTPUT,
@wasLockedByAnother BIT = 0 OUTPUT
AS
BEGIN
	DECLARE @WorkSessionExists BIT = 0
	SET @wasLockedByAnother = 0
	EXEC [dbo].[HasWorkSessionId] @WorkSessionId, @WorkSessionExists OUTPUT
	IF @WorkSessionExists = 1
	BEGIN
		EXEC [dbo].[tryToLock] @WorkSessionId, @lockerId, @wasLockedByAnother OUTPUT
		IF @wasLockedByAnother = 0
			SET @State = (SELECT  TOP (1) [State] FROM [dbo].[States] WHERE [WorkSessionId] = @WorkSessionId)
	END
END
GO

CREATE PROCEDURE [dbo].[CheckIn]
@WorkSessionId NVARCHAR(50),
@DocumentId NVARCHAR(max),
@lockerId NVARCHAR(1000),
@State VARBINARY(max),
@Success BIT = 0 OUTPUT
AS
BEGIN
	DECLARE @wasLockedBy NVARCHAR(1000)
	SET @Success = 0

	SET @wasLockedBy = (SELECT [LockerId] FROM [dbo].[Locks] WHERE [WorkSessionId]=@WorkSessionId )

	DECLARE @wasLocked BIT = 0
	IF @wasLockedBy IS NOT NULL
		SET @wasLocked = 1

    DECLARE @wasLockedByMe BIT = 0
	IF @wasLockedBy = @lockerId
		SET @wasLockedByMe = 1
        
	DECLARE @wasLockedByAnother BIT = 0
	IF @wasLocked = 1 AND @wasLockedByMe != 1
		SET @wasLockedByAnother = 1

	IF @wasLockedByAnother != 1
	BEGIN
		EXEC [dbo].[setState] @WorkSessionId, @DocumentId, @State
		SET @Success = 1
	END

	IF @wasLockedByMe = 1
		DELETE FROM [dbo].[Locks] WHERE [WorkSessionId]=@WorkSessionId
END
GO


CREATE PROCEDURE [dbo].[UndoCheckOut]
@WorkSessionId NVARCHAR(50),
@lockerId NVARCHAR(1000),
@wasLockedByAnother BIT = 0 OUTPUT,
@Success BIT = 0 OUTPUT
AS
BEGIN
	DECLARE @wasLockedBy NVARCHAR(1000)
	SET @wasLockedByAnother = 0
	SET @Success = 0

	SET @wasLockedBy = (SELECT [LockerId] FROM [dbo].[Locks] WHERE [WorkSessionId]=@WorkSessionId )

	DECLARE @wasLockedByMe BIT = 0
	IF @wasLockedBy = @lockerId
		SET @wasLockedByMe = 1
	ELSE
	BEGIN
		IF @wasLockedBy != ''
			SET @wasLockedByAnother = 1
	END

	IF @wasLockedByMe = 1
	BEGIN
		DELETE FROM [dbo].[Locks] WHERE WorkSessionId=@WorkSessionId
		SET @Success = 1
	END
END
GO

CREATE PROCEDURE [dbo].[Get]
@Key NVARCHAR(50),
@Value VARBINARY(max) OUTPUT
AS
BEGIN
	SET @Value = (SELECT  TOP (1) [Value] FROM [dbo].[KeyValue] WHERE [Key] = @Key)
END
GO

CREATE PROCEDURE [dbo].[Set]
@Key NVARCHAR(50),
@Value VARBINARY(max)
AS
BEGIN
	IF @Value = ''
		DELETE FROM [dbo].[KeyValue] WHERE [Key]=@Key
	ELSE
	BEGIN
		UPDATE [dbo].[KeyValue] SET [Value]=@Value WHERE [Key]=@Key
		IF @@ROWCOUNT=0
			INSERT INTO [dbo].[KeyValue] ([Key], [Value]) VALUES (@Key, @Value)
	END
END
GO