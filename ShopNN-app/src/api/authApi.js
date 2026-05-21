import axiosClient from "./axiosClient";

const authApi = {
  // POST /api/Account/signup
  // Body: { username, password, email }
  // Response: { success, message, data: null }
  register: (data) => axiosClient.post("/Account/signup", data),

  // POST /api/Account/signin
  // Body: { username, password }
  // Response: { success, message, data: { accessToken, refreshToken } }
  login: (data) => axiosClient.post("/Account/signin", data),

  // POST /api/Account/refresh
  // Body: { token: refreshToken }
  // Response: { success, message, data: { accessToken, refreshToken } }
  refreshToken: (data) => axiosClient.post("/Account/refresh", data),

  // POST /api/Account/SignOut
  // Body: { token: refreshToken }
  // Response: { success, message }
  logout: (data) => axiosClient.post("/Account/SignOut", data),

  // GET /api/Account/profile
  // Response: { success, data: { id, userName, email, roles } }
  getProfile: () => axiosClient.get("/Account/profile"),
};

export default authApi;