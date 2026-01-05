IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [AllowanceTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Basis] int NOT NULL,
        [IsEpfApplicable] bit NOT NULL,
        [IsEtfApplicable] bit NOT NULL,
        [IsTaxable] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_AllowanceTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [AttendanceRecords] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [PeriodStart] date NOT NULL,
        [PeriodEnd] date NOT NULL,
        [HoursWorked] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_AttendanceRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditEvents] (
        [Id] uniqueidentifier NOT NULL,
        [TimestampUtc] datetime2 NOT NULL,
        [ActorUserId] nvarchar(100) NOT NULL,
        [ActorDisplayName] nvarchar(200) NULL,
        [EntityType] nvarchar(200) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [Action] nvarchar(200) NOT NULL,
        [BeforeJson] nvarchar(max) NULL,
        [AfterJson] nvarchar(max) NULL,
        [CorrelationId] nvarchar(100) NULL,
        [PreviousHash] nvarchar(128) NULL,
        [Hash] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_AuditEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [DeductionTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Basis] int NOT NULL,
        [IsPreTax] bit NOT NULL,
        [IsPostTax] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_DeductionTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [EmployeePayItems] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [PayItemType] int NOT NULL,
        [PayItemCode] nvarchar(50) NOT NULL,
        [Amount] decimal(18,2) NULL,
        [Percentage] decimal(18,2) NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_EmployeePayItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_EmployeePayItems_AmountOrPercentage] CHECK (((Amount IS NOT NULL AND Percentage IS NULL) OR (Amount IS NULL AND Percentage IS NOT NULL))),
        CONSTRAINT [CK_EmployeePayItems_EffectiveDates] CHECK (([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])),
        CONSTRAINT [CK_EmployeePayItems_PositiveValues] CHECK (((Amount IS NULL OR Amount > 0) AND (Percentage IS NULL OR Percentage > 0)))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [EpfEtfRuleSets] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NULL,
        [EmployeeEpfRate] decimal(5,2) NOT NULL,
        [EmployerEpfRate] decimal(5,2) NOT NULL,
        [EmployerEtfRate] decimal(5,2) NOT NULL,
        [MinimumWageForEpf] decimal(18,2) NULL,
        [MaximumEarningForEpf] decimal(18,2) NULL,
        [MaximumEarningForEtf] decimal(18,2) NULL,
        [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_EpfEtfRuleSets] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [GeneralLedgerAccountMappings] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(64) NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [MappingType] int NOT NULL,
        [DebitAccount] nvarchar(128) NOT NULL,
        [CreditAccount] nvarchar(128) NOT NULL,
        [Notes] nvarchar(1024) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_GeneralLedgerAccountMappings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [LeaveRequests] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [LeaveType] int NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [TotalDays] float NOT NULL,
        [Reason] nvarchar(500) NULL,
        [Status] int NOT NULL,
        [ApprovedById] uniqueidentifier NULL,
        [RequestedAt] datetimeoffset NOT NULL DEFAULT (SYSUTCDATETIME()),
        [ApprovedAt] datetimeoffset NULL,
        [IsHalfDay] bit NULL,
        [HalfDaySession] nvarchar(2) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [Loans] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [PrincipalAmount] decimal(18,2) NOT NULL,
        [OutstandingPrincipal] decimal(18,2) NOT NULL,
        [InstallmentAmount] decimal(18,2) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Loans] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [OTRules] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [WeekdayMultiplier] decimal(18,2) NOT NULL,
        [WeekendMultiplier] decimal(18,2) NOT NULL,
        [HolidayMultiplier] decimal(18,2) NOT NULL,
        [RoundingMinutes] int NOT NULL DEFAULT 0,
        [DailyCapHours] float NOT NULL DEFAULT 0.0E0,
        [PayRunCapHours] float NOT NULL DEFAULT 0.0E0,
        [AppliesOnWeekend] bit NOT NULL DEFAULT CAST(1 AS bit),
        [AppliesOnHoliday] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_OTRules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [OvertimeRecords] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [Date] date NOT NULL,
        [Hours] float NOT NULL,
        [Type] int NOT NULL,
        [Status] int NOT NULL DEFAULT 1,
        [Reason] nvarchar(500) NULL,
        [ApprovedById] uniqueidentifier NULL,
        [ApprovedAt] datetimeoffset NULL,
        [PayRunId] uniqueidentifier NULL,
        [IsLockedForPayroll] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_OvertimeRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [PayrollSettings] (
        [Id] uniqueidentifier NOT NULL,
        [WorkingDaysPerMonth] int NOT NULL,
        [WorkingHoursPerDay] int NOT NULL,
        [NoPayCalculationBasis] int NOT NULL DEFAULT 4,
        [AttendanceHalfDayHours] decimal(5,2) NOT NULL DEFAULT 4.0,
        [WeekdayOvertimeMultiplier] decimal(18,2) NOT NULL DEFAULT 1.5,
        [WeekendOvertimeMultiplier] decimal(18,2) NOT NULL DEFAULT 2.0,
        [HolidayOvertimeMultiplier] decimal(18,2) NOT NULL DEFAULT 2.0,
        [OvertimeRoundingMinutes] int NOT NULL DEFAULT 15,
        [OvertimeDailyCapHours] float NOT NULL DEFAULT 12.0E0,
        [OvertimePayRunCapHours] float NOT NULL DEFAULT 80.0E0,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PayrollSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [TaxRuleSets] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [YearOfAssessment] int NOT NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NULL,
        [IsDefault] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_TaxRuleSets] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Branches_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [RecurringPayItemRules] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [RuleType] int NOT NULL,
        [AllowanceTypeId] uniqueidentifier NULL,
        [DeductionTypeId] uniqueidentifier NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Frequency] int NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [Taxable] bit NOT NULL,
        [EpfEtfContributable] bit NOT NULL,
        [Prorate] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_RecurringPayItemRules] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RecurringPayItemRules_ComponentType] CHECK (((RuleType = 1 AND AllowanceTypeId IS NOT NULL AND DeductionTypeId IS NULL) OR (RuleType = 2 AND DeductionTypeId IS NOT NULL AND AllowanceTypeId IS NULL))),
        CONSTRAINT [CK_RecurringPayItemRules_EffectiveDates] CHECK (([EndDate] IS NULL OR [EndDate] >= [StartDate])),
        CONSTRAINT [FK_RecurringPayItemRules_AllowanceTypes_AllowanceTypeId] FOREIGN KEY ([AllowanceTypeId]) REFERENCES [AllowanceTypes] ([Id]),
        CONSTRAINT [FK_RecurringPayItemRules_DeductionTypes_DeductionTypeId] FOREIGN KEY ([DeductionTypeId]) REFERENCES [DeductionTypes] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [LoanRepayments] (
        [Id] uniqueidentifier NOT NULL,
        [LoanId] uniqueidentifier NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [IsPaid] bit NOT NULL,
        CONSTRAINT [PK_LoanRepayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LoanRepayments_Loans_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Loans] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [TaxReliefs] (
        [Id] uniqueidentifier NOT NULL,
        [TaxRuleSetId] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ReliefType] int NOT NULL,
        [Frequency] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_TaxReliefs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TaxReliefs_TaxRuleSets_TaxRuleSetId] FOREIGN KEY ([TaxRuleSetId]) REFERENCES [TaxRuleSets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [TaxSlabs] (
        [Id] uniqueidentifier NOT NULL,
        [TaxRuleSetId] uniqueidentifier NOT NULL,
        [FromAmount] decimal(18,2) NOT NULL,
        [ToAmount] decimal(18,2) NULL,
        [RatePercent] decimal(5,2) NOT NULL,
        [Order] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_TaxSlabs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TaxSlabs_TaxRuleSets_TaxRuleSetId] FOREIGN KEY ([TaxRuleSetId]) REFERENCES [TaxRuleSets] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [CostCenters] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [CompanyId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CostCenters] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CostCenters_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CostCenters_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeCode] nvarchar(20) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Initials] nvarchar(20) NULL,
        [CallingName] nvarchar(100) NULL,
        [NicNumber] nvarchar(20) NOT NULL,
        [EpfNumber] nvarchar(20) NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Gender] int NOT NULL,
        [MaritalStatus] int NOT NULL,
        [EmploymentStartDate] datetime2 NOT NULL,
        [ProbationEndDate] datetime2 NULL,
        [ConfirmationDate] datetime2 NULL,
        [BaseSalary] decimal(18,2) NOT NULL,
        [BankName] nvarchar(max) NULL,
        [BankCode] nvarchar(max) NULL,
        [BranchCode] nvarchar(max) NULL,
        [BankAccountNumber] nvarchar(max) NULL,
        [CompanyId] uniqueidentifier NULL,
        [BranchId] uniqueidentifier NULL,
        [CostCenterId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_CostCenters_CostCenterId] FOREIGN KEY ([CostCenterId]) REFERENCES [CostCenters] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [PayRuns] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [PeriodType] int NOT NULL,
        [Reference] nvarchar(50) NOT NULL,
        [PeriodStart] datetime2 NOT NULL,
        [PeriodEnd] datetime2 NOT NULL,
        [PayDate] datetime2 NOT NULL,
        [IsLocked] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IsConsolidated] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CompanyId] uniqueidentifier NULL,
        [BranchId] uniqueidentifier NULL,
        [CostCenterId] uniqueidentifier NULL,
        [Status] int NOT NULL,
        [ExportStatus] int NOT NULL,
        [ExportedBank] nvarchar(max) NULL,
        [ExportedAt] datetime2 NULL,
        [ExportDownloadedAt] datetime2 NULL,
        [GeneralLedgerStatus] int NOT NULL,
        [GeneralLedgerReviewedAt] datetime2 NULL,
        [GeneralLedgerReviewedByUserId] nvarchar(max) NULL,
        [GeneralLedgerReviewedByUserName] nvarchar(max) NULL,
        [GeneralLedgerApprovedAt] datetime2 NULL,
        [GeneralLedgerApprovedByUserId] nvarchar(max) NULL,
        [GeneralLedgerApprovedByUserName] nvarchar(max) NULL,
        [GeneralLedgerExportedAt] datetime2 NULL,
        [PreparedAt] datetime2 NULL,
        [PreparedByUserId] nvarchar(100) NULL,
        [PreparedByUserName] nvarchar(200) NULL,
        [ApprovedAt] datetime2 NULL,
        [ApprovedByUserId] nvarchar(100) NULL,
        [ApprovedByUserName] nvarchar(200) NULL,
        [LockedAt] datetime2 NULL,
        [LockedByUserId] nvarchar(100) NULL,
        [LockedByUserName] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PayRuns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayRuns_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PayRuns_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PayRuns_CostCenters_CostCenterId] FOREIGN KEY ([CostCenterId]) REFERENCES [CostCenters] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [EmployeeRecurringPayItems] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [PayItemKind] int NOT NULL,
        [AllowanceTypeId] uniqueidentifier NULL,
        [DeductionTypeId] uniqueidentifier NULL,
        [Amount] decimal(18,2) NULL,
        [Percentage] decimal(18,2) NULL,
        [EffectiveFrom] date NOT NULL,
        [EffectiveTo] date NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_EmployeeRecurringPayItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_EmployeeRecurringPayItems_AmountOrPercentage] CHECK (((Amount IS NOT NULL AND Percentage IS NULL) OR (Amount IS NULL AND Percentage IS NOT NULL))),
        CONSTRAINT [CK_EmployeeRecurringPayItems_EffectiveDates] CHECK (([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])),
        CONSTRAINT [CK_EmployeeRecurringPayItems_KindAndType] CHECK (((PayItemKind = 1 AND AllowanceTypeId IS NOT NULL AND DeductionTypeId IS NULL) OR (PayItemKind = 2 AND DeductionTypeId IS NOT NULL AND AllowanceTypeId IS NULL))),
        CONSTRAINT [CK_EmployeeRecurringPayItems_PositiveValues] CHECK (((Amount IS NULL OR Amount > 0) AND (Percentage IS NULL OR Percentage > 0))),
        CONSTRAINT [FK_EmployeeRecurringPayItems_AllowanceTypes_AllowanceTypeId] FOREIGN KEY ([AllowanceTypeId]) REFERENCES [AllowanceTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EmployeeRecurringPayItems_DeductionTypes_DeductionTypeId] FOREIGN KEY ([DeductionTypeId]) REFERENCES [DeductionTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_EmployeeRecurringPayItems_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [RecurringPayItemAssignments] (
        [Id] uniqueidentifier NOT NULL,
        [RuleId] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_RecurringPayItemAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RecurringPayItemAssignments_EffectiveDates] CHECK (([EndDate] IS NULL OR [EndDate] >= [StartDate])),
        CONSTRAINT [FK_RecurringPayItemAssignments_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RecurringPayItemAssignments_RecurringPayItemRules_RuleId] FOREIGN KEY ([RuleId]) REFERENCES [RecurringPayItemRules] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [RecurringRules] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [RuleType] int NOT NULL,
        [Frequency] int NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [Amount] decimal(18,2) NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [IsEpfApplicable] bit NOT NULL,
        [IsEtfApplicable] bit NOT NULL,
        [IsTaxable] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_RecurringRules] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecurringRules_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [PayRunRecurringLines] (
        [Id] uniqueidentifier NOT NULL,
        [PayRunId] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [RuleId] uniqueidentifier NOT NULL,
        [PaySlipLineId] uniqueidentifier NULL,
        [LineType] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PayRunRecurringLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayRunRecurringLines_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PayRunRecurringLines_PayRuns_PayRunId] FOREIGN KEY ([PayRunId]) REFERENCES [PayRuns] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PayRunRecurringLines_RecurringPayItemRules_RuleId] FOREIGN KEY ([RuleId]) REFERENCES [RecurringPayItemRules] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [PayRunStatusHistory] (
        [Id] uniqueidentifier NOT NULL,
        [PayRunId] uniqueidentifier NOT NULL,
        [FromStatus] int NOT NULL,
        [ToStatus] int NOT NULL,
        [ActorUserId] nvarchar(100) NULL,
        [ActorDisplayName] nvarchar(200) NULL,
        [Comment] nvarchar(1000) NULL,
        [TimestampUtc] datetime2 NOT NULL,
        [PreviousHash] nvarchar(128) NULL,
        [Hash] nvarchar(128) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PayRunStatusHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayRunStatusHistory_PayRuns_PayRunId] FOREIGN KEY ([PayRunId]) REFERENCES [PayRuns] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [PaySlips] (
        [Id] uniqueidentifier NOT NULL,
        [PayRunId] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [BasicSalary] decimal(18,2) NOT NULL,
        [TotalEarnings] decimal(18,2) NOT NULL,
        [TotalDeductions] decimal(18,2) NOT NULL,
        [NetPay] decimal(18,2) NOT NULL,
        [EmployeeEpf] decimal(18,2) NOT NULL,
        [EmployerEpf] decimal(18,2) NOT NULL,
        [EmployerEtf] decimal(18,2) NOT NULL,
        [PayeTax] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PaySlips] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaySlips_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PaySlips_PayRuns_PayRunId] FOREIGN KEY ([PayRunId]) REFERENCES [PayRuns] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [StatutoryReports] (
        [Id] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [PayRunId] uniqueidentifier NOT NULL,
        [PeriodStart] datetime2 NOT NULL,
        [PeriodEnd] datetime2 NOT NULL,
        [GeneratedAtUtc] datetime2 NOT NULL,
        [GeneratedBy] nvarchar(200) NOT NULL,
        [Status] int NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [FileName] nvarchar(200) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [Checksum] nvarchar(128) NOT NULL,
        [WarningCount] int NOT NULL,
        [WarningFilePath] nvarchar(500) NULL,
        [WarningFileName] nvarchar(200) NULL,
        [WarningContentType] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [ModifiedAt] datetime2 NULL,
        [ModifiedBy] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_StatutoryReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StatutoryReports_PayRuns_PayRunId] FOREIGN KEY ([PayRunId]) REFERENCES [PayRuns] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [DeductionLines] (
        [Id] uniqueidentifier NOT NULL,
        [PaySlipId] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [Source] nvarchar(50) NOT NULL DEFAULT N'',
        [Amount] decimal(18,2) NOT NULL,
        [IsPreTax] bit NOT NULL,
        [IsPostTax] bit NOT NULL,
        CONSTRAINT [PK_DeductionLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DeductionLines_PaySlips_PaySlipId] FOREIGN KEY ([PaySlipId]) REFERENCES [PaySlips] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE TABLE [EarningLines] (
        [Id] uniqueidentifier NOT NULL,
        [PaySlipId] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [IsEpfApplicable] bit NOT NULL,
        [IsEtfApplicable] bit NOT NULL,
        [IsTaxable] bit NOT NULL,
        CONSTRAINT [PK_EarningLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EarningLines_PaySlips_PaySlipId] FOREIGN KEY ([PaySlipId]) REFERENCES [PaySlips] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Basis', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'IsEpfApplicable', N'IsEtfApplicable', N'IsTaxable', N'ModifiedAt', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[AllowanceTypes]'))
        SET IDENTITY_INSERT [AllowanceTypes] ON;
    EXEC(N'INSERT INTO [AllowanceTypes] ([Id], [Basis], [Code], [CreatedAt], [CreatedBy], [Description], [IsActive], [IsEpfApplicable], [IsEtfApplicable], [IsTaxable], [ModifiedAt], [ModifiedBy], [Name])
    VALUES (''11111111-1111-1111-1111-111111111111'', 1, N''BASIC'', ''2020-01-01T00:00:00.0000000Z'', N''system'', NULL, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), NULL, NULL, N''Basic Salary''),
    (''22222222-2222-2222-2222-222222222222'', 1, N''TRA'', ''2020-01-01T00:00:00.0000000Z'', N''system'', NULL, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), NULL, NULL, N''Transport Allowance''),
    (''33333333-3333-3333-3333-333333333333'', 1, N''ATD'', ''2020-01-01T00:00:00.0000000Z'', N''system'', NULL, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), NULL, NULL, N''Attendance Allowance'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Basis', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'IsEpfApplicable', N'IsEtfApplicable', N'IsTaxable', N'ModifiedAt', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[AllowanceTypes]'))
        SET IDENTITY_INSERT [AllowanceTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Basis', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'IsPostTax', N'IsPreTax', N'ModifiedAt', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[DeductionTypes]'))
        SET IDENTITY_INSERT [DeductionTypes] ON;
    EXEC(N'INSERT INTO [DeductionTypes] ([Id], [Basis], [Code], [CreatedAt], [CreatedBy], [Description], [IsActive], [IsPostTax], [IsPreTax], [ModifiedAt], [ModifiedBy], [Name])
    VALUES (''44444444-4444-4444-4444-444444444444'', 1, N''EPF_EE'', ''2020-01-01T00:00:00.0000000Z'', N''system'', NULL, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), NULL, NULL, N''Employee EPF''),
    (''55555555-5555-5555-5555-555555555555'', 1, N''LOAN'', ''2020-01-01T00:00:00.0000000Z'', N''system'', NULL, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), NULL, NULL, N''Loan Installment''),
    (''66666666-6666-6666-6666-666666666666'', 1, N''NOPAY'', ''2020-01-01T00:00:00.0000000Z'', N''system'', NULL, CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), NULL, NULL, N''No Pay Deduction''),
    (''77777777-7777-7777-7777-777777777777'', 1, N''PAYE'', ''2020-01-01T00:00:00.0000000Z'', N''system'', NULL, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL, NULL, N''PAYE Tax'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Basis', N'Code', N'CreatedAt', N'CreatedBy', N'Description', N'IsActive', N'IsPostTax', N'IsPreTax', N'ModifiedAt', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[DeductionTypes]'))
        SET IDENTITY_INSERT [DeductionTypes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'EffectiveFrom', N'EffectiveTo', N'EmployeeEpfRate', N'EmployerEpfRate', N'EmployerEtfRate', N'IsActive', N'IsDefault', N'MaximumEarningForEpf', N'MaximumEarningForEtf', N'MinimumWageForEpf', N'ModifiedAt', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[EpfEtfRuleSets]'))
        SET IDENTITY_INSERT [EpfEtfRuleSets] ON;
    EXEC(N'INSERT INTO [EpfEtfRuleSets] ([Id], [CreatedAt], [CreatedBy], [EffectiveFrom], [EffectiveTo], [EmployeeEpfRate], [EmployerEpfRate], [EmployerEtfRate], [IsActive], [IsDefault], [MaximumEarningForEpf], [MaximumEarningForEtf], [MinimumWageForEpf], [ModifiedAt], [ModifiedBy], [Name])
    VALUES (''88888888-8888-8888-8888-888888888888'', ''2020-01-01T00:00:00.0000000Z'', N''system'', ''2020-01-01'', NULL, 8.0, 12.0, 3.0, CAST(1 AS bit), CAST(1 AS bit), NULL, NULL, NULL, NULL, NULL, N''Sri Lanka Default EPF/ETF'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'EffectiveFrom', N'EffectiveTo', N'EmployeeEpfRate', N'EmployerEpfRate', N'EmployerEtfRate', N'IsActive', N'IsDefault', N'MaximumEarningForEpf', N'MaximumEarningForEtf', N'MinimumWageForEpf', N'ModifiedAt', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[EpfEtfRuleSets]'))
        SET IDENTITY_INSERT [EpfEtfRuleSets] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'EffectiveFrom', N'EffectiveTo', N'IsActive', N'IsDefault', N'ModifiedAt', N'ModifiedBy', N'Name', N'YearOfAssessment') AND [object_id] = OBJECT_ID(N'[TaxRuleSets]'))
        SET IDENTITY_INSERT [TaxRuleSets] ON;
    EXEC(N'INSERT INTO [TaxRuleSets] ([Id], [CreatedAt], [CreatedBy], [EffectiveFrom], [EffectiveTo], [IsActive], [IsDefault], [ModifiedAt], [ModifiedBy], [Name], [YearOfAssessment])
    VALUES (''99999999-9999-9999-9999-999999999999'', ''2020-01-01T00:00:00.0000000Z'', N''system'', ''2025-04-01'', NULL, CAST(1 AS bit), CAST(1 AS bit), NULL, NULL, N''Sri Lanka PAYE YA 2025/26'', 2025)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'EffectiveFrom', N'EffectiveTo', N'IsActive', N'IsDefault', N'ModifiedAt', N'ModifiedBy', N'Name', N'YearOfAssessment') AND [object_id] = OBJECT_ID(N'[TaxRuleSets]'))
        SET IDENTITY_INSERT [TaxRuleSets] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'FromAmount', N'IsActive', N'ModifiedAt', N'ModifiedBy', N'Order', N'RatePercent', N'TaxRuleSetId', N'ToAmount') AND [object_id] = OBJECT_ID(N'[TaxSlabs]'))
        SET IDENTITY_INSERT [TaxSlabs] ON;
    EXEC(N'INSERT INTO [TaxSlabs] ([Id], [CreatedAt], [CreatedBy], [FromAmount], [IsActive], [ModifiedAt], [ModifiedBy], [Order], [RatePercent], [TaxRuleSetId], [ToAmount])
    VALUES (''aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1'', ''2020-01-01T00:00:00.0000000Z'', N''system'', 0.0, CAST(1 AS bit), NULL, NULL, 1, 0.0, ''99999999-9999-9999-9999-999999999999'', 100000.0),
    (''aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2'', ''2020-01-01T00:00:00.0000000Z'', N''system'', 100000.0, CAST(1 AS bit), NULL, NULL, 2, 6.0, ''99999999-9999-9999-9999-999999999999'', 141667.0),
    (''aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3'', ''2020-01-01T00:00:00.0000000Z'', N''system'', 141667.0, CAST(1 AS bit), NULL, NULL, 3, 12.0, ''99999999-9999-9999-9999-999999999999'', 183333.0),
    (''aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4'', ''2020-01-01T00:00:00.0000000Z'', N''system'', 183333.0, CAST(1 AS bit), NULL, NULL, 4, 18.0, ''99999999-9999-9999-9999-999999999999'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'CreatedBy', N'FromAmount', N'IsActive', N'ModifiedAt', N'ModifiedBy', N'Order', N'RatePercent', N'TaxRuleSetId', N'ToAmount') AND [object_id] = OBJECT_ID(N'[TaxSlabs]'))
        SET IDENTITY_INSERT [TaxSlabs] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AllowanceTypes_Code] ON [AllowanceTypes] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditEvents_Action] ON [AuditEvents] ([Action]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditEvents_EntityType_EntityId] ON [AuditEvents] ([EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditEvents_TimestampUtc] ON [AuditEvents] ([TimestampUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_Code] ON [Branches] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Branches_CompanyId] ON [Branches] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Companies_Code] ON [Companies] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CostCenters_BranchId] ON [CostCenters] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CostCenters_Code] ON [CostCenters] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CostCenters_CompanyId] ON [CostCenters] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DeductionLines_PaySlipId] ON [DeductionLines] ([PaySlipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DeductionTypes_Code] ON [DeductionTypes] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EarningLines_PaySlipId] ON [EarningLines] ([PaySlipId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmployeePayItems_EmployeeId_PayItemCode_PayItemType_EffectiveFrom] ON [EmployeePayItems] ([EmployeeId], [PayItemCode], [PayItemType], [EffectiveFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmployeeRecurringPayItems_AllowanceTypeId] ON [EmployeeRecurringPayItems] ([AllowanceTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmployeeRecurringPayItems_DeductionTypeId] ON [EmployeeRecurringPayItems] ([DeductionTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmployeeRecurringPayItems_EmployeeId] ON [EmployeeRecurringPayItems] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmployeeRecurringPayItems_EmployeeId_PayItemKind_AllowanceTypeId_DeductionTypeId_EffectiveFrom] ON [EmployeeRecurringPayItems] ([EmployeeId], [PayItemKind], [AllowanceTypeId], [DeductionTypeId], [EffectiveFrom]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_BranchId] ON [Employees] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_CompanyId] ON [Employees] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_CostCenterId] ON [Employees] ([CostCenterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_NicNumber] ON [Employees] ([NicNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EpfEtfRuleSets_EffectiveFrom_IsActive_IsDefault] ON [EpfEtfRuleSets] ([EffectiveFrom], [IsActive], [IsDefault]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GeneralLedgerAccountMappings_Code_MappingType] ON [GeneralLedgerAccountMappings] ([Code], [MappingType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_EmployeeId_StartDate] ON [LeaveRequests] ([EmployeeId], [StartDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LoanRepayments_LoanId] ON [LoanRepayments] ([LoanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Loans_EmployeeId_Status] ON [Loans] ([EmployeeId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OvertimeRecords_EmployeeId_Date] ON [OvertimeRecords] ([EmployeeId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PayRunRecurringLines_EmployeeId] ON [PayRunRecurringLines] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PayRunRecurringLines_PayRunId_EmployeeId_RuleId] ON [PayRunRecurringLines] ([PayRunId], [EmployeeId], [RuleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PayRunRecurringLines_RuleId] ON [PayRunRecurringLines] ([RuleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PayRuns_BranchId] ON [PayRuns] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PayRuns_Code] ON [PayRuns] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PayRuns_CompanyId] ON [PayRuns] ([CompanyId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PayRuns_CostCenterId] ON [PayRuns] ([CostCenterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PayRuns_Reference] ON [PayRuns] ([Reference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PayRunStatusHistory_PayRunId] ON [PayRunStatusHistory] ([PayRunId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PaySlips_EmployeeId] ON [PaySlips] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PaySlips_PayRunId] ON [PaySlips] ([PayRunId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecurringPayItemAssignments_EmployeeId] ON [RecurringPayItemAssignments] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecurringPayItemAssignments_RuleId_EmployeeId] ON [RecurringPayItemAssignments] ([RuleId], [EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecurringPayItemRules_AllowanceTypeId] ON [RecurringPayItemRules] ([AllowanceTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecurringPayItemRules_DeductionTypeId] ON [RecurringPayItemRules] ([DeductionTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RecurringRules_EmployeeId] ON [RecurringRules] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatutoryReports_GeneratedAtUtc] ON [StatutoryReports] ([GeneratedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatutoryReports_PayRunId_Type] ON [StatutoryReports] ([PayRunId], [Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TaxReliefs_TaxRuleSetId] ON [TaxReliefs] ([TaxRuleSetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TaxRuleSets_YearOfAssessment_IsActive_IsDefault] ON [TaxRuleSets] ([YearOfAssessment], [IsActive], [IsDefault]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TaxSlabs_TaxRuleSetId] ON [TaxSlabs] ([TaxRuleSetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219164607_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251219164607_InitialCreate', N'8.0.8');
END;
GO

COMMIT;
GO

