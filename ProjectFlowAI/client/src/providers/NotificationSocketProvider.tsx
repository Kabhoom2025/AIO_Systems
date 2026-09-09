import * as signalR from "@microsoft/signalr";
import { Alert, Snackbar } from "@mui/material";
import { useEffect, useRef, useState, type ReactNode } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { router } from "../app/router";
import { NOTIFICATIONS_HUB_URL } from "../api/hubUrls";
import { useAuthStore } from "../store/authStore";
import type { AppNotification } from "../types";

/**
 * Owns the NotificationsHub SignalR connection. On "NotificationReceived" it invalidates the
 * unread-count/list queries so the bell updates, and shows a transient toast (this app has no
 * pre-existing global toast system, so a simple Snackbar queue lives here).
 */
export function NotificationSocketProvider({ children }: { children: ReactNode }) {
  const accessToken = useAuthStore((s) => s.accessToken);
  const queryClient = useQueryClient();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [toast, setToast] = useState<AppNotification | null>(null);

  useEffect(() => {
    if (!accessToken) {
      connectionRef.current?.stop();
      connectionRef.current = null;
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(NOTIFICATIONS_HUB_URL, {
        accessTokenFactory: () => useAuthStore.getState().accessToken ?? "",
      })
      .withAutomaticReconnect()
      .build();

    connection.on("NotificationReceived", (notification: AppNotification) => {
      queryClient.invalidateQueries({ queryKey: ["notifications", "unread-count"] });
      queryClient.invalidateQueries({ queryKey: ["notifications", "list"] });
      setToast(notification);
    });

    connection.start().catch(() => undefined);
    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [accessToken, queryClient]);

  return (
    <>
      {children}
      <Snackbar
        open={!!toast}
        autoHideDuration={5000}
        onClose={() => setToast(null)}
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
      >
        <Alert
          severity="info"
          onClose={() => setToast(null)}
          sx={{ cursor: toast?.linkUrl ? "pointer" : undefined, maxWidth: 360 }}
          onClick={() => {
            if (toast?.linkUrl) {
              router.navigate(toast.linkUrl);
              setToast(null);
            }
          }}
        >
          <strong>{toast?.title}</strong>
          <div style={{ fontSize: 13 }}>{toast?.body}</div>
        </Alert>
      </Snackbar>
    </>
  );
}
