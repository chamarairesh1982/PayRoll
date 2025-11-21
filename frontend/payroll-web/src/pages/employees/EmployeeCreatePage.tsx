import { useNavigate } from "react-router-dom";
import EmployeeForm from "../../components/forms/EmployeeForm";
import { createEmployee } from "../../api/employeesApi";
import { Employee } from "../../types/Employee";

const EmployeeCreatePage = () => {
  const navigate = useNavigate();

  const handleSubmit = async (values: Partial<Employee>) => {
    await createEmployee(values);
    navigate("/employees");
  };

  return (
    <section>
      <h1>Create Employee</h1>
      <EmployeeForm mode="create" onSubmit={handleSubmit} />
    </section>
  );
};

export default EmployeeCreatePage;
