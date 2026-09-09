import { Chip, MenuItem, Select, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { AppShell } from "../../components/AppShell";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { NoOrganizationState } from "../../components/NoOrganizationState";
import { useAuditLogs } from "../../hooks/useAuditLogs";
import { useAuthStore } from "../../store/authStore";
import type { AuditLog } from "../../types";

const ENTITY_TYPES = ["Organization", "Department", "Team", "User", "Role", "Invitation"];

export function AuditLogsPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const [entityType, setEntityType] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  const { data, isLoading, isFetching } = useAuditLogs({
    organizationId,
    entityType: entityType || undefined,
    from: from || undefined,
    to: to || undefined,
    page: page + 1,
    pageSize,
  });

  const columns: DataTableColumn<AuditLog>[] = [
    {
      key: "createdAt",
      label: "Date",
      render: (log) => new Date(log.createdAt).toLocaleString(),
    },
    { key: "action", label: "Action", render: (log) => log.action },
    {
      key: "entityType",
      label: "Entity",
      render: (log) => <Chip label={log.entityType} size="small" variant="outlined" />,
    },
    { key: "entityId", label: "Entity ID", render: (log) => log.entityId },
    { key: "userId", label: "User ID", render: (log) => log.userId },
  ];

  if (!organizationId) return <NoOrganizationState />;

  return (
    <AppShell>
      <Stack spacing={3}>
        <Typography variant="h5" fontWeight={700}>
          Audit logs
        </Typography>

        <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
          <Select
            size="small"
            displayEmpty
            value={entityType}
            onChange={(e) => {
              setEntityType(e.target.value);
              setPage(0);
            }}
            sx={{ minWidth: 200 }}
            aria-label="Filter by entity type"
          >
            <MenuItem value="">All entity types</MenuItem>
            {ENTITY_TYPES.map((t) => (
              <MenuItem key={t} value={t}>
                {t}
              </MenuItem>
            ))}
          </Select>
          <TextField
            size="small"
            label="From"
            type="date"
            value={from}
            onChange={(e) => {
              setFrom(e.target.value);
              setPage(0);
            }}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <TextField
            size="small"
            label="To"
            type="date"
            value={to}
            onChange={(e) => {
              setTo(e.target.value);
              setPage(0);
            }}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Stack>

        <DataTable
          columns={columns}
          rows={data?.items ?? []}
          getRowId={(log) => log.id}
          isLoading={isLoading || isFetching}
          totalCount={data?.totalCount ?? 0}
          page={page}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPage(0);
          }}
          emptyTitle="No audit log entries"
          emptyDescription="Activity in your organization will appear here."
        />
      </Stack>
    </AppShell>
  );
}
