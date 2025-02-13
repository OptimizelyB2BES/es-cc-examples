-- Create property configuration record.
DECLARE @entityConfigId UNIQUEIDENTIFIER;
DECLARE @propertyConfigId UNIQUEIDENTIFIER;

-- Function to insert PropertyConfiguration and PropertyPermission
DECLARE @entityNames TABLE (Name NVARCHAR(50));
INSERT INTO @entityNames
  (Name)
VALUES
  ('product'),
  ('orderLine');

DECLARE @entityName NVARCHAR(50);

DECLARE entity_cursor CURSOR FOR
SELECT Name
FROM @entityNames;

OPEN entity_cursor;
FETCH NEXT FROM entity_cursor INTO @entityName;

WHILE @@FETCH_STATUS = 0
BEGIN
  SET @entityConfigId = (SELECT Id
  FROM AppDict.EntityConfiguration
  WHERE name = @entityName);

  IF NOT EXISTS (SELECT 1
  FROM [AppDict].[PropertyConfiguration]
  WHERE [Name] = 'DirectShip' AND EntityConfigurationId = @entityConfigId)
    BEGIN
    INSERT INTO [AppDict].[PropertyConfiguration]
      ([EntityConfigurationId]
      ,[Name]
      ,[ParentProperty]
      ,[Label]
      ,[ControlType]
      ,[IsRequired]
      ,[IsTranslatable]
      ,[HintText]
      ,[CreatedOn]
      ,[CreatedBy]
      ,[ModifiedOn]
      ,[ModifiedBy]
      ,[ToolTip]
      ,[PropertyType]
      ,[IsCustomProperty]
      ,[CanView]
      ,[CanEdit]
      ,[HintTextPropertyName]
      ,[SyncToPim])
    VALUES
      (@entityConfigId
                    , 'DirectShip'
                    , ''
                    , 'DirectShip'
                    , 'Insite.Admin.ControlTypes.ToggleSwitchControl'
                    , 0
                    , 0
                    , ''
                    , GETUTCDATE()
                    , ''
                    , GETUTCDATE()
                    , ''
                    , ''
                    , 'System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
                    , 1
                    , 1
                    , 1
                    , ''
                    , 0);

    SET @propertyConfigId = (SELECT Id
    FROM [AppDict].[PropertyConfiguration]
    WHERE [Name] = 'DirectShip' AND [EntityConfigurationId] = @entityConfigId);

    INSERT INTO [AppDict].[PropertyPermission]
      ([PropertyConfigurationId]
      ,[RoleName]
      ,[CreatedBy]
      ,[CreatedOn]
      ,[ModifiedBy]
      ,[ModifiedOn]
      ,[CanView]
      ,[CanEdit])
    VALUES
      (@propertyConfigId
               , 'ISC_StoreFrontApi'
               , ''
               , GETUTCDATE()
               , ''
               , GETUTCDATE()
               , 1
               , 1);
  END

  FETCH NEXT FROM entity_cursor INTO @entityName;
END;

CLOSE entity_cursor;
DEALLOCATE entity_cursor;