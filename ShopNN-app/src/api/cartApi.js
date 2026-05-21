import axiosClient from "./axiosClient";

const cartApi = {
  // GET /api/Cart
  getCart: () => axiosClient.get("/Cart"),

  // POST /api/Cart/items — { productId, quantity }
  addItem: (data) => axiosClient.post("/Cart/items", data),

  // PUT /api/Cart/items/:itemId — { quantity }
  updateItem: (itemId, data) => axiosClient.put(`/Cart/items/${itemId}`, data),

  // DELETE /api/Cart/items/:itemId
  removeItem: (itemId) => axiosClient.delete(`/Cart/items/${itemId}`),

  // DELETE /api/Cart/clear
  clearCart: () => axiosClient.delete("/Cart/clear"),
};

export default cartApi;