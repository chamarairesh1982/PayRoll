import { createContext, useContext, useState } from "react";

type UiContextValue = {
  sidebarOpen: boolean;
  toggleSidebar: () => void;
};

const UiContext = createContext<UiContextValue | undefined>(undefined);

export const UiProvider = ({ children }: { children: React.ReactNode }) => {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const value: UiContextValue = {
    sidebarOpen,
    toggleSidebar: () => setSidebarOpen((open) => !open)
  };

  return <UiContext.Provider value={value}>{children}</UiContext.Provider>;
};

export const useUiContext = () => {
  const context = useContext(UiContext);
  if (!context) throw new Error("UiContext not available");
  return context;
};
