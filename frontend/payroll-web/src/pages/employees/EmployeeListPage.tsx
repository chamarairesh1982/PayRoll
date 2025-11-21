import DataTable from "../../components/common/DataTable";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import { Employee } from "../../types/Employee";

const EmployeeListPage = () => {
  const employees: Employee[] = [];
  return (
    <section>
      <h1>Employees</h1>
      {employees.length === 0 ? (
        <LoadingSpinner />
      ) : (
        <DataTable
          headers={["Employee #", "Name", "Type", "Status"]}
          rows={employees.map((employee) => (
            <tr key={employee.id}>
              <td>{employee.employeeNumber}</td>
              <td>{employee.fullName}</td>
              <td>{employee.type}</td>
              <td>{employee.status}</td>
            </tr>
          ))}
        />
      )}
    </section>
  );
};

export default EmployeeListPage;
