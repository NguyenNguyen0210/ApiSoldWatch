import axiosClient from "./axiosClient";

const orderApi = {
  // POST /api/Order/checkout — { paymentMethod: "COD" | "VnPay" }
  checkout: (data) => axiosClient.post("/Order/checkout", data),

  // GET /api/Order/my-orders
  getMyOrders: () => axiosClient.get("/Order/my-orders"),

  // GET /api/Order/admin/all (Admin only)
  getAllOrders: () => axiosClient.get("/Order/admin/all"),

  // GET /api/Order/admin/search?... (Admin only, paged)
  searchOrders: (params) => axiosClient.get("/Order/admin/search", { params }),

  // PUT /api/Order/admin/:id/status — statusNum as raw number body (0=Pending, 1=Processing, 2=Shipped, 3=Delivered, 4=Cancelled)
  updateStatus: (id, statusNum) => axiosClient.put(`/Order/admin/${id}/status`, statusNum),

  // PUT /api/Order/admin/:id/payment-status — statusNum as raw number body (0=Unpaid, 1=Paid, 2=Failed, 3=Refunded)
  updatePaymentStatus: (id, statusNum) => axiosClient.put(`/Order/admin/${id}/payment-status`, statusNum),
};

export default orderApi;
