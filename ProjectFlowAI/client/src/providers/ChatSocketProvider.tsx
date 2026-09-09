import * as signalR from "@microsoft/signalr";
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { useQueryClient } from "@tanstack/react-query";
import { CHAT_HUB_URL } from "../api/hubUrls";
import { useAuthStore } from "../store/authStore";
import { chatKeys } from "../hooks/chatKeys";
import { prependMessageToThread, updateMessageInAllThreads } from "../hooks/chatCache";
import type { ChatMessage, ChatReaction, ChatThreadRef } from "../types";

interface TypingEntry {
  label: string;
  expiresAt: number;
}

interface ChatSocketContextValue {
  connectionState: signalR.HubConnectionState;
  onlineUserIds: Set<string>;
  /** Keyed by `${kind}:${id}` (matching ChatThreadRef), value is a display label or null if no
   * one is currently typing in that thread. */
  typingLabelFor: (thread: ChatThreadRef) => string | null;
  notifyTyping: (thread: ChatThreadRef, otherUserName?: string) => void;
}

const ChatSocketContext = createContext<ChatSocketContextValue | null>(null);

function threadKey(kind: string, id: string) {
  return `${kind}:${id}`;
}

export function ChatSocketProvider({ children }: { children: ReactNode }) {
  const accessToken = useAuthStore((s) => s.accessToken);
  const queryClient = useQueryClient();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const typingDebounceRef = useRef<Map<string, number>>(new Map());

  const [connectionState, setConnectionState] = useState<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected
  );
  const [onlineUserIds, setOnlineUserIds] = useState<Set<string>>(new Set());
  const [typingMap, setTypingMap] = useState<Map<string, TypingEntry>>(new Map());

  useEffect(() => {
    if (!accessToken) {
      connectionRef.current?.stop();
      connectionRef.current = null;
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(CHAT_HUB_URL, {
        accessTokenFactory: () => useAuthStore.getState().accessToken ?? "",
      })
      .withAutomaticReconnect()
      .build();

    connection.on("MessageReceived", (message: ChatMessage) => {
      const thread: ChatThreadRef | null = message.channelId
        ? { kind: "channel", id: message.channelId }
        : message.directConversationId
        ? { kind: "conversation", id: message.directConversationId }
        : null;
      if (thread) {
        prependMessageToThread(queryClient, chatKeys.messages(thread), message);
      }
      queryClient.invalidateQueries({ queryKey: ["chat", "channels"] });
      queryClient.invalidateQueries({ queryKey: ["chat", "conversations"] });
    });

    connection.on("MessageDeleted", (messageId: string) => {
      updateMessageInAllThreads(queryClient, messageId, (m) => ({
        ...m,
        isDeleted: true,
        body: "",
      }));
    });

    connection.on(
      "ReactionChanged",
      (messageId: string, reactions: ChatReaction[]) => {
        updateMessageInAllThreads(queryClient, messageId, (m) => ({ ...m, reactions }));
      }
    );

    connection.on("UserOnline", (userId: string) => {
      setOnlineUserIds((prev) => new Set(prev).add(userId));
    });

    connection.on("UserOffline", (userId: string) => {
      setOnlineUserIds((prev) => {
        const next = new Set(prev);
        next.delete(userId);
        return next;
      });
    });

    connection.on("UserTyping", (userId: string, threadId: string, isChannel: boolean) => {
      const kind = isChannel ? "channel" : "conversation";
      const key = threadKey(kind, threadId);
      void userId;
      setTypingMap((prev) => {
        const next = new Map(prev);
        next.set(key, { label: "typing", expiresAt: Date.now() + 3000 });
        return next;
      });
    });

    connection.onreconnecting(() => setConnectionState(signalR.HubConnectionState.Reconnecting));
    connection.onreconnected(() => setConnectionState(signalR.HubConnectionState.Connected));
    connection.onclose(() => setConnectionState(signalR.HubConnectionState.Disconnected));

    connection
      .start()
      .then(() => setConnectionState(signalR.HubConnectionState.Connected))
      .catch(() => setConnectionState(signalR.HubConnectionState.Disconnected));

    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [accessToken, queryClient]);

  // Clear expired typing entries roughly once a second.
  useEffect(() => {
    const interval = setInterval(() => {
      setTypingMap((prev) => {
        const now = Date.now();
        let changed = false;
        const next = new Map(prev);
        for (const [key, entry] of prev) {
          if (entry.expiresAt <= now) {
            next.delete(key);
            changed = true;
          }
        }
        return changed ? next : prev;
      });
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  const notifyTyping = useCallback((thread: ChatThreadRef, otherUserName?: string) => {
    void otherUserName;
    const key = threadKey(thread.kind, thread.id);
    const now = Date.now();
    const lastSent = typingDebounceRef.current.get(key) ?? 0;
    if (now - lastSent < 2000) return;
    typingDebounceRef.current.set(key, now);
    connectionRef.current
      ?.invoke("Typing", thread.id, thread.kind === "channel")
      .catch(() => undefined);
  }, []);

  const typingLabelFor = useCallback(
    (thread: ChatThreadRef) => {
      const entry = typingMap.get(threadKey(thread.kind, thread.id));
      if (!entry || entry.expiresAt <= Date.now()) return null;
      return entry.label;
    },
    [typingMap]
  );

  const value = useMemo<ChatSocketContextValue>(
    () => ({ connectionState, onlineUserIds, typingLabelFor, notifyTyping }),
    [connectionState, onlineUserIds, typingLabelFor, notifyTyping]
  );

  return <ChatSocketContext.Provider value={value}>{children}</ChatSocketContext.Provider>;
}

export function useChatSocket() {
  const ctx = useContext(ChatSocketContext);
  if (!ctx) throw new Error("useChatSocket must be used within a ChatSocketProvider");
  return ctx;
}
