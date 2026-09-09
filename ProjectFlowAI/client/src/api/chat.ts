import { axiosClient } from "./axiosClient";
import type {
  ChatAttachment,
  ChatChannel,
  ChatMessage,
  ChatMessagesResponse,
  CreateChatChannelRequest,
  CreateConversationRequest,
  DirectConversation,
  SendChatMessageRequest,
} from "../types";

export const chatApi = {
  listChannels: (organizationId: string, projectId?: string) =>
    axiosClient
      .get<ChatChannel[]>("/chat/channels", { params: { organizationId, projectId } })
      .then((r) => r.data),

  createChannel: (payload: CreateChatChannelRequest) =>
    axiosClient.post<ChatChannel>("/chat/channels", payload).then((r) => r.data),

  addChannelMember: (channelId: string, userId: string) =>
    axiosClient.post<void>(`/chat/channels/${channelId}/members`, { userId }).then((r) => r.data),

  removeChannelMember: (channelId: string, userId: string) =>
    axiosClient.delete<void>(`/chat/channels/${channelId}/members/${userId}`).then((r) => r.data),

  getChannelMessages: (channelId: string, before?: string, limit = 50) =>
    axiosClient
      .get<ChatMessagesResponse>(`/chat/channels/${channelId}/messages`, {
        params: { before, limit },
      })
      .then((r) => r.data),

  sendChannelMessage: (channelId: string, payload: SendChatMessageRequest) =>
    axiosClient
      .post<ChatMessage>(`/chat/channels/${channelId}/messages`, payload)
      .then((r) => r.data),

  listConversations: (organizationId: string) =>
    axiosClient
      .get<DirectConversation[]>("/chat/conversations", { params: { organizationId } })
      .then((r) => r.data),

  createConversation: (payload: CreateConversationRequest) =>
    axiosClient.post<DirectConversation>("/chat/conversations", payload).then((r) => r.data),

  getConversationMessages: (conversationId: string, before?: string, limit = 50) =>
    axiosClient
      .get<ChatMessagesResponse>(`/chat/conversations/${conversationId}/messages`, {
        params: { before, limit },
      })
      .then((r) => r.data),

  sendConversationMessage: (conversationId: string, payload: SendChatMessageRequest) =>
    axiosClient
      .post<ChatMessage>(`/chat/conversations/${conversationId}/messages`, payload)
      .then((r) => r.data),

  react: (messageId: string, emoji: string) =>
    axiosClient.post<void>(`/chat/messages/${messageId}/reactions`, { emoji }).then((r) => r.data),

  uploadAttachment: (messageId: string, file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return axiosClient
      .post<ChatAttachment>(`/chat/messages/${messageId}/attachments`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      .then((r) => r.data);
  },

  removeMessage: (messageId: string) =>
    axiosClient.delete<void>(`/chat/messages/${messageId}`).then((r) => r.data),

  markChannelRead: (channelId: string) =>
    axiosClient.post<void>(`/chat/channels/${channelId}/read`).then((r) => r.data),
};
