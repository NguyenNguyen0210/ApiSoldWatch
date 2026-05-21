import { useState, useEffect, useCallback } from "react";
import {
  Row, Col, Card, Input, Select, Slider, Button, Pagination,
  Spin, Empty, Typography, Badge, message, Breadcrumb,
} from "antd";
import {
  SearchOutlined, FilterOutlined, ShoppingCartOutlined,
  EyeOutlined, ReloadOutlined,
} from "@ant-design/icons";
import { useNavigate, useSearchParams } from "react-router-dom";
import productApi from "../../api/productApi";
import categoryApi from "../../api/categoryApi";
import useCartStore from "../../store/cartStore";
import useAuthStore from "../../store/authStore";
import "./Product.css";

const { Title, Text } = Typography;
const { Option } = Select;

export default function ProductListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { addItem } = useCartStore();
  const { accessToken } = useAuthStore();

  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [addingId, setAddingId] = useState(null);
  const [pagination, setPagination] = useState({ page: 1, pageSize: 12, totalCount: 0 });

  // Filter state
  const [filters, setFilters] = useState({
    Search: searchParams.get("Search") || "",
    CategoryId: searchParams.get("CategoryId") || undefined,
    MinPrice: searchParams.get("MinPrice") || undefined,
    MaxPrice: searchParams.get("MaxPrice") || undefined,
    InStock: searchParams.get("InStock") || undefined,
    SortBy: searchParams.get("SortBy") || "name",
    SortOrder: searchParams.get("SortOrder") || "asc",
  });
  const [priceRange, setPriceRange] = useState([0, 500000000]);

  // Load categories
  useEffect(() => {
    categoryApi.getAll().then((res) => setCategories(res.data.data || []));
  }, []);

  // Load products
  const fetchProducts = useCallback(async (page = 1) => {
    setLoading(true);
    try {
      const params = {
        ...filters,
        MinPrice: priceRange[0] > 0 ? priceRange[0] : undefined,
        MaxPrice: priceRange[1] < 500000000 ? priceRange[1] : undefined,
        Page: page,
        PageSize: pagination.pageSize,
      };
      // Xóa các param undefined
      Object.keys(params).forEach((k) => params[k] === undefined && delete params[k]);

      const res = await productApi.search(params);
      const data = res.data.data; // PagedResult<ProductResponseDTO>
      setProducts(data.items || []);
      setPagination((prev) => ({ ...prev, page, totalCount: data.totalCount }));
    } catch {
      message.error("Không thể tải danh sách sản phẩm!");
    } finally {
      setLoading(false);
    }
  }, [filters, priceRange]);

  useEffect(() => {
    fetchProducts(1);
  }, [filters]);

  const handleAddToCart = async (product) => {
    if (!accessToken) {
      message.warning("Vui lòng đăng nhập để thêm vào giỏ hàng!");
      navigate("/login");
      return;
    }
    try {
      setAddingId(product.id);
      await addItem(product.id, 1);
      message.success(`Đã thêm "${product.name}" vào giỏ hàng!`);
    } catch (err) {
      message.error(err.response?.data?.message || "Thêm vào giỏ thất bại!");
    } finally {
      setAddingId(null);
    }
  };

  const handleReset = () => {
    setFilters({ Search: "", CategoryId: undefined, InStock: undefined, SortBy: "name", SortOrder: "asc" });
    setPriceRange([0, 500000000]);
  };

  const formatPrice = (price) =>
    new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

  return (
    <div className="product-list-page">
      <Breadcrumb
        items={[{ title: "Trang chủ", href: "/" }, { title: "Sản phẩm" }]}
      />

      <Title level={2} className="product-list-title">
        🕐 Bộ sưu tập đồng hồ
      </Title>

      <Row gutter={[24, 24]}>
        {/* ===== SIDEBAR FILTER ===== */}
        <Col xs={24} lg={6}>
          <Card
            title={<><FilterOutlined /> Bộ lọc</>}
            extra={<Button size="small" icon={<ReloadOutlined />} onClick={handleReset}>Reset</Button>}
            className="filter-card"
          >
            {/* Tìm kiếm */}
            <div className="filter-group">
              <Text strong className="filter-label">Tìm kiếm</Text>
              <Input
                prefix={<SearchOutlined />}
                placeholder="Tên đồng hồ..."
                value={filters.Search}
                onChange={(e) => setFilters((f) => ({ ...f, Search: e.target.value }))}
                allowClear
              />
            </div>

            {/* Danh mục */}
            <div className="filter-group">
              <Text strong className="filter-label">Danh mục</Text>
              <Select
                placeholder="Tất cả danh mục"
                value={filters.CategoryId}
                onChange={(v) => setFilters((f) => ({ ...f, CategoryId: v }))}
                style={{ width: "100%" }}
                allowClear
              >
                {categories.map((c) => (
                  <Option key={c.id} value={c.id}>{c.name}</Option>
                ))}
              </Select>
            </div>

            {/* Khoảng giá */}
            <div className="filter-group">
              <Text strong className="filter-label">Khoảng giá</Text>
              <Slider
                range
                min={0}
                max={30000000}
                step={1000000}
                value={priceRange}
                onChange={setPriceRange}
                onChangeComplete={() => fetchProducts(1)}
                tooltip={{ formatter: (v) => formatPrice(v) }}
              />
              <div className="price-range-display">
                <Text type="secondary" className="price-range-text">{formatPrice(priceRange[0])}</Text>
                <Text type="secondary" className="price-range-text">{formatPrice(priceRange[1])}</Text>
              </div>
            </div>

            {/* Tình trạng */}
            <div className="filter-group">
              <Text strong className="filter-label">Tình trạng</Text>
              <Select
                placeholder="Tất cả"
                value={filters.InStock}
                onChange={(v) => setFilters((f) => ({ ...f, InStock: v }))}
                style={{ width: "100%" }}
                allowClear
              >
                <Option value="true">Còn hàng</Option>
                <Option value="false">Hết hàng</Option>
              </Select>
            </div>

            {/* Sắp xếp */}
            <div className="filter-group">
              <Text strong className="filter-label">Sắp xếp theo</Text>
              <Select
                value={filters.SortBy}
                onChange={(v) => setFilters((f) => ({ ...f, SortBy: v }))}
                style={{ width: "100%", marginBottom: 8 }}
              >
                <Option value="name">Tên</Option>
                <Option value="price">Giá</Option>
                <Option value="stock">Tồn kho</Option>
              </Select>
              <Select
                value={filters.SortOrder}
                onChange={(v) => setFilters((f) => ({ ...f, SortOrder: v }))}
                style={{ width: "100%" }}
              >
                <Option value="asc">Tăng dần</Option>
                <Option value="desc">Giảm dần</Option>
              </Select>
            </div>
          </Card>
        </Col>

        {/* ===== PRODUCT GRID ===== */}
        <Col xs={24} lg={18}>
          <Spin spinning={loading}>
            {products.length === 0 && !loading ? (
              <Empty description="Không tìm thấy sản phẩm nào" className="product-grid-empty" />
            ) : (
              <>
                <div className="product-grid">
                  <Row gutter={[16, 16]}>
                    {products.map((product) => (
                      <Col key={product.id} xs={12} sm={12} md={8} xl={6}>
                        <Badge.Ribbon
                          text={product.stock === 0 ? "Hết hàng" : product.categoryName || ""}
                          color={product.stock === 0 ? "red" : "#c9a84c"}
                        >
                          <Card
                            hoverable
                            cover={
                              <div className="product-grid-cover">
                                {product.imageUrl ? (
                                  <img
                                    src={product.imageUrl}
                                    alt={product.name}
                                    onError={(e) => { e.target.src = "https://placehold.co/300x200?text=No+Image"; }}
                                  />
                                ) : (
                                  <Text type="secondary">Chưa có ảnh</Text>
                                )}
                              </div>
                            }
                            actions={[
                              <Button
                                key="view"
                                type="text"
                                icon={<EyeOutlined />}
                                onClick={() => navigate(`/products/${product.id}`)}
                              >
                                Xem
                              </Button>,
                              <Button
                                key="cart"
                                type="text"
                                icon={<ShoppingCartOutlined />}
                                loading={addingId === product.id}
                                disabled={product.stock === 0}
                                onClick={() => handleAddToCart(product)}
                                className={product.stock > 0 ? "product-grid-cart-btn" : ""}
                              >
                                Thêm
                              </Button>,
                            ]}
                          >
                            <Card.Meta
                              title={
                                <Text strong ellipsis className="product-grid-name">
                                  {product.name}
                                </Text>
                              }
                              description={
                                <div>
                                  <Text className="product-grid-price">
                                    {formatPrice(product.price)}
                                  </Text>
                                  <br />
                                  <Text type="secondary" className="product-grid-stock">
                                    Còn: {product.stock} chiếc
                                  </Text>
                                </div>
                              }
                            />
                          </Card>
                        </Badge.Ribbon>
                      </Col>
                    ))}
                  </Row>
                </div>

                {/* Phân trang */}
                <div className="product-grid-pagination">
                  <Pagination
                    current={pagination.page}
                    pageSize={pagination.pageSize}
                    total={pagination.totalCount}
                    onChange={(page) => fetchProducts(page)}
                    showSizeChanger={false}
                    showTotal={(total) => `Tổng ${total} sản phẩm`}
                  />
                </div>
              </>
            )}
          </Spin>
        </Col>
      </Row>
    </div>
  );
}