import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar";
import TopBar from "./TopBar";
import "./layout.css";

const AppLayout = () => {
  return (
    <div className="app-layout">
      <Sidebar />
      <div className="app-content">
        <TopBar />
        <main>
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default AppLayout;
