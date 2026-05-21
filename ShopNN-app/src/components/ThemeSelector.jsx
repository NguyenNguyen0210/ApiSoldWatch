import React from "react";
import { Dropdown, Button, Tooltip } from "antd";
import { BgColorsOutlined, CheckOutlined } from "@ant-design/icons";
import useThemeStore from "../store/themeStore";

export default function ThemeSelector() {
  const { activeTheme, setTheme } = useThemeStore();

  const themeOptions = [
    {
      key: "dark",
      label: (
        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", minWidth: 150, padding: "4px 8px" }}>
          <span style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <span style={{ display: "inline-block", width: 12, height: 12, borderRadius: "50%", background: "#0a0a0a", border: "1px solid #c9a84c" }} />
            <span>Dark</span>
          </span>
          {activeTheme === "dark" && <CheckOutlined style={{ color: "#c9a84c" }} />}
        </div>
      ),
      onClick: () => setTheme("dark"),
    },
    {
      key: "light",
      label: (
        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", minWidth: 150, padding: "4px 8px" }}>
          <span style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <span style={{ display: "inline-block", width: 12, height: 12, borderRadius: "50%", background: "#fdfcf7", border: "1px solid #b3923b" }} />
            <span>Light</span>
          </span>
          {activeTheme === "light" && <CheckOutlined style={{ color: "#b3923b" }} />}
        </div>
      ),
      onClick: () => setTheme("light"),
    },
  ];

  return (
    <Dropdown menu={{ items: themeOptions }} placement="bottomRight" trigger={["click"]}>
      <Tooltip title="Tùy chỉnh giao diện">
        <Button
          type="text"
          shape="circle"
          icon={<BgColorsOutlined />}
          style={{
            fontSize: 18,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            color: "var(--primary-color)",
          }}
        />
      </Tooltip>
    </Dropdown>
  );
}
