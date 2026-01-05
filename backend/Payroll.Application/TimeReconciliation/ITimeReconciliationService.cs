namespace Payroll.Application.TimeReconciliation;

public interface ITimeReconciliationService
{
    TimeReconciliationEmployeeResult ReconcileEmployee(TimeReconciliationEmployeeInput input);
}
