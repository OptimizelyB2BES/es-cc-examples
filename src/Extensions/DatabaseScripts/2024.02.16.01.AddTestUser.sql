--------------------------------------------------------------------------------
-- Add Test User and all related tables to enable unit testing
--------------------------------------------------------------------------------
DECLARE @webUserEmail NVARCHAR(100)
    , @webUserRolePostfix NVARCHAR(100)
    , @webUserRole NVARCHAR(100)
    , @webUserEmailDomain NVARCHAR(100)
    , @webUserFirstName NVARCHAR(100)
    , @webUserLastName NVARCHAR(100)
    , @websiteName NVARCHAR(100);
DECLARE @customerAssignmentTable TABLE (
    CustomerNumber VARCHAR(10)
    , CustomerSequence VARCHAR(10)
    );

--------------------------------------------------------------------------------
-- SET THESE
--------------------------------------------------------------------------------
SET @webUserEmail = 'b2b-expertservices'
SET @webUserRolePostfix = '+admin'
SET @webUserRole = 'administrator'
SET @webUserEmailDomain = '@optimizely.com'
SET @webUserFirstName = 'Test'
SET @webUserLastName = 'B2B-ES'
SET @websiteName = 'main'

INSERT INTO @customerAssignmentTable
VALUES (
    '11251000'
    , ''
    )
    , (
    '11251000'
    , '72381723'
    )
    , (
    '701112'
    , ''
    )
    , (
    '701112'
    , '75671810'
    )
    , (
    '314470'
    , ''
    )
    , (
    '314470'
    , '314470'
    )

--------------------------------------------------------------------------------
-- END SET
--------------------------------------------------------------------------------
DECLARE @username NVARCHAR(100)
    , @userEmail NVARCHAR(100)
    , @aspNetUserId UNIQUEIDENTIFIER
    , @websiteId UNIQUEIDENTIFIER
    , @userProfileId UNIQUEIDENTIFIER;

SET @userEmail = @webUserEmail + @webUserRolePostfix + @webUserEmailDomain
SET @username = @userEmail

IF EXISTS (
        SELECT 1
        FROM AspNetUsers
        WHERE (1 = 1)
            AND @username = UserName
        )
BEGIN
    PRINT 'User exists. Continue...'
END
ELSE
BEGIN
    PRINT 'User does not exist. Creating...';

    INSERT INTO AspNetUsers (
        Id
        , Email
        , EmailConfirmed
        , PasswordHash
        , SecurityStamp
        , PhoneNumber
        , PhoneNumberConfirmed
        , TwoFactorEnabled
        , LockoutEndDateUtc
        , LockoutEnabled
        , AccessFailedCount
        , UserName
        , Hometown
        , ConcurrencyStamp
        , LockoutEnd
        , NormalizedEmail
        , NormalizedUserName
        )
    VALUES (
        NEWID()
        , @userEmail
        , 1
        , NULL
        , NULL
        , NULL
        , 0
        , 0
        , '2021-01-01 00:00:00.001'
        , 0
        , 0
        , @username
        , NULL
        , NULL
        , NULL
        , UPPER(@userEmail)
        , UPPER(@username)
        )
END

SET @aspNetUserId = (
        SELECT Id
        FROM AspNetUsers
        WHERE (1 = 1)
            AND username = @username
        )

PRINT 'AspNetUserId'
PRINT @aspNetUserId

IF EXISTS (
        SELECT 1
        FROM AspNetUserRoles ur
        JOIN AspNetRoles r ON r.Id = ur.RoleId
        WHERE (1 = 1)
            AND ur.UserId = @aspNetUserId
            AND r.Name = @webUserRole
        )
BEGIN
    PRINT 'User role assigned. Continue...'
END
ELSE
BEGIN
    PRINT 'User role not assigned. Creating...'

    INSERT INTO AspNetUserRoles (
        UserId
        , RoleId
        )
    VALUES (
        @aspNetUserId
        , (
            SELECT Id
            FROM AspNetRoles
            WHERE (1 = 1)
                AND Name = @webUserRole
            )
        )
END

IF EXISTS (
        SELECT 1
        FROM UserProfile
        WHERE (1 = 1)
            AND Email = @userEmail
        )
BEGIN
    PRINT 'User profile exists. Continue...'
