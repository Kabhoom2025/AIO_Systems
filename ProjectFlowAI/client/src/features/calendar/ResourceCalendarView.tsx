import { Box, Paper, Tooltip, Typography } from "@mui/material";
import { addDays, format } from "date-fns";
import { useMemo } from "react";
import { EmptyState } from "../../components/EmptyState";
import { SkeletonCard } from "../../components/Skeletons";
import { useWorkload } from "../../hooks/useWorkload";
import type { CalendarQueryParams } from "../../types";
import { workloadColor } from "./calendarMeta";

interface ResourceCalendarViewProps {
  params: CalendarQueryParams;
  from: Date;
  to: Date;
}

export function ResourceCalendarView({ params, from, to }: ResourceCalendarViewProps) {
  const { data, isLoading } = useWorkload(params);

  const days = useMemo(() => {
    const result: Date[] = [];
    let cursor = from;
    while (cursor <= to) {
      result.push(cursor);
      cursor = addDays(cursor, 1);
    }
    return result;
  }, [from, to]);

  const { userNames, cellByUserAndDay, maxHours } = useMemo(() => {
    const rows = data?.rows ?? [];
    const names = new Map<string, string>();
    const cells = new Map<string, number>();
    let max = 0;
    rows.forEach((r) => {
      names.set(r.userId, r.userName);
      cells.set(`${r.userId}|${r.date.slice(0, 10)}`, r.allocatedHours);
      if (r.allocatedHours > max) max = r.allocatedHours;
    });
    return { userNames: names, cellByUserAndDay: cells, maxHours: max };
  }, [data]);

  if (isLoading) return <SkeletonCard count={3} />;

  if (userNames.size === 0) {
    return (
      <EmptyState
        title="No workload data"
        description="Nothing is allocated in this date range yet."
      />
    );
  }

  return (
    <Paper variant="outlined" sx={{ overflowX: "auto" }}>
      <Box sx={{ display: "grid", gridTemplateColumns: `200px repeat(${days.length}, 40px)`, minWidth: 200 + days.length * 40 }}>
        <Box sx={{ p: 1, borderBottom: 1, borderRight: 1, borderColor: "divider", bgcolor: "action.hover" }}>
          <Typography variant="caption" fontWeight={700}>
            Person
          </Typography>
        </Box>
        {days.map((d) => (
          <Box
            key={d.toISOString()}
            sx={{
              p: 0.5,
              textAlign: "center",
              borderBottom: 1,
              borderColor: "divider",
              bgcolor: "action.hover",
            }}
          >
            <Typography variant="caption" color="text.secondary">
              {format(d, "d")}
            </Typography>
          </Box>
        ))}

        {[...userNames.entries()].map(([userId, userName]) => (
          <Box key={userId} sx={{ display: "contents" }}>
            <Box sx={{ p: 1, borderBottom: 1, borderRight: 1, borderColor: "divider" }}>
              <Typography variant="body2" noWrap>
                {userName}
              </Typography>
            </Box>
            {days.map((d) => {
              const key = `${userId}|${format(d, "yyyy-MM-dd")}`;
              const hours = cellByUserAndDay.get(key) ?? 0;
              return (
                <Tooltip key={key} title={`${userName} — ${format(d, "MMM d")}: ${hours}h allocated`}>
                  <Box
                    sx={{
                      height: 32,
                      borderBottom: 1,
                      borderColor: "divider",
                      bgcolor: workloadColor(hours, maxHours),
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                    }}
                  >
                    {hours > 0 && (
                      <Typography variant="caption" sx={{ fontSize: "0.6rem", color: "#1B1D28" }}>
                        {hours}
                      </Typography>
                    )}
                  </Box>
                </Tooltip>
              );
            })}
          </Box>
        ))}
      </Box>
    </Paper>
  );
}
