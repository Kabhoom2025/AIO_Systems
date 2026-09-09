import { Card, CardContent, Chip, Stack, Switch, TextField, Typography } from "@mui/material";
import { format, startOfMonth } from "date-fns";
import { useState } from "react";
import { DataTable, type DataTableColumn } from "../../components/DataTable";
import { useSetTimeLogBillable, useTimesheet } from "../../hooks/useTimesheet";
import { useAuthStore } from "../../store/authStore";
import type { TimesheetEntry } from "../../types";

function formatHours(minutes: number): string {
  return (minutes / 60).toFixed(1);
}

export function TimesheetsPage() {
  const user = useAuthStore((s) => s.user);
  const [from, setFrom] = useState(format(startOfMonth(new Date()), "yyyy-MM-dd"));
  const [to, setTo] = useState(format(new Date(), "yyyy-MM-dd"));

  const { data, isLoading } = useTimesheet({ userId: user?.id ?? "", from, to });
  const setBillable = useSetTimeLogBillable();

  const entries = data?.entries ?? [];

  const columns: DataTableColumn<TimesheetEntry>[] = [
    { key: "date", label: "Date", render: (row) => row.date.slice(0, 10) },
    { key: "projectName", label: "Project", render: (row) => row.projectName },
    { key: "workItemTitle", label: "Work item", render: (row) => row.workItemTitle },
    {
      key: "minutes",
      label: "Hours",
      align: "right",
      render: (row) => formatHours(row.minutes),
    },
    {
      key: "isBillable",
      label: "Billable",
      align: "right",
      render: (row) => (
        <Stack direction="row" spacing={1} alignItems="center" justifyContent="flex-end">
          <Chip
            size="small"
            label={row.isBillable ? "Billable" : "Non-billable"}
            color={row.isBillable ? "success" : "default"}
          />
          <Switch
            size="small"
            checked={row.isBillable}
            disabled={setBillable.isPending}
            onChange={(e) => setBillable.mutate({ timeLogId: row.id, isBillable: e.target.checked })}
          />
        </Stack>
      ),
    },
  ];

  return (
    <Stack spacing={2}>
      <Typography variant="h5" fontWeight={700}>
        My Time
      </Typography>

      <Stack direction="row" spacing={2}>
        <TextField
          size="small"
          type="date"
          label="From"
          value={from}
          onChange={(e) => setFrom(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          size="small"
          type="date"
          label="To"
          value={to}
          onChange={(e) => setTo(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
      </Stack>

      <Stack direction="row" spacing={2}>
        <Card variant="outlined" sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="caption" color="text.secondary">
              Total hours
            </Typography>
            <Typography variant="h5" fontWeight={700}>
              {isLoading ? "—" : formatHours(data?.totalMinutes ?? 0)}
            </Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="caption" color="text.secondary">
              Billable hours
            </Typography>
            <Typography variant="h5" fontWeight={700} color="success.main">
              {isLoading ? "—" : formatHours(data?.billableMinutes ?? 0)}
            </Typography>
          </CardContent>
        </Card>
      </Stack>

      <DataTable
        columns={columns}
        rows={entries}
        getRowId={(row) => row.id}
        isLoading={isLoading}
        totalCount={entries.length}
        page={0}
        pageSize={entries.length || 10}
        onPageChange={() => {}}
        onPageSizeChange={() => {}}
        emptyTitle="No time logged"
        emptyDescription="Time entries logged in this range will show up here."
      />
    </Stack>
  );
}
