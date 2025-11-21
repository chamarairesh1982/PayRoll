import { useAuthContext } from "../context/AuthContext";

const useAuth = () => {
  const auth = useAuthContext();
  return auth;
};

export default useAuth;
