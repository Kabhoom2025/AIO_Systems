import SearchIcon from "@mui/icons-material/Search";
import { Avatar, Chip, InputAdornment, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { AppShell } from "../../components/AppShell";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { NoOrganizationState } from "../../components/NoOrganizationState";
import { useUsers } from "../../hooks/useUsers";
import { useAuthStore } from "../../store/authStore";
import type { User } from "../../types";

export function UsersPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState<string>("email");
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");

  const { data, isLoading, isFetching } = useUsers({
    organizationId,
    search: search || undefined,
    page: page + 1,
    pageSize,
    sortBy,
    sortDir,
  });

  const columns: DataTableColumn<User>[] = [
    {
      key: "name",
      label: "Name",
      sortable: true,
      render: (u) => (
        <Stack direction="row" spacing={1.5} alignItems="center">
          <Avatar src={u.avatarUrl ?? undefined} sx={{ width: 32, height: 32, fontSize: 13 }}>
            {u.firstName?.[0]}
            {u.lastName?.[0]}
          </Avatar>
          <Typography variant="body2">
            {u.firstName} {u.lastName}
          </Typography>
        </Stack>
      ),
    },
    {
      key: "email",
      label: "Email",
      sortable: true,
      render: (u) => u.email,
    },
    {
      key: "roles",
      label: "Roles",
      render: (u) => (
        <Stack direction="row" spacing={0.5} flexWrap="wrap">
          {u.roles.map((r) => (
            <Chip key={r} label={r} size="small" variant="outlined" />
          ))}
        </Stack>
      ),
    },
    {
      key: "status",
      label: "Status",
      render: (u) => (
        <Chip
          label={u.isEmailVerified ? "Verified" : "Pending"}
          size="small"
          color={u.isEmailVerified ? "success" : "warning"}
          variant="outlined"
        />
      ),
    },
  ];

  if (!organizationId) return <NoOrganizationState />;

  return (
    <AppShell>
      <Stack spacing={3}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography variant="h5" fontWeight={700}>
            Users
          </Typography>
          <TextField
            size="small"
            placeholder="Search users..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(0);
            }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              },
            }}
            sx={{ width: 280 }}
            aria-label="Search users"
          />
        </Stack>

        <DataTable
          columns={columns}
          rows={data?.items ?? []}
          getRowId={(u) => u.id}
          isLoading={isLoading || isFetching}
          totalCount={data?.totalCount ?? 0}
          page={page}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPage(0);
          }}
          sortBy={sortBy}
          sortDir={sortDir}
          onSortChange={(by, dir) => {
            setSortBy(by);
            setSortDir(dir);
          }}
          emptyTitle="No users found"
          emptyDescription="Invite teammates to your organization to see them here."
        />
      </Stack>
    </AppShell>
  );
}
