import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import TodayIcon from "@mui/icons-material/Today";
import {
  Box,
  Button,
  IconButton,
  Skeleton,
  Stack,
  Tab,
  Tabs,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import {
  addDays,
  addMonths,
  addWeeks,
  endOfMonth,
  endOfWeek,
  format,
  startOfMonth,
  startOfWeek,
  subMonths,
  subWeeks,
} from "date-fns";
import { useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuthStore } from "../../store/authStore";
import { useCalendarEvents } from "../../hooks/useCalendarEvents";
import { AgendaView, DayView, WeekView } from "./AgendaViews";
import { MonthView } from "./MonthView";
import { ResourceCalendarView } from "./ResourceCalendarView";
import { EVENT_TYPE_COLOR, EVENT_TYPE_LABEL } from "./calendarMeta";
import type { CalendarEventType } from "../../types";

type CalendarView = "day" | "week" | "month" | "agenda";

export function CalendarPage() {
  const { projectId } = useParams<{ projectId?: string }>();
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  const [tab, setTab] = useState<"calendar" | "resource">("calendar");
  const [view, setView] = useState<CalendarView>("month");
  const [anchor, setAnchor] = useState(new Date());

  const range = useMemo(() => {
    if (view === "month") {
      return {
        from: startOfWeek(startOfMonth(anchor), { weekStartsOn: 1 }),
        to: endOfWeek(endOfMonth(anchor), { weekStartsOn: 1 }),
      };
    }
    if (view === "week") {
      return { from: startOfWeek(anchor, { weekStartsOn: 1 }), to: endOfWeek(anchor, { weekStartsOn: 1 }) };
    }
    if (view === "day") {
      return { from: anchor, to: anchor };
    }
    return { from: anchor, to: addDays(anchor, 30) };
  }, [view, anchor]);

  const { data, isLoading } = useCalendarEvents({
    organizationId,
    projectId,
    from: format(range.from, "yyyy-MM-dd"),
    to: format(range.to, "yyyy-MM-dd"),
  });

  const events = data?.events ?? [];

  const goPrev = () => {
    if (view === "month") setAnchor((a) => subMonths(a, 1));
    else if (view === "week") setAnchor((a) => subWeeks(a, 1));
    else if (view === "day") setAnchor((a) => addDays(a, -1));
    else setAnchor((a) => addDays(a, -30));
  };
  const goNext = () => {
    if (view === "month") setAnchor((a) => addMonths(a, 1));
    else if (view === "week") setAnchor((a) => addWeeks(a, 1));
    else if (view === "day") setAnchor((a) => addDays(a, 1));
    else setAnchor((a) => addDays(a, 30));
  };
  const goToday = () => setAnchor(new Date());

  const eventTypes: CalendarEventType[] = ["WorkItemDue", "Milestone", "SprintStart", "SprintEnd"];

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" gap={1}>
        <Typography variant="h6" fontWeight={700}>
          {projectId ? "Project calendar" : "Calendar"}
        </Typography>
        <Tabs value={tab} onChange={(_, v) => setTab(v)}>
          <Tab label="Calendar" value="calendar" />
          <Tab label="Resource view" value="resource" />
        </Tabs>
      </Stack>

      {tab === "calendar" && (
        <>
          <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" gap={1.5}>
            <Stack direction="row" spacing={1} alignItems="center">
              <IconButton size="small" onClick={goPrev} aria-label="Previous">
                <ChevronLeftIcon />
              </IconButton>
              <Button size="small" startIcon={<TodayIcon fontSize="small" />} onClick={goToday}>
                Today
              </Button>
              <IconButton size="small" onClick={goNext} aria-label="Next">
                <ChevronRightIcon />
              </IconButton>
              <Typography variant="subtitle1" fontWeight={700} sx={{ ml: 1 }}>
                {view === "day" ? format(anchor, "MMMM d, yyyy") : format(anchor, "MMMM yyyy")}
              </Typography>
            </Stack>
            <ToggleButtonGroup size="small" exclusive value={view} onChange={(_, v) => v && setView(v)}>
              <ToggleButton value="day">Daily</ToggleButton>
              <ToggleButton value="week">Weekly</ToggleButton>
              <ToggleButton value="month">Monthly</ToggleButton>
              <ToggleButton value="agenda">Agenda</ToggleButton>
            </ToggleButtonGroup>
          </Stack>

          <Stack direction="row" spacing={2} flexWrap="wrap">
            {eventTypes.map((t) => (
              <Stack key={t} direction="row" spacing={0.75} alignItems="center">
                <Box sx={{ width: 10, height: 10, borderRadius: "50%", bgcolor: EVENT_TYPE_COLOR[t] }} />
                <Typography variant="caption" color="text.secondary">
                  {EVENT_TYPE_LABEL[t]}
                </Typography>
              </Stack>
            ))}
          </Stack>

          {isLoading ? (
            <Skeleton variant="rounded" height={480} />
          ) : (
            <>
              {view === "month" && <MonthView month={anchor} events={events} />}
              {view === "week" && <WeekView anchor={anchor} events={events} />}
              {view === "day" && <DayView anchor={anchor} events={events} />}
              {view === "agenda" && <AgendaView anchor={anchor} events={events} />}
            </>
          )}
        </>
      )}

      {tab === "resource" && (
        <ResourceCalendarView
          params={{
            organizationId,
            projectId,
            from: format(range.from, "yyyy-MM-dd"),
            to: format(range.to, "yyyy-MM-dd"),
          }}
          from={range.from}
          to={range.to}
        />
      )}
    </Stack>
  );
}
