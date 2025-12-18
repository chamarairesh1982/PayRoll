using Payroll.Application.PayrollConfig.DTOs;

namespace Payroll.Application.PayrollConfig;

public interface IOvertimeRuleService
{
    Task<OvertimeRuleConfigDto> GetAsync(CancellationToken ct = default);

    Task<OvertimeRuleConfigDto> UpdateAsync(UpdateOvertimeRuleConfigRequest request, CancellationToken ct = default);
}