END
ELSE
BEGIN
    PRINT 'User profile does not exist. Creating...'

    INSERT INTO UserProfile (
        Id
        , FirstName
        , LastName
        , Company
        , Phone
        , Extension
        , Fax
        , Position
        , UserName
        , IsGuest
        , Email
        , DefaultCustomerId
        , ApproverUserProfileId
        , LimitExceededNotification
        , DashboardIsHomepage
        , CurrencyId
        , IsPasswordChangeRequired
        , HasRfqUpdates
        , LanguageId
        , CreatedOn
        , CreatedBy
        , ModifiedOn
        , ModifiedBy
        , PasswordChangedOn
        , ActivationStatus
        , LastLoginOn
        , IsDeactivated
        , LastReactivatedOn
        , LastActivationEmailSentOn
        , RecentlyViewedProducts
        , DefaultWarehouseId
        , DefaultFulfillmentMethod
        , EmailOptOutAbandonedCart
        , PunchOutAcceptPriceProvided
        , PunchOutUseProvidedShipToId
        , CarryOverUnusedMonthlyBalances
        , DisplayPricingAndInventory
        )
    VALUES (
        'c2b32945-f001-422e-98d2-b0a5014784ca'
        , @webUserFirstName
        , @webUserLastName
        , 'Optimizely'
        , '9522424977'
        , ''
        , ''
        , ''
        , @username
        , 0
        , @userEmail
        , NULL
        , NULL
        , 0
        , 0
        , NULL
        , 0
        , 0
        , NULL
        , '2023-10-24 19:52:27.5485197 +00:00'
        , 'admin_is\dhartjes'
        , '2023-11-13 20:37:35.1030197 +00:00'
        , @username
        , '2023-10-24 19:53:02.7871717 +00:00'
        , 'Activated'
        , '2023-11-13 14:37:34.5799610 -06:00'
        , 0
        , '2023-10-24 19:52:11.3543634 +00:00'
        , NULL
        , ''
        , NULL
        , ''
        , 0
        , 1
        , 1
        , 1
        , 0
        )
END

SET @userProfileId = (
        SELECT Id
        FROM UserProfile
        WHERE (1 = 1)
            AND Email = @userEmail
        )

IF NOT EXISTS (
        SELECT 1
        FROM Website
        WHERE (1 = 1)
            AND Name = @websiteName
        )
BEGIN
    PRINT 'Website with name ' + @websiteName + ' does not exist. Cannot assign website. Continue...'
END
ELSE
BEGIN
    SET @websiteId = (
            SELECT Id
            FROM Website
            WHERE (1 = 1)
                AND Name = @websiteName
            )

    PRINT 'WebsiteId:'
    PRINT @websiteId

    IF EXISTS (
            SELECT 1
            FROM UserProfileWebsite upw
            WHERE (1 = 1)
                AND upw.UserProfileId = @userProfileId
                AND upw.WebsiteId = @websiteId
            )
    BEGIN
        PRINT 'Website is assigned to user profile. Continue...'
    END
    ELSE
    BEGIN
        PRINT 'Website is not assigned to user profile. Creating...'

        INSERT INTO UserProfileWebsite (
            UserProfileId
            , WebsiteId
            , CreatedOn
            , CreatedBy
            , ModifiedOn
            , ModifiedBy
            )
        VALUES (
            @userProfileId
            , @websiteId
            , '2023-10-24 20:17:38.5966667 +00:00'
            , 'AddTestUserScript'
            , GETUTCDATE()
            , 'AddTestUserScript'
            )
    END
END

INSERT INTO CustomerUserProfile (
    CustomerId
    , UserProfileId
    , DefaultCostCode
    , IsDefault
    , CreatedOn
    , CreatedBy
    , ModifiedOn
    , ModifiedBy
    )
SELECT c.Id
    , @userProfileId
    , ''
    , 0
    , '2021-01-01 00:00:00.001'
    , 'AddTestUserScript'
    , GETUTCDATE()
    , 'AddTestUserScript'
FROM Customer c
WHERE (1 = 1)
    AND Id IN (
        SELECT c.Id
        FROM @customerAssignmentTable cat
        JOIN Customer c ON c.CustomerNumber = cat.CustomerNumber
            AND c.CustomerSequence = cat.CustomerSequence
        )
    AND Id NOT IN (
        SELECT CustomerId
        FROM CustomerUserProfile cup
        WHERE (1 = 1)
            AND UserProfileId = @userProfileId
        )

PRINT 'Exiting...'
