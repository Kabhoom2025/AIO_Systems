import { CssBaseline, ThemeProvider } from "@mui/material";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { MsalProvider } from "@azure/msal-react";
import { RouterProvider } from "react-router-dom";
import { useMemo } from "react";
import { msalInstance } from "./msalConfig";
import { router } from "./router";
import { buildTheme } from "./theme";
import { useThemeStore } from "../store/themeStore";
import { ChatSocketProvider } from "../providers/ChatSocketProvider";
import { NotificationSocketProvider } from "../providers/NotificationSocketProvider";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

export function App() {
  const mode = useThemeStore((s) => s.mode);
  const theme = useMemo(() => buildTheme(mode), [mode]);

  return (
    <MsalProvider instance={msalInstance}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <ChatSocketProvider>
            <NotificationSocketProvider>
              <RouterProvider router={router} />
            </NotificationSocketProvider>
          </ChatSocketProvider>
        </ThemeProvider>
        {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
      </QueryClientProvider>
    </MsalProvider>
  );
}
