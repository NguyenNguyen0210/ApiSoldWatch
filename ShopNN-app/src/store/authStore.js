import { create } from "zustand";
import authApi from "../api/authApi";

const useAuthStore = create((set, get) => ({
  user: JSON.parse(localStorage.getItem("user")) || null,
  accessToken: localStorage.getItem("accessToken") || null,
  refreshToken: localStorage.getItem("refreshToken") || null,

  // Lưu thông tin sau khi login
  setAuth: (accessToken, refreshToken) => {
    localStorage.setItem("accessToken", accessToken);
    localStorage.setItem("refreshToken", refreshToken);
    set({ accessToken, refreshToken });
  },

  // Lưu profile user (gọi sau setAuth)
  setUser: (user) => {
    localStorage.setItem("user", JSON.stringify(user));
    set({ user });
  },

  // Đăng xuất
  logout: async () => {
    const refreshToken = get().refreshToken;
    try {
      if (refreshToken) await authApi.logout({ token: refreshToken });
    } catch {}
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
    set({ user: null, accessToken: null, refreshToken: null });
  },

  isAuthenticated: () => !!get().accessToken,
  isAdmin: () => get().user?.roles?.includes("Admin") || false,
}));

export default useAuthStore;