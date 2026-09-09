import { axiosClient } from "./axiosClient";
import type { AcceptInvitationRequest, CreateInvitationRequest, Invitation, PagedResult } from "../types";

export const invitationsApi = {
  // Backend returns a PagedResult, not a flat array.
  list: (organizationId: string) =>
    axiosClient
      .get<PagedResult<Invitation>>("/invitations", { params: { organizationId, pageSize: 500 } })
      .then((r) => r.data.items),

  create: (payload: CreateInvitationRequest) =>
    axiosClient.post<Invitation>("/invitations", payload).then((r) => r.data),

  accept: (payload: AcceptInvitationRequest) =>
    axiosClient.post<void>("/invitations/accept", payload).then((r) => r.data),

  revoke: (id: string) => axiosClient.delete<void>(`/invitations/${id}`).then((r) => r.data),
};
