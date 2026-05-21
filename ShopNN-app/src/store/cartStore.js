import { create } from "zustand";
import cartApi from "../api/cartApi";

const useCartStore = create((set, get) => ({
  cart: null,
  loading: false,

  // Lấy giỏ hàng từ API
  fetchCart: async () => {
    try {
      set({ loading: true });
      const res = await cartApi.getCart();
      set({ cart: res.data.data });
    } catch {
      set({ cart: null });
    } finally {
      set({ loading: false });
    }
  },

  // Thêm sản phẩm vào giỏ
  addItem: async (productId, quantity = 1) => {
    await cartApi.addItem({ productId, quantity });
    await get().fetchCart();
  },

  // Cập nhật số lượng
  updateItem: async (itemId, quantity) => {
    await cartApi.updateItem(itemId, { quantity });
    await get().fetchCart();
  },

  // Xóa 1 item
  removeItem: async (itemId) => {
    await cartApi.removeItem(itemId);
    await get().fetchCart();
  },

  // Xóa toàn bộ
  clearCart: async () => {
    await cartApi.clearCart();
    set({ cart: null });
  },

  cartCount: () => {
    const cart = get().cart;
    if (!cart?.items) return 0;
    return cart.items.reduce((sum, item) => sum + item.quantity, 0);
  },
}));

export default useCartStore;