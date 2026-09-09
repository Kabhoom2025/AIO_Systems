import { createTheme, type ThemeOptions } from "@mui/material/styles";
import type { ThemeMode } from "../store/themeStore";

const baseOptions: ThemeOptions = {
  typography: {
    fontFamily: [
      "Inter",
      "-apple-system",
      "BlinkMacSystemFont",
      "Segoe UI",
      "Roboto",
      "Helvetica Neue",
      "Arial",
      "sans-serif",
    ].join(","),
    h1: { fontWeight: 700 },
    h2: { fontWeight: 700 },
    h3: { fontWeight: 600 },
    h4: { fontWeight: 600 },
    h5: { fontWeight: 600 },
    h6: { fontWeight: 600 },
    button: { fontWeight: 600, textTransform: "none" },
  },
  shape: {
    borderRadius: 10,
  },
  spacing: 8,
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          transition: "transform 120ms ease, box-shadow 120ms ease",
        },
        contained: {
          "&:hover": {
            transform: "translateY(-1px)",
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
        },
      },
    },
    MuiDialog: {
      defaultProps: {
        transitionDuration: 200,
      },
    },
    MuiTextField: {
      defaultProps: {
        size: "small",
      },
    },
  },
};

export function buildTheme(mode: ThemeMode) {
  const isLight = mode === "light";

  return createTheme({
    ...baseOptions,
    palette: {
      mode,
      primary: {
        main: isLight ? "#6355FF" : "#8B7CFF",
        light: "#8B7CFF",
        dark: "#4A3FCC",
        contrastText: "#FFFFFF",
      },
      secondary: {
        main: isLight ? "#00B8A9" : "#3DD9CA",
      },
      background: {
        default: isLight ? "#F6F7FB" : "#12131A",
        paper: isLight ? "#FFFFFF" : "#1A1B25",
      },
      text: {
        primary: isLight ? "#1B1D28" : "#EDEEF4",
        secondary: isLight ? "#5B5E70" : "#A6A9BC",
      },
      divider: isLight ? "#E4E6EF" : "#2B2D3A",
      grey: {
        50: "#F8F9FC",
        100: "#F0F1F7",
        200: "#E4E6EF",
        300: "#CBCDDB",
        400: "#A6A9BC",
        500: "#7C7F94",
        600: "#5B5E70",
        700: "#41434F",
        800: "#2B2D3A",
        900: "#1B1D28",
      },
    },
  });
}
