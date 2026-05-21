import axiosClient from "./axiosClient";

const categoryApi = {
  // GET /api/category
  getAll: () => axiosClient.get("/category"),

  // GET /api/category/:id
  getById: (id) => axiosClient.get(`/category/${id}`),

  // POST /api/category (Admin)
  create: (data) => axiosClient.post("/category", data),

  // PUT /api/category/:id (Admin)
  update: (id, data) => axiosClient.put(`/category/${id}`, data),

  // DELETE /api/category/:id (Admin)
  delete: (id) => axiosClient.delete(`/category/${id}`),
};

export default categoryApi;