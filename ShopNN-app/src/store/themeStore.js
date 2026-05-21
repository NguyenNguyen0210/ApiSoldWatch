import { create } from "zustand";

const THEME_VARIABLES = {
  dark: {
    "--primary-color": "#c9a84c",
    "--primary-hover": "#e5c158",
    "--bg-layout": "#121212",
    "--bg-card": "#1c1c1c",
    "--text-color": "#ffffff",
    "--text-muted": "rgba(255, 255, 255, 0.65)",
    "--header-bg": "#0a0a0a",
    "--border-color": "rgba(201, 168, 76, 0.15)",
  },
  light: {
    "--primary-color": "#b3923b",
    "--primary-hover": "#c9a84c",
    "--bg-layout": "#fdfcf7",
    "--bg-card": "#ffffff",
    "--text-color": "#1a1a1a",
    "--text-muted": "rgba(0, 0, 0, 0.65)",
    "--header-bg": "#ffffff",
    "--border-color": "rgba(179, 146, 59, 0.15)",
  },
};

const applyThemeVariables = (themeName) => {
  const variables = THEME_VARIABLES[themeName] || THEME_VARIABLES.dark;
  const root = document.documentElement;
  Object.entries(variables).forEach(([property, value]) => {
    root.style.setProperty(property, value);
  });
};

const useThemeStore = create((set, get) => ({
  activeTheme: ["light", "dark"].includes(localStorage.getItem("appTheme")) ? localStorage.getItem("appTheme") : "dark",

  initTheme: () => {
    const currentTheme = get().activeTheme;
    applyThemeVariables(currentTheme);
  },

  setTheme: (themeName) => {
    localStorage.setItem("appTheme", themeName);
    applyThemeVariables(themeName);
    set({ activeTheme: themeName });
  },
}));

export default useThemeStore;
export { THEME_VARIABLES };
