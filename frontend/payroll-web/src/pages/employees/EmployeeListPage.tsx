import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import DataTable from "../../components/common/DataTable";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import Pagination from "../../components/common/Pagination";
import { deleteEmployee, getEmployees } from "../../api/employeesApi";
import { Employee } from "../../types/Employee";

const PAGE_SIZE = 10;

const EmployeeListPage = () => {
  const navigate = useNavigate();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const totalPages = useMemo(
    () => Math.max(1, Math.ceil(totalCount / PAGE_SIZE)),
    [totalCount]
  );

  useEffect(() => {
    const fetchEmployees = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await getEmployees(page, PAGE_SIZE);
        setEmployees(response.items);
        setTotalCount(response.totalCount);
      } catch (err) {
        setError((err as Error).message || "Failed to load employees");
      } finally {
        setLoading(false);
      }
    };

    fetchEmployees();
  }, [page]);

  const handleDelete = async (id: string) => {
    const confirmed = window.confirm("Are you sure you want to delete this employee?");
    if (!confirmed) return;

    try {
      await deleteEmployee(id);
      const isLastItemOnPage = employees.length === 1 && page > 1;
      const nextPage = isLastItemOnPage ? page - 1 : page;
      setPage(nextPage);
      const response = await getEmployees(nextPage, PAGE_SIZE);
      setEmployees(response.items);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError((err as Error).message || "Failed to delete employee");
    }
  };

  return (
    <section>
      <div className="page-header">
        <h1>Employees</h1>
        <button onClick={() => navigate("/employees/new")}>Create Employee</button>
      </div>

      {error && <div className="error">{error}</div>}

      {loading ? (
        <LoadingSpinner />
      ) : employees.length === 0 ? (
        <p>No employees found.</p>
      ) : (
        <>
          <DataTable
            headers={["Employee Code", "Name", "NIC", "Employment Start", "Base Salary", "Status", "Actions"]}
            rows={employees.map((employee) => (
              <tr key={employee.id}>
                <td>{employee.employeeCode}</td>
                <td>
                  {employee.firstName} {employee.lastName}
                </td>
                <td>{employee.nicNumber}</td>
                <td>{employee.employmentStartDate ? new Date(employee.employmentStartDate).toLocaleDateString() : "-"}</td>
                <td>{employee.baseSalary.toLocaleString()}</td>
                <td>{employee.isActive ? "Active" : "Inactive"}</td>
                <td>
                  <div className="table-actions">
                    <button onClick={() => navigate(`/employees/${employee.id}`)}>View</button>
                    <button onClick={() => navigate(`/employees/${employee.id}/edit`)}>Edit</button>
                    <button onClick={() => handleDelete(employee.id)}>Delete</button>
                  </div>
                </td>
              </tr>
            ))}
          />
          {totalPages > 1 && (
            <Pagination page={page} totalPages={totalPages} onChange={setPage} />
          )}
        </>
      )}
    </section>
  );
};

export default EmployeeListPage;
