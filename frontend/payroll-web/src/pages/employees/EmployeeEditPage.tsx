import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import EmployeeForm from "../../components/forms/EmployeeForm";
import { getEmployee, updateEmployee } from "../../api/employeesApi";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import { Employee } from "../../types/Employee";

const EmployeeEditPage = () => {
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

  const handleSubmit = async (values: Partial<Employee>) => {
    if (!id) return;
    await updateEmployee(id, values);
    navigate("/employees");
  };

  if (!id) {
    return <p>Employee not found.</p>;
  }

  if (loading) return <LoadingSpinner />;
  if (error) return <div className="error">{error}</div>;
  if (!employee) return <p>Employee not found.</p>;

  return (
    <section>
      <h1>Edit Employee</h1>
      <EmployeeForm mode="edit" initialValues={employee} onSubmit={handleSubmit} />
    </section>
  );
};

export default EmployeeEditPage;
