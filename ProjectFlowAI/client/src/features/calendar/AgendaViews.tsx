import { Box, Chip, List, ListItem, ListItemText, Paper, Stack, Typography } from "@mui/material";
import { addDays, format, isSameDay, isWithinInterval, startOfWeek } from "date-fns";
import { useMemo } from "react";
import { EmptyState } from "../../components/EmptyState";
import type { CalendarEvent } from "../../types";
import { EVENT_TYPE_COLOR, EVENT_TYPE_LABEL } from "./calendarMeta";

function EventRow({ event }: { event: CalendarEvent }) {
  return (
    <ListItem
      sx={{
        borderLeft: 4,
        borderColor: EVENT_TYPE_COLOR[event.type],
        mb: 0.5,
        bgcolor: "background.paper",
        borderRadius: 1,
      }}
    >
      <ListItemText
        primary={event.title}
        secondary={
          <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 0.25 }}>
            <Chip size="small" label={EVENT_TYPE_LABEL[event.type]} sx={{ height: 18, fontSize: "0.65rem" }} />
            {event.projectName && (
              <Typography variant="caption" color="text.secondary">
                {event.projectName}
              </Typography>
            )}
          </Stack>
        }
      />
    </ListItem>
  );
}

interface ViewProps {
  anchor: Date;
  events: CalendarEvent[];
}

export function DayView({ anchor, events }: ViewProps) {
  const dayEvents = useMemo(
    () => events.filter((e) => isSameDay(new Date(e.date), anchor)),
    [events, anchor]
  );
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 1.5 }}>
        {format(anchor, "EEEE, MMMM d yyyy")}
      </Typography>
      {dayEvents.length === 0 ? (
        <EmptyState title="No events" description="Nothing scheduled for this day." />
      ) : (
        <List disablePadding>
          {dayEvents.map((e) => (
            <EventRow key={e.id} event={e} />
          ))}
        </List>
      )}
    </Paper>
  );
}

export function WeekView({ anchor, events }: ViewProps) {
  const weekStart = startOfWeek(anchor, { weekStartsOn: 1 });
  const days = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i));

  return (
    <Stack spacing={1.5}>
      {days.map((day) => {
        const dayEvents = events.filter((e) => isSameDay(new Date(e.date), day));
        return (
          <Paper key={day.toISOString()} variant="outlined" sx={{ p: 1.5 }}>
            <Typography variant="subtitle2" fontWeight={700} sx={{ mb: dayEvents.length ? 1 : 0 }}>
              {format(day, "EEE, MMM d")}
            </Typography>
            {dayEvents.length > 0 && (
              <List disablePadding>
                {dayEvents.map((e) => (
                  <EventRow key={e.id} event={e} />
                ))}
              </List>
            )}
          </Paper>
        );
      })}
    </Stack>
  );
}

export function AgendaView({ anchor, events }: ViewProps) {
  const upcoming = useMemo(() => {
    const from = anchor;
    const to = addDays(anchor, 30);
    return [...events]
      .filter((e) => isWithinInterval(new Date(e.date), { start: from, end: to }))
      .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
  }, [events, anchor]);

  const byDay = useMemo(() => {
    const map = new Map<string, CalendarEvent[]>();
    upcoming.forEach((e) => {
      const key = e.date.slice(0, 10);
      const list = map.get(key) ?? [];
      list.push(e);
      map.set(key, list);
    });
    return map;
  }, [upcoming]);

  if (upcoming.length === 0) {
    return <EmptyState title="Nothing on the agenda" description="No events in the next 30 days." />;
  }

  return (
    <Stack spacing={1.5}>
      {[...byDay.entries()].map(([day, dayEvents]) => (
        <Box key={day}>
          <Typography variant="subtitle2" fontWeight={700} sx={{ mb: 0.5 }}>
            {format(new Date(day), "EEEE, MMMM d")}
          </Typography>
          <List disablePadding>
            {dayEvents.map((e) => (
              <EventRow key={e.id} event={e} />
            ))}
          </List>
        </Box>
      ))}
    </Stack>
  );
}
