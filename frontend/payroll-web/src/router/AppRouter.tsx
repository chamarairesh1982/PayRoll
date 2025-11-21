import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import LoginPage from "../pages/auth/LoginPage";
import DashboardPage from "../pages/dashboard/DashboardPage";
import EmployeeListPage from "../pages/employees/EmployeeListPage";
import EmployeeCreatePage from "../pages/employees/EmployeeCreatePage";
import EmployeeDetailPage from "../pages/employees/EmployeeDetailPage";
import EmployeeEditPage from "../pages/employees/EmployeeEditPage";
import PayRunsListPage from "../pages/payroll/PayRunsListPage";
import PayRunDetailPage from "../pages/payroll/PayRunDetailPage";

const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<AppLayout />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/employees" element={<EmployeeListPage />} />
          <Route path="/employees/new" element={<EmployeeCreatePage />} />
          <Route path="/employees/:id" element={<EmployeeDetailPage />} />
          <Route path="/employees/:id/edit" element={<EmployeeEditPage />} />
          <Route path="/payruns" element={<PayRunsListPage />} />
          <Route path="/payruns/:id" element={<PayRunDetailPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;
