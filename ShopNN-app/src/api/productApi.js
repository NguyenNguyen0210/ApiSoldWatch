import axiosClient from "./axiosClient";

const productApi = {
  // GET /api/products
  getAll: () => axiosClient.get("/products"),

  // GET /api/products/search?Search=&CategoryId=&MinPrice=&MaxPrice=&InStock=&SortBy=&SortOrder=&Page=&PageSize=
  search: (params) => axiosClient.get("/products/search", { params }),

  // GET /api/products/:id
  getById: (id) => axiosClient.get(`/products/${id}`),

  // POST /api/products (Admin)
  create: (data) => axiosClient.post("/products", data),

  // PUT /api/products/:id (Admin)
  update: (id, data) => axiosClient.put(`/products/${id}`, data),

  // DELETE /api/products/:id (Admin)
  delete: (id) => axiosClient.delete(`/products/${id}`),
};

export default productApi;