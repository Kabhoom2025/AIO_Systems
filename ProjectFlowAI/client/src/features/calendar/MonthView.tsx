import { Box, Chip, Paper, Stack, Tooltip, Typography } from "@mui/material";
import {
  addDays,
  endOfMonth,
  endOfWeek,
  format,
  isSameMonth,
  isToday,
  startOfMonth,
  startOfWeek,
} from "date-fns";
import { useMemo } from "react";
import type { CalendarEvent } from "../../types";
import { EVENT_TYPE_COLOR } from "./calendarMeta";

interface MonthViewProps {
  month: Date;
  events: CalendarEvent[];
  onEventClick?: (event: CalendarEvent) => void;
}

const WEEKDAY_LABELS = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

export function MonthView({ month, events, onEventClick }: MonthViewProps) {
  const days = useMemo(() => {
    const start = startOfWeek(startOfMonth(month), { weekStartsOn: 1 });
    const end = endOfWeek(endOfMonth(month), { weekStartsOn: 1 });
    const result: Date[] = [];
    let cursor = start;
    while (cursor <= end) {
      result.push(cursor);
      cursor = addDays(cursor, 1);
    }
    return result;
  }, [month]);

  const eventsByDay = useMemo(() => {
    const map = new Map<string, CalendarEvent[]>();
    events.forEach((e) => {
      const key = e.date.slice(0, 10);
      const list = map.get(key) ?? [];
      list.push(e);
      map.set(key, list);
    });
    return map;
  }, [events]);

  return (
    <Paper variant="outlined">
      <Box sx={{ display: "grid", gridTemplateColumns: "repeat(7, 1fr)" }}>
        {WEEKDAY_LABELS.map((label) => (
          <Box
            key={label}
            sx={{
              p: 1,
              textAlign: "center",
              borderBottom: 1,
              borderColor: "divider",
              bgcolor: "action.hover",
            }}
          >
            <Typography variant="caption" fontWeight={700} color="text.secondary">
              {label}
            </Typography>
          </Box>
        ))}
        {days.map((day) => {
          const key = format(day, "yyyy-MM-dd");
          const dayEvents = eventsByDay.get(key) ?? [];
          const inMonth = isSameMonth(day, month);
          return (
            <Box
              key={key}
              sx={{
                minHeight: 108,
                p: 0.75,
                borderRight: 1,
                borderBottom: 1,
                borderColor: "divider",
                bgcolor: inMonth ? "background.paper" : "action.hover",
                opacity: inMonth ? 1 : 0.55,
              }}
            >
              <Stack direction="row" justifyContent="flex-end">
                <Box
                  sx={{
                    width: 24,
                    height: 24,
                    borderRadius: "50%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    bgcolor: isToday(day) ? "primary.main" : "transparent",
                    color: isToday(day) ? "primary.contrastText" : "text.primary",
                  }}
                >
                  <Typography variant="caption" fontWeight={isToday(day) ? 700 : 400}>
                    {format(day, "d")}
                  </Typography>
                </Box>
              </Stack>
              <Stack spacing={0.4} sx={{ mt: 0.5 }}>
                {dayEvents.slice(0, 3).map((e) => (
                  <Tooltip
                    key={e.id}
                    title={`${e.title}${e.projectName ? ` — ${e.projectName}` : ""}`}
                  >
                    <Chip
                      size="small"
                      label={e.title}
                      onClick={onEventClick ? () => onEventClick(e) : undefined}
                      sx={{
                        height: 18,
                        fontSize: "0.65rem",
                        justifyContent: "flex-start",
                        bgcolor: EVENT_TYPE_COLOR[e.type],
                        color: "#fff",
                        "& .MuiChip-label": { px: 0.75, overflow: "hidden", textOverflow: "ellipsis" },
                      }}
                    />
                  </Tooltip>
                ))}
                {dayEvents.length > 3 && (
                  <Typography variant="caption" color="text.secondary">
                    +{dayEvents.length - 3} more
                  </Typography>
                )}
              </Stack>
            </Box>
          );
        })}
      </Box>
    </Paper>
  );
}
