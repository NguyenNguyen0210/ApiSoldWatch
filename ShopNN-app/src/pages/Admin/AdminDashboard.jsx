import { useEffect, useState } from "react";
import {
  Typography, Row, Col, Card, Table, Button, Input, Select,
  InputNumber, Modal, Form, Tag, Popconfirm, Tabs, message, Spin, Tooltip
} from "antd";
import {
  PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined,
  ShoppingOutlined, TagsOutlined, DatabaseOutlined, DollarOutlined,
  LineChartOutlined, EyeOutlined
} from "@ant-design/icons";
import productApi from "../../api/productApi";
import categoryApi from "../../api/categoryApi";
import orderApi from "../../api/orderApi";
import "./Admin.css";

const { Title, Text } = Typography;
const { TabPane } = Tabs;

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

const formatDate = (dateStr) => {
  if (!dateStr) return "N/A";
  const date = new Date(dateStr);
  return date.toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

const ORDER_STATUS_COLOR = {
  Pending: "orange",
  Processing: "blue",
  Shipped: "cyan",
  Delivered: "green",
  Cancelled: "red",
};

const ORDER_STATUS_LABEL = {
  Pending: "Chờ xử lý",
  Processing: "Đang xử lý",
  Shipped: "Đang giao",
  Delivered: "Đã giao",
  Cancelled: "Đã hủy",
};

const ORDER_STATUS_ENUM = {
  Pending: 0,
  Processing: 1,
  Shipped: 2,
  Delivered: 3,
  Cancelled: 4,
};

const PAYMENT_STATUS_COLOR = {
  Unpaid: "default",
  Paid: "success",
  Failed: "error",
  Refunded: "warning",
};

const PAYMENT_STATUS_ENUM = {
  Unpaid: 0,
  Paid: 1,
  Failed: 2,
  Refunded: 3,
};

const PAYMENT_STATUS_LABEL = {
  Unpaid: "Chưa thanh toán",
  Paid: "Đã thanh toán",
  Failed: "Thất bại",
  Refunded: "Hoàn tiền",
};

export default function AdminDashboard() {
  const [activeTab, setActiveTab] = useState("products");

  // Statistical & list data
  const [stats, setStats] = useState({
    totalProducts: 0,
    totalCategories: 0,
    totalOrders: 0,
    totalRevenue: 0,
  });

  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [orders, setOrders] = useState([]);

  // Loading states
  const [loading, setLoading] = useState(false);

  // Search & filter states
  const [prodSearch, setProdSearch] = useState("");
  const [orderSearch, setOrderSearch] = useState("");
  const [catSearch, setCatSearch] = useState("");

  // Product Modals
  const [prodModalVisible, setProdModalVisible] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [prodForm] = Form.useForm();

  // Category Modals
  const [catModalVisible, setCatModalVisible] = useState(false);
  const [editingCategory, setEditingCategory] = useState(null);
  const [catForm] = Form.useForm();

  // Load all lists
  const fetchAllData = async () => {
    setLoading(true);
    try {
      const [prodRes, catRes, orderRes] = await Promise.all([
        productApi.getAll(),
        categoryApi.getAll(),
        orderApi.getAllOrders(),
      ]);

      const prods = prodRes.data.data || [];
      const cats = catRes.data.data || [];
      const ords = orderRes.data.data || [];

      setProducts(prods);
      setCategories(cats);
      setOrders(ords);

      // Compute statistics
      const revenue = ords
        .filter(o => o.paymentStatus === "Paid" || o.status === "Delivered")
        .reduce((sum, o) => sum + o.totalAmount, 0);

      setStats({
        totalProducts: prods.length,
        totalCategories: cats.length,
        totalOrders: ords.length,
        totalRevenue: revenue,
      });
    } catch (err) {
      message.error("Lỗi khi tải dữ liệu trang quản trị!");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAllData();
  }, []);

  // Category Actions
  const handleOpenCatModal = (cat = null) => {
    setEditingCategory(cat);
    if (cat) {
      catForm.setFieldsValue({ name: cat.name, imageUrl: cat.imageUrl });
    } else {
      catForm.resetFields();
    }
    setCatModalVisible(true);
  };

  const handleSaveCategory = async (values) => {
    try {
      if (editingCategory) {
        await categoryApi.update(editingCategory.id, values);
        message.success("Cập nhật danh mục thành công!");
      } else {
        await categoryApi.create(values);
        message.success("Thêm danh mục mới thành công!");
      }
      setCatModalVisible(false);
      fetchAllData();
    } catch (err) {
      message.error(err.response?.data?.message || "Lỗi thao tác danh mục!");
    }
  };

  const handleDeleteCategory = async (id) => {
    try {
      await categoryApi.delete(id);
      message.success("Xóa danh mục thành công!");
      fetchAllData();
    } catch (err) {
      message.error(err.response?.data?.message || "Lỗi xóa danh mục!");
    }
  };

  const handleOpenProdModal = (prod = null) => {
    setEditingProduct(prod);
    if (prod) {
      prodForm.setFieldsValue({
        name: prod.name,
        description: prod.description,
        price: prod.price,
        stock: prod.stock,
        categoryId: prod.categoryId,
        imageUrl: prod.imageUrl,
      });
    } else {
      prodForm.resetFields();
    }
    setProdModalVisible(true);
  };

  const handleSaveProduct = async (values) => {
    try {
      if (editingProduct) {
        await productApi.update(editingProduct.id, values);
        message.success("Cập nhật sản phẩm thành công!");
      } else {
        await productApi.create(values);
        message.success("Thêm sản phẩm mới thành công!");
      }
      setProdModalVisible(false);
      fetchAllData();
    } catch (err) {
      message.error(err.response?.data?.message || "Lỗi thao tác sản phẩm!");
    }
  };

  const handleDeleteProduct = async (id) => {
    try {
      await productApi.delete(id);
      message.success("Xóa sản phẩm thành công!");
      fetchAllData();
    } catch (err) {
      message.error(err.response?.data?.message || "Lỗi xóa sản phẩm!");
    }
  };

  // Order Actions
  const handleUpdateOrderStatus = async (id, status) => {
    try {
      const statusNum = ORDER_STATUS_ENUM[status] ?? 0;
      await orderApi.updateStatus(id, statusNum);
      message.success("Cập nhật trạng thái đơn hàng thành công!");
      fetchAllData();
    } catch (err) {
      message.error(err.response?.data?.message || "Lỗi cập nhật trạng thái!");
    }
  };

  const handleUpdatePaymentStatus = async (id, paymentStatus) => {
    try {
      const statusNum = PAYMENT_STATUS_ENUM[paymentStatus] ?? 0;
      await orderApi.updatePaymentStatus(id, statusNum);
      message.success("Cập nhật trạng thái thanh toán thành công!");
      fetchAllData();
    } catch (err) {
      message.error(err.response?.data?.message || "Lỗi cập nhật trạng thái thanh toán!");
    }
  };

  // Table Columns Setup
  const productColumns = [
    {
      title: "Ảnh",
      dataIndex: "imageUrl",
      key: "imageUrl",
      width: 80,
      render: (img) => <img src={img || "https://placehold.co/100"} alt="product" className="admin-product-thumb" />
    },
    {
      title: "Tên sản phẩm",
      dataIndex: "name",
      key: "name",
      sorter: (a, b) => a.name.localeCompare(b.name),
    },
    {
      title: "Danh mục",
      dataIndex: "categoryName",
      key: "categoryName",
      render: (name) => name || <Text type="secondary">Chưa phân loại</Text>,
    },
    {
      title: "Giá bán",
      dataIndex: "price",
      key: "price",
      sorter: (a, b) => a.price - b.price,
      render: (price) => <span style={{ fontWeight: 600 }}>{formatPrice(price)}</span>,
    },
    {
      title: "Tồn kho",
      dataIndex: "stock",
      key: "stock",
      sorter: (a, b) => a.stock - b.stock,
      render: (stock) => {
        if (stock === 0) return <Tag color="error">Hết hàng</Tag>;
        if (stock < 5) return <Tag color="warning">Sắp hết ({stock})</Tag>;
        return <Tag color="success">{stock} chiếc</Tag>;
      }
    },
    {
      title: "Hành động",
      key: "actions",
      width: 120,
      render: (_, record) => (
        <div className="admin-action-btn-group">
          <Tooltip title="Chỉnh sửa">
            <Button
              type="text"
              icon={<EditOutlined />}
              className="admin-edit-action"
              onClick={() => handleOpenProdModal(record)}
            />
          </Tooltip>
          <Tooltip title="Xóa">
            <Popconfirm
              title="Bạn chắc chắn muốn xóa sản phẩm này?"
              onConfirm={() => handleDeleteProduct(record.id)}
              okText="Có"
              cancelText="Không"
              okButtonProps={{ danger: true }}
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                className="admin-delete-action"
              />
            </Popconfirm>
          </Tooltip>
        </div>
      )
    }
  ];

  const categoryColumns = [
    {
      title: "ID",
      dataIndex: "id",
      key: "id",
      width: 80,
    },
    {
      title: "Ảnh bìa",
      dataIndex: "imageUrl",
      key: "imageUrl",
      width: 100,
      render: (img) => (
        <img
          src={img || "https://placehold.co/100?text=Category"}
          alt="category"
          className="admin-product-thumb"
          onError={(e) => { e.target.src = "https://placehold.co/100?text=No+Img"; }}
        />
      )
    },
    {
      title: "Tên danh mục",
      dataIndex: "name",
      key: "name",
      sorter: (a, b) => a.name.localeCompare(b.name),
    },
    {
      title: "Hành động",
      key: "actions",
      width: 120,
      render: (_, record) => (
        <div className="admin-action-btn-group">
          <Tooltip title="Chỉnh sửa">
            <Button
              type="text"
              icon={<EditOutlined />}
              className="admin-edit-action"
              onClick={() => handleOpenCatModal(record)}
            />
          </Tooltip>
          <Tooltip title="Xóa">
            <Popconfirm
              title="Xóa danh mục sẽ không ảnh hưởng sản phẩm cũ. Tiếp tục?"
              onConfirm={() => handleDeleteCategory(record.id)}
              okText="Có"
              cancelText="Không"
              okButtonProps={{ danger: true }}
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                className="admin-delete-action"
              />
            </Popconfirm>
          </Tooltip>
        </div>
      )
    }
  ];

  const orderColumns = [
    {
      title: "Đơn hàng",
      dataIndex: "id",
      key: "id",
      render: (id) => <span style={{ fontFamily: "Jost", fontWeight: 600 }}>#{id.substring(0, 8).toUpperCase()}</span>,
    },
    {
      title: "Ngày đặt",
      dataIndex: "createdAt",
      key: "createdAt",
      render: (date) => formatDate(date),
    },
    {
      title: "Sản phẩm đặt mua",
      dataIndex: "items",
      key: "items",
      render: (items) => (
        <div style={{ maxHeight: 70, overflowY: "auto" }}>
          {items?.map((item, idx) => (
            <div key={idx} style={{ fontSize: 13 }}>
              • {item.productName || `SP #${item.productId}`} <Text type="secondary">x{item.quantity}</Text>
            </div>
          ))}
        </div>
      )
    },
    {
      title: "Tổng tiền",
      dataIndex: "totalAmount",
      key: "totalAmount",
      sorter: (a, b) => a.totalAmount - b.totalAmount,
      render: (amt) => <span style={{ fontWeight: 600, color: "#c9a84c" }}>{formatPrice(amt)}</span>,
    },
    {
      title: "Phương thức",
      dataIndex: "paymentMethod",
      key: "paymentMethod",
    },
    {
      title: "Thanh toán",
      dataIndex: "paymentStatus",
      key: "paymentStatus",
      render: (status, record) => (
        <Select
          value={status}
          onChange={(val) => handleUpdatePaymentStatus(record.id, val)}
          className="payment-status-select"
          size="small"
          style={{ minWidth: 130 }}
        >
          {Object.keys(PAYMENT_STATUS_LABEL).map(statusKey => (
            <Select.Option key={statusKey} value={statusKey}>
              <Tag color={PAYMENT_STATUS_COLOR[statusKey]} style={{ border: "none", margin: 0 }}>
                {PAYMENT_STATUS_LABEL[statusKey]}
              </Tag>
            </Select.Option>
          ))}
        </Select>
      )
    },
    {
      title: "Trạng thái giao hàng",
      key: "status",
      render: (_, record) => (
        <Select
          value={record.status}
          onChange={(val) => handleUpdateOrderStatus(record.id, val)}
          className="order-status-select"
          size="small"
        >
          {Object.keys(ORDER_STATUS_LABEL).map(statusKey => (
            <Select.Option key={statusKey} value={statusKey}>
              <Tag color={ORDER_STATUS_COLOR[statusKey]} style={{ border: "none", margin: 0 }}>
                {ORDER_STATUS_LABEL[statusKey]}
              </Tag>
            </Select.Option>
          ))}
        </Select>
      )
    }
  ];

  // Filtering lists based on search keys
  const filteredProducts = products.filter(
    (p) =>
      (p.name && typeof p.name === "string" && p.name.toLowerCase().includes(prodSearch.toLowerCase())) ||
      (p.categoryName && typeof p.categoryName === "string" && p.categoryName.toLowerCase().includes(prodSearch.toLowerCase()))
  );

  const filteredOrders = orders.filter(
    (o) =>
      (o.id && String(o.id).toLowerCase().includes(orderSearch.toLowerCase())) ||
      o.items?.some((i) => i.productName && typeof i.productName === "string" && i.productName.toLowerCase().includes(orderSearch.toLowerCase()))
  );

  const filteredCategories = categories.filter(
    (c) =>
      (c.name && typeof c.name === "string" && c.name.toLowerCase().includes(catSearch.toLowerCase())) ||
      (c.id && String(c.id).toLowerCase().includes(catSearch.toLowerCase()))
  );

  return (
    <div className="admin-dashboard">
      <Title level={2} className="admin-title">🛡️ Quản trị hệ thống ShopNN</Title>

      {/* Overview Cards */}
      <Spin spinning={loading}>
        <Row gutter={[16, 16]} className="admin-stats-row">
          <Col xs={12} sm={12} md={6}>
            <div className="stat-card-luxury">
              <div className="stat-card-header">
                <span className="stat-card-title">Doanh thu</span>
                <DollarOutlined className="stat-card-icon" />
              </div>
              <div className="stat-card-value">{formatPrice(stats.totalRevenue)}</div>
            </div>
          </Col>
          <Col xs={12} sm={12} md={6}>
            <div className="stat-card-luxury">
              <div className="stat-card-header">
                <span className="stat-card-title">Đơn hàng</span>
                <ShoppingOutlined className="stat-card-icon" />
              </div>
              <div className="stat-card-value">{stats.totalOrders}</div>
            </div>
          </Col>
          <Col xs={12} sm={12} md={6}>
            <div className="stat-card-luxury">
              <div className="stat-card-header">
                <span className="stat-card-title">Sản phẩm</span>
                <DatabaseOutlined className="stat-card-icon" />
              </div>
              <div className="stat-card-value">{stats.totalProducts}</div>
            </div>
          </Col>
          <Col xs={12} sm={12} md={6}>
            <div className="stat-card-luxury">
              <div className="stat-card-header">
                <span className="stat-card-title">Danh mục</span>
                <TagsOutlined className="stat-card-icon" />
              </div>
              <div className="stat-card-value">{stats.totalCategories}</div>
            </div>
          </Col>
        </Row>
      </Spin>

      {/* Main Tab Controller */}
      <div className="admin-layout-wrapper">
        <Tabs
          activeKey={activeTab}
          onChange={(tab) => setActiveTab(tab)}
          className="admin-tabs"
        >
          {/* TAB 1: PRODUCT MANAGEMENT */}
          <TabPane
            tab={<span><DatabaseOutlined />Sản phẩm</span>}
            key="products"
          >
            <div className="admin-tab-content">
              <div className="admin-table-controls">
                <Input
                  placeholder="Tìm kiếm sản phẩm, danh mục..."
                  prefix={<SearchOutlined />}
                  value={prodSearch}
                  onChange={(e) => setProdSearch(e.target.value)}
                  className="admin-search-input"
                  allowClear
                />
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  className="admin-add-btn"
                  onClick={() => handleOpenProdModal()}
                >
                  Thêm sản phẩm
                </Button>
              </div>

              <Table
                columns={productColumns}
                dataSource={filteredProducts}
                rowKey="id"
                loading={loading}
                className="admin-table"
                pagination={{ pageSize: 8 }}
              />
            </div>
          </TabPane>

          {/* TAB 2: CATEGORY MANAGEMENT */}
          <TabPane
            tab={<span><TagsOutlined />Danh mục</span>}
            key="categories"
          >
            <div className="admin-tab-content">
              <div className="admin-table-controls">
                <Input
                  placeholder="Tìm kiếm danh mục..."
                  prefix={<SearchOutlined />}
                  value={catSearch}
                  onChange={(e) => setCatSearch(e.target.value)}
                  className="admin-search-input"
                  allowClear
                />
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  className="admin-add-btn"
                  onClick={() => handleOpenCatModal()}
                >
                  Thêm danh mục
                </Button>
              </div>

              <Table
                columns={categoryColumns}
                dataSource={filteredCategories}
                rowKey="id"
                loading={loading}
                className="admin-table"
                pagination={{ pageSize: 8 }}
              />
            </div>
          </TabPane>

          {/* TAB 3: ORDER MANAGEMENT */}
          <TabPane
            tab={<span><ShoppingOutlined />Đơn hàng</span>}
            key="orders"
          >
            <div className="admin-tab-content">
              <div className="admin-table-controls">
                <Input
                  placeholder="Tìm đơn hàng theo mã, tên SP..."
                  prefix={<SearchOutlined />}
                  value={orderSearch}
                  onChange={(e) => setOrderSearch(e.target.value)}
                  className="admin-search-input"
                  allowClear
                />
              </div>

              <Table
                columns={orderColumns}
                dataSource={filteredOrders}
                rowKey="id"
                loading={loading}
                className="admin-table"
                pagination={{ pageSize: 8 }}
              />
            </div>
          </TabPane>
        </Tabs>
      </div>
      <Modal
        title={editingProduct ? "Chỉnh sửa sản phẩm" : "Thêm sản phẩm mới"}
        open={prodModalVisible}
        onCancel={() => setProdModalVisible(false)}
        footer={null}
        destroyOnClose
        className="admin-form-modal"
      >
        <Form
          form={prodForm}
          layout="vertical"
          onFinish={handleSaveProduct}
        >
          <Form.Item
            name="name"
            label="Tên sản phẩm"
            rules={[{ required: true, message: "Vui lòng nhập tên sản phẩm!" }]}
          >
            <Input placeholder="Nhập tên sản phẩm..." />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="price"
                label="Giá bán (VND)"
                rules={[{ required: true, message: "Nhập giá bán!" }]}
              >
                <InputNumber
                  style={{ width: "100%" }}
                  min={0}
                  formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={value => value.replace(/\$\s?|(,*)/g, '')}
                  placeholder="VND"
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="stock"
                label="Số lượng tồn kho"
                rules={[{ required: true, message: "Nhập số lượng tồn!" }]}
              >
                <InputNumber style={{ width: "100%" }} min={0} placeholder="Số lượng" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="categoryId"
            label="Danh mục sản phẩm"
            rules={[{ required: true, message: "Vui lòng chọn danh mục!" }]}
          >
            <Select placeholder="Chọn danh mục">
              {categories.map((cat) => (
                <Select.Option key={cat.id} value={cat.id}>
                  {cat.name}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="imageUrl"
            label="URL Hình ảnh sản phẩm"
          >
            <Input placeholder="https://example.com/image.jpg" />
          </Form.Item>

          <Form.Item
            name="description"
            label="Mô tả sản phẩm"
          >
            <Input.TextArea rows={4} placeholder="Nhập mô tả chi tiết sản phẩm..." />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: "right" }}>
            <Button onClick={() => setProdModalVisible(false)} style={{ marginRight: 8, borderRadius: 8 }}>
              Hủy
            </Button>
            <Button type="primary" htmlType="submit" style={{ background: "#c9a84c", borderColor: "#c9a84c", color: "#000", borderRadius: 8, fontWeight: 600 }}>
              Lưu lại
            </Button>
          </Form.Item>
        </Form>
      </Modal>

      {/* ===== MODAL: ADD / EDIT CATEGORY ===== */}
      <Modal
        title={editingCategory ? "Chỉnh sửa danh mục" : "Thêm danh mục mới"}
        open={catModalVisible}
        onCancel={() => setCatModalVisible(false)}
        footer={null}
        destroyOnClose
        className="admin-form-modal"
      >
        <Form
          form={catForm}
          layout="vertical"
          onFinish={handleSaveCategory}
        >
          <Form.Item
            name="name"
            label="Tên danh mục"
            rules={[{ required: true, message: "Vui lòng nhập tên danh mục!" }]}
          >
            <Input placeholder="Nhập tên danh mục..." />
          </Form.Item>

          <Form.Item
            name="imageUrl"
            label="URL Hình ảnh danh mục"
          >
            <Input placeholder="https://example.com/category.jpg" />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: "right" }}>
            <Button onClick={() => setCatModalVisible(false)} style={{ marginRight: 8, borderRadius: 8 }}>
              Hủy
            </Button>
            <Button type="primary" htmlType="submit" style={{ background: "#c9a84c", borderColor: "#c9a84c", color: "#000", borderRadius: 8, fontWeight: 600 }}>
              Lưu lại
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
