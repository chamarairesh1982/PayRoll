import { Link } from "react-router-dom";

const Sidebar = () => (
  <aside className="sidebar">
    <h2>Payroll</h2>
    <nav>
      <ul>
        <li><Link to="/">Dashboard</Link></li>
        <li><Link to="/employees">Employees</Link></li>
        <li><Link to="/payruns">Pay runs</Link></li>
      </ul>
    </nav>
  </aside>
);

export default Sidebar;
