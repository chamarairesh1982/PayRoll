import EmployeeForm from "../../components/forms/EmployeeForm";
import { Employee } from "../../types/Employee";

const EmployeeCreatePage = () => {
  const handleSubmit = (employee: Partial<Employee>) => {
    // TODO: call API to create employee
    console.log("Create employee", employee);
  };

  return (
    <section>
      <h1>Create employee</h1>
      <EmployeeForm onSubmit={handleSubmit} />
    </section>
  );
};

export default EmployeeCreatePage;
