import { useState } from "react";
import { Employee } from "../../types/Employee";

type Props = {
  onSubmit: (employee: Partial<Employee>) => void;
};

const EmployeeForm = ({ onSubmit }: Props) => {
  const [form, setForm] = useState<Partial<Employee>>({});

  const handleChange = (key: keyof Employee) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [key]: event.target.value });
  };

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit(form);
      }}
    >
      <label>
        Full name
        <input value={form.fullName ?? ""} onChange={handleChange("fullName")}
        />
      </label>
      <button type="submit">Save</button>
    </form>
  );
};

export default EmployeeForm;
