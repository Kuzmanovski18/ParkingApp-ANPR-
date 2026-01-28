import { createContext, useContext, useEffect, useState } from "react";

type Role = "User" | "Admin";

type AuthState = {
  token: string | null;
  username: string | null;
  role: Role | null;
  login: (token: string, username: string, role: Role) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem("token"));
  const [username, setUsername] = useState<string | null>(localStorage.getItem("username"));
  const [role, setRole] = useState<Role | null>(localStorage.getItem("role") as Role | null);

  function login(t: string, u: string, r: Role) {
    setToken(t);
    setUsername(u);
    setRole(r);
    localStorage.setItem("token", t);
    localStorage.setItem("username", u);
    localStorage.setItem("role", r);
  }

  function logout() {
    setToken(null);
    setUsername(null);
    setRole(null);
    localStorage.clear();
  }

  return (
    <AuthContext.Provider value={{ token, username, role, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}
