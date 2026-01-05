namespace Payroll.Application.DTOs.Organizations;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class BranchDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
}

public class CostCenterDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
}
