import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getEmployee } from "../../api/employeesApi";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import { Employee } from "../../types/Employee";

const EmployeeDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchEmployee = async () => {
      if (!id) return;
      setLoading(true);
      setError(null);
      try {
        const response = await getEmployee(id);
        setEmployee(response);
      } catch (err) {
        setError((err as Error).message || "Failed to load employee");
      } finally {
        setLoading(false);
      }
    };

    fetchEmployee();
  }, [id]);

  if (!id) return <p>Employee not found.</p>;
  if (loading) return <LoadingSpinner />;
  if (error) return <div className="error">{error}</div>;
  if (!employee) return <p>Employee not found.</p>;

  return (
    <section>
      <div className="page-header">
        <h1>Employee Details</h1>
        <div className="page-actions">
          <button onClick={() => navigate(`/employees/${employee.id}/edit`)}>Edit</button>
          <button onClick={() => navigate("/employees")}>Back</button>
        </div>
      </div>

      <div className="detail-grid">
        <div>
          <strong>Employee Code:</strong> {employee.employeeCode}
        </div>
        <div>
          <strong>Name:</strong> {employee.firstName} {employee.lastName}
        </div>
        <div>
          <strong>Initials:</strong> {employee.initials || "-"}
        </div>
        <div>
          <strong>Calling Name:</strong> {employee.callingName || "-"}
        </div>
        <div>
          <strong>NIC:</strong> {employee.nicNumber}
        </div>
        <div>
          <strong>Date of Birth:</strong> {new Date(employee.dateOfBirth).toLocaleDateString()}
        </div>
        <div>
          <strong>Gender:</strong> {employee.gender}
        </div>
        <div>
          <strong>Marital Status:</strong> {employee.maritalStatus}
        </div>
        <div>
          <strong>Employment Start Date:</strong> {new Date(employee.employmentStartDate).toLocaleDateString()}
        </div>
        <div>
          <strong>Probation End Date:</strong>
          {employee.probationEndDate ? new Date(employee.probationEndDate).toLocaleDateString() : "-"}
        </div>
        <div>
          <strong>Confirmation Date:</strong>
          {employee.confirmationDate ? new Date(employee.confirmationDate).toLocaleDateString() : "-"}
        </div>
        <div>
          <strong>Base Salary:</strong> {employee.baseSalary.toLocaleString()}
        </div>
        <div>
          <strong>Status:</strong> {employee.isActive ? "Active" : "Inactive"}
        </div>
      </div>
    </section>
  );
};

export default EmployeeDetailPage;
