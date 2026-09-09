import { axiosClient } from "./axiosClient";
import type {
  CreateDocPageCommentRequest,
  CreateDocPageRequest,
  DocPageComment,
  DocPageDetail,
  DocPageListParams,
  DocPageSummary,
  DocPageVersionDetail,
  DocPageVersionSummary,
  UpdateDocPageRequest,
} from "../types";

export const docPagesApi = {
  list: (params: DocPageListParams) =>
    axiosClient.get<DocPageSummary[]>("/doc-pages", { params }).then((r) => r.data),

  get: (id: string) => axiosClient.get<DocPageDetail>(`/doc-pages/${id}`).then((r) => r.data),

  create: (payload: CreateDocPageRequest) =>
    axiosClient.post<DocPageDetail>("/doc-pages", payload).then((r) => r.data),

  update: (id: string, payload: UpdateDocPageRequest) =>
    axiosClient.put<DocPageDetail>(`/doc-pages/${id}`, payload).then((r) => r.data),

  remove: (id: string) => axiosClient.delete<void>(`/doc-pages/${id}`).then((r) => r.data),

  listVersions: (id: string) =>
    axiosClient.get<DocPageVersionSummary[]>(`/doc-pages/${id}/versions`).then((r) => r.data),

  getVersion: (id: string, versionId: string) =>
    axiosClient
      .get<DocPageVersionDetail>(`/doc-pages/${id}/versions/${versionId}`)
      .then((r) => r.data),

  restoreVersion: (id: string, versionId: string) =>
    axiosClient
      .post<DocPageDetail>(`/doc-pages/${id}/versions/${versionId}/restore`)
      .then((r) => r.data),

  listComments: (id: string) =>
    axiosClient.get<DocPageComment[]>(`/doc-pages/${id}/comments`).then((r) => r.data),

  addComment: (id: string, payload: CreateDocPageCommentRequest) =>
    axiosClient.post<DocPageComment>(`/doc-pages/${id}/comments`, payload).then((r) => r.data),

  removeComment: (commentId: string) =>
    axiosClient.delete<void>(`/doc-page-comments/${commentId}`).then((r) => r.data),

  uploadImage: (id: string, file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return axiosClient
      .post<{ url: string }>(`/doc-pages/${id}/images`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      .then((r) => r.data);
  },
};
