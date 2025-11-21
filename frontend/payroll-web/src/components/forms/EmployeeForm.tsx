import { useEffect, useMemo, useState } from "react";
import { Employee } from "../../types/Employee";

export type EmployeeFormProps = {
  initialValues?: Partial<Employee>;
  mode: "create" | "edit";
  onSubmit: (values: Partial<Employee>) => Promise<void>;
};

type ValidationErrors = Partial<Record<keyof Employee, string>> & { baseSalary?: string };

type FormState = Partial<Employee> & { baseSalary?: number };

const defaultValues: FormState = {
  employeeCode: "",
  firstName: "",
  lastName: "",
  nicNumber: "",
  dateOfBirth: "",
  gender: "Male",
  maritalStatus: "Single",
  employmentStartDate: "",
  probationEndDate: "",
  confirmationDate: "",
  baseSalary: 0,
  isActive: true
};

const EmployeeForm = ({ initialValues, mode, onSubmit }: EmployeeFormProps) => {
  const [values, setValues] = useState<FormState>({ ...defaultValues, ...initialValues });
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    setValues((prev) => ({ ...prev, ...initialValues }));
  }, [initialValues]);

  const displayDate = useMemo(() => {
    const normalizeDate = (date?: string | null) =>
      date ? new Date(date).toISOString().split("T")[0] : "";

    return {
      dateOfBirth: normalizeDate(values.dateOfBirth),
      employmentStartDate: normalizeDate(values.employmentStartDate),
      probationEndDate: normalizeDate(values.probationEndDate ?? undefined),
      confirmationDate: normalizeDate(values.confirmationDate ?? undefined)
    };
  }, [values.confirmationDate, values.dateOfBirth, values.employmentStartDate, values.probationEndDate]);

  const validate = (formValues: FormState) => {
    const validationErrors: ValidationErrors = {};

    if (!formValues.employeeCode) validationErrors.employeeCode = "Employee Code is required";
    if (!formValues.firstName) validationErrors.firstName = "First Name is required";
    if (!formValues.lastName) validationErrors.lastName = "Last Name is required";
    if (!formValues.nicNumber) validationErrors.nicNumber = "NIC Number is required";
    if (!formValues.dateOfBirth) validationErrors.dateOfBirth = "Date of Birth is required";
    if (!formValues.employmentStartDate)
      validationErrors.employmentStartDate = "Employment Start Date is required";
    if (!formValues.baseSalary || Number(formValues.baseSalary) <= 0)
      validationErrors.baseSalary = "Base Salary must be greater than 0";

    return validationErrors;
  };

  const handleChange = (field: keyof FormState, value: string | number | boolean) => {
    setValues((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitError(null);
    const validationErrors = validate(values);
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) return;

    setSubmitting(true);
    try {
      await onSubmit(values);
    } catch (error) {
      setSubmitError((error as Error).message || "An error occurred");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form className="form" onSubmit={handleSubmit}>
      <div className="form-grid">
        <label>
          Employee Code*
          <input
            type="text"
            value={values.employeeCode || ""}
            onChange={(e) => handleChange("employeeCode", e.target.value)}
          />
          {errors.employeeCode && <span className="error">{errors.employeeCode}</span>}
        </label>

        <label>
          First Name*
          <input
            type="text"
            value={values.firstName || ""}
            onChange={(e) => handleChange("firstName", e.target.value)}
          />
          {errors.firstName && <span className="error">{errors.firstName}</span>}
        </label>

        <label>
          Last Name*
          <input
            type="text"
            value={values.lastName || ""}
            onChange={(e) => handleChange("lastName", e.target.value)}
          />
          {errors.lastName && <span className="error">{errors.lastName}</span>}
        </label>

        <label>
          NIC Number*
          <input
            type="text"
            value={values.nicNumber || ""}
            onChange={(e) => handleChange("nicNumber", e.target.value)}
          />
          {errors.nicNumber && <span className="error">{errors.nicNumber}</span>}
        </label>

        <label>
          Date of Birth*
          <input
            type="date"
            value={displayDate.dateOfBirth}
            onChange={(e) => handleChange("dateOfBirth", e.target.value)}
          />
          {errors.dateOfBirth && <span className="error">{errors.dateOfBirth}</span>}
        </label>

        <label>
          Gender
          <select
            value={values.gender}
            onChange={(e) => handleChange("gender", e.target.value as Employee["gender"])}
          >
            <option value="Male">Male</option>
            <option value="Female">Female</option>
            <option value="Other">Other</option>
          </select>
        </label>

        <label>
          Marital Status
          <select
            value={values.maritalStatus}
            onChange={(e) => handleChange("maritalStatus", e.target.value as Employee["maritalStatus"])}
          >
            <option value="Single">Single</option>
            <option value="Married">Married</option>
            <option value="Other">Other</option>
          </select>
        </label>

        <label>
          Employment Start Date*
          <input
            type="date"
            value={displayDate.employmentStartDate}
            onChange={(e) => handleChange("employmentStartDate", e.target.value)}
          />
          {errors.employmentStartDate && <span className="error">{errors.employmentStartDate}</span>}
        </label>

        <label>
          Probation End Date
          <input
            type="date"
            value={displayDate.probationEndDate}
            onChange={(e) => handleChange("probationEndDate", e.target.value)}
          />
        </label>

        <label>
          Confirmation Date
          <input
            type="date"
            value={displayDate.confirmationDate}
            onChange={(e) => handleChange("confirmationDate", e.target.value)}
          />
        </label>

        <label>
          Base Salary*
          <input
            type="number"
            value={values.baseSalary ?? ""}
            onChange={(e) => handleChange("baseSalary", Number(e.target.value))}
          />
          {errors.baseSalary && <span className="error">{errors.baseSalary}</span>}
        </label>

        <label className="checkbox">
          <input
            type="checkbox"
            checked={values.isActive ?? false}
            onChange={(e) => handleChange("isActive", e.target.checked)}
          />
          Is Active
        </label>
      </div>

      {submitError && <div className="error">{submitError}</div>}

      <div className="form-actions">
        <button type="submit" disabled={submitting}>
          {submitting ? "Saving..." : mode === "create" ? "Create" : "Update"}
        </button>
      </div>
    </form>
  );
};

export default EmployeeForm;
