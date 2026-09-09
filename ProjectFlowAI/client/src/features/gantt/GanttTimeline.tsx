import { useTheme } from "@mui/material/styles";
import { Box, Tooltip, Typography } from "@mui/material";
import { addDays, differenceInCalendarDays, format, isMonday, startOfMonth } from "date-fns";
import { useMemo } from "react";
import {
  HEADER_HEIGHT,
  LABEL_COLUMN_WIDTH,
  ROW_HEIGHT,
  STATUS_BAR_COLOR,
  ZOOM_PX_PER_DAY,
  CRITICAL_PATH_COLOR,
  type GanttZoom,
} from "./ganttMeta";
import type { GanttBaselineDetail, GanttChart } from "../../types";

interface GanttTimelineProps {
  chart: GanttChart;
  zoom: GanttZoom;
  criticalPathIds: Set<string> | null;
  baseline: GanttBaselineDetail | null;
  onItemClick?: (id: string) => void;
}

interface Tick {
  offsetPx: number;
  label: string;
  isBoundary: boolean;
}

function buildTicks(zoom: GanttZoom, rangeStart: Date, totalDays: number, pxPerDay: number): Tick[] {
  const ticks: Tick[] = [];
  for (let i = 0; i < totalDays; i++) {
    const day = addDays(rangeStart, i);
    if (zoom === "day") {
      ticks.push({ offsetPx: i * pxPerDay, label: format(day, "EEE d"), isBoundary: true });
    } else if (zoom === "week") {
      if (isMonday(day) || i === 0) {
        ticks.push({ offsetPx: i * pxPerDay, label: format(day, "MMM d"), isBoundary: true });
      }
    } else {
      if (day.getDate() === 1 || i === 0) {
        ticks.push({ offsetPx: i * pxPerDay, label: format(day, "MMM yyyy"), isBoundary: true });
      }
    }
  }
  return ticks;
}

export function GanttTimeline({ chart, zoom, criticalPathIds, baseline, onItemClick }: GanttTimelineProps) {
  const theme = useTheme();
  const pxPerDay = ZOOM_PX_PER_DAY[zoom];

  const { rangeStart, totalDays, rows, milestoneRow } = useMemo(() => {
    const allDates: Date[] = [];
    chart.items.forEach((i) => {
      allDates.push(new Date(i.startDate), new Date(i.endDate));
    });
    chart.milestones.forEach((m) => allDates.push(new Date(m.dueDate)));
    if (baseline) {
      baseline.items.forEach((i) => {
        allDates.push(new Date(i.plannedStartDate), new Date(i.plannedEndDate));
      });
    }
    if (allDates.length === 0) {
      const today = new Date();
      allDates.push(today, addDays(today, 14));
    }
    const minDate = new Date(Math.min(...allDates.map((d) => d.getTime())));
    const maxDate = new Date(Math.max(...allDates.map((d) => d.getTime())));
    const start = addDays(startOfMonth(minDate), -3);
    const end = addDays(maxDate, 5);
    const days = Math.max(differenceInCalendarDays(end, start) + 1, 14);

    const itemRows = chart.items.map((item, index) => ({ item, rowIndex: index }));

    return {
      rangeStart: start,
      totalDays: days,
      rows: itemRows,
      milestoneRow: chart.milestones.length > 0,
    };
  }, [chart, baseline]);

  const totalWidth = totalDays * pxPerDay;
  const ticks = useMemo(() => buildTicks(zoom, rangeStart, totalDays, pxPerDay), [zoom, rangeStart, totalDays, pxPerDay]);

  const rowIndexById = useMemo(() => {
    const map = new Map<string, number>();
    rows.forEach(({ item, rowIndex }) => map.set(item.id, rowIndex));
    return map;
  }, [rows]);

  const milestoneOffset = milestoneRow ? ROW_HEIGHT : 0;
  const bodyHeight = milestoneOffset + rows.length * ROW_HEIGHT;

  const baselineByWorkItemId = useMemo(() => {
    const map = new Map<string, GanttBaselineDetail["items"][number]>();
    baseline?.items.forEach((i) => map.set(i.workItemId, i));
    return map;
  }, [baseline]);

  const dayOffset = (date: string | Date) => differenceInCalendarDays(new Date(date), rangeStart) * pxPerDay;

  return (
    <Box sx={{ display: "flex", border: 1, borderColor: "divider", borderRadius: 1, overflow: "hidden" }}>
      {/* Fixed label column */}
      <Box
        sx={{
          width: LABEL_COLUMN_WIDTH,
          flexShrink: 0,
          borderRight: 1,
          borderColor: "divider",
          bgcolor: "background.paper",
        }}
      >
        <Box
          sx={{
            height: HEADER_HEIGHT,
            display: "flex",
            alignItems: "center",
            px: 1.5,
            borderBottom: 1,
            borderColor: "divider",
            fontWeight: 700,
          }}
        >
          <Typography variant="subtitle2" fontWeight={700}>
            Work item
          </Typography>
        </Box>
        {milestoneRow && (
          <Box
            sx={{
              height: ROW_HEIGHT,
              display: "flex",
              alignItems: "center",
              px: 1.5,
              borderBottom: 1,
              borderColor: "divider",
              color: "text.secondary",
              fontSize: 13,
            }}
          >
            Milestones
          </Box>
        )}
        {rows.map(({ item }) => {
          const isCritical = criticalPathIds?.has(item.id) ?? false;
          return (
            <Box
              key={item.id}
              onClick={() => onItemClick?.(item.id)}
              sx={{
                height: ROW_HEIGHT,
                display: "flex",
                alignItems: "center",
                px: 1.5,
                borderBottom: 1,
                borderColor: "divider",
                cursor: onItemClick ? "pointer" : "default",
                "&:hover": onItemClick ? { bgcolor: "action.hover" } : undefined,
              }}
            >
              <Typography
                variant="body2"
                noWrap
                sx={{ fontWeight: isCritical ? 700 : 400, color: isCritical ? CRITICAL_PATH_COLOR : "text.primary" }}
                title={item.title}
              >
                {item.title}
              </Typography>
            </Box>
          );
        })}
      </Box>

      {/* Scrollable timeline */}
      <Box sx={{ overflowX: "auto", flex: 1 }}>
        <Box sx={{ position: "relative", width: totalWidth, minWidth: "100%" }}>
          {/* Header */}
          <Box
            sx={{
              height: HEADER_HEIGHT,
              position: "sticky",
              borderBottom: 1,
              borderColor: "divider",
              bgcolor: "background.paper",
              top: 0,
              zIndex: 2,
            }}
          >
            {ticks.map((t, idx) => (
              <Box
                key={idx}
                sx={{
                  position: "absolute",
                  left: t.offsetPx,
                  top: 0,
                  height: "100%",
                  display: "flex",
                  alignItems: "center",
                  pl: 0.5,
                  borderLeft: 1,
                  borderColor: "divider",
                }}
              >
                <Typography variant="caption" color="text.secondary" sx={{ whiteSpace: "nowrap" }}>
                  {t.label}
                </Typography>
              </Box>
            ))}
          </Box>

          {/* Body with vertical gridlines */}
          <Box sx={{ position: "relative", height: bodyHeight }}>
            {ticks.map((t, idx) => (
              <Box
                key={idx}
                sx={{
                  position: "absolute",
                  left: t.offsetPx,
                  top: 0,
                  bottom: 0,
                  borderLeft: 1,
                  borderColor: "divider",
                  opacity: 0.6,
                }}
              />
            ))}

            {/* Row backgrounds */}
            {rows.map(({ rowIndex }) => (
              <Box
                key={rowIndex}
                sx={{
                  position: "absolute",
                  left: 0,
                  right: 0,
                  top: milestoneOffset + rowIndex * ROW_HEIGHT,
                  height: ROW_HEIGHT,
                  borderBottom: 1,
                  borderColor: "divider",
                }}
              />
            ))}
            {milestoneRow && (
              <Box
                sx={{
                  position: "absolute",
                  left: 0,
                  right: 0,
                  top: 0,
                  height: ROW_HEIGHT,
                  borderBottom: 1,
                  borderColor: "divider",
                }}
              />
            )}

            {/* Milestones */}
            {chart.milestones.map((m) => (
              <Tooltip key={m.id} title={`${m.name} — ${m.dueDate.slice(0, 10)}`}>
                <Box
                  sx={{
                    position: "absolute",
                    left: dayOffset(m.dueDate) - 7,
                    top: ROW_HEIGHT / 2 - 7,
                    width: 14,
                    height: 14,
                    bgcolor: theme.palette.warning.main,
                    transform: "rotate(45deg)",
                    borderRadius: 0.5,
                  }}
                />
              </Tooltip>
            ))}

            {/* Dependency lines */}
            <svg
              width={totalWidth}
              height={bodyHeight}
              style={{ position: "absolute", left: 0, top: 0, pointerEvents: "none" }}
            >
              <defs>
                <marker id="gantt-arrow" markerWidth="8" markerHeight="8" refX="6" refY="3" orient="auto">
                  <path d="M0,0 L6,3 L0,6 Z" fill={theme.palette.text.disabled} />
                </marker>
              </defs>
              {rows.flatMap(({ item, rowIndex }) =>
                item.dependencies.map((dep) => {
                  const fromRow = rowIndexById.get(dep.dependsOnWorkItemId);
                  const fromItem = chart.items.find((i) => i.id === dep.dependsOnWorkItemId);
                  if (fromRow === undefined || !fromItem) return null;
                  const x1 = dayOffset(fromItem.endDate) + pxPerDay;
                  const y1 = milestoneOffset + fromRow * ROW_HEIGHT + ROW_HEIGHT / 2;
                  const x2 = dayOffset(item.startDate);
                  const y2 = milestoneOffset + rowIndex * ROW_HEIGHT + ROW_HEIGHT / 2;
                  const midX = x1 + Math.max((x2 - x1) / 2, 8);
                  return (
                    <path
                      key={`${dep.dependsOnWorkItemId}-${item.id}`}
                      d={`M ${x1} ${y1} L ${midX} ${y1} L ${midX} ${y2} L ${x2} ${y2}`}
                      fill="none"
                      stroke={theme.palette.text.disabled}
                      strokeWidth={1.5}
                      markerEnd="url(#gantt-arrow)"
                    />
                  );
                })
              )}
            </svg>

            {/* Baseline ghost bars + actual bars */}
            {rows.map(({ item, rowIndex }) => {
              const top = milestoneOffset + rowIndex * ROW_HEIGHT;
              const isCritical = criticalPathIds?.has(item.id) ?? false;
              const baselineItem = baselineByWorkItemId.get(item.id);
              const barLeft = dayOffset(item.startDate);
              const barWidth = Math.max(
                (differenceInCalendarDays(new Date(item.endDate), new Date(item.startDate)) + 1) * pxPerDay,
                6
              );
              return (
                <Box key={item.id}>
                  {baselineItem && (
                    <Box
                      sx={{
                        position: "absolute",
                        left: dayOffset(baselineItem.plannedStartDate),
                        top: top + 4,
                        width: Math.max(
                          (differenceInCalendarDays(
                            new Date(baselineItem.plannedEndDate),
                            new Date(baselineItem.plannedStartDate)
                          ) +
                            1) *
                            pxPerDay,
                          6
                        ),
                        height: 10,
                        border: "1.5px dashed",
                        borderColor: "text.disabled",
                        borderRadius: 0.5,
                      }}
                    />
                  )}
                  <Tooltip
                    title={`${item.title} — ${item.status} — ${Math.round(item.progress * 100)}% complete`}
                  >
                    <Box
                      onClick={() => onItemClick?.(item.id)}
                      sx={{
                        position: "absolute",
                        left: barLeft,
                        top: top + (baselineItem ? 17 : 12),
                        width: barWidth,
                        height: 18,
                        borderRadius: 1,
                        bgcolor: STATUS_BAR_COLOR[item.status],
                        border: isCritical ? `2px solid ${CRITICAL_PATH_COLOR}` : "none",
                        cursor: onItemClick ? "pointer" : "default",
                        overflow: "hidden",
                        boxShadow: isCritical ? `0 0 0 2px ${CRITICAL_PATH_COLOR}33` : "none",
                      }}
                    >
                      <Box
                        sx={{
                          height: "100%",
                          width: `${Math.min(Math.max(item.progress, 0), 1) * 100}%`,
                          bgcolor: "rgba(0,0,0,0.25)",
                        }}
                      />
                    </Box>
                  </Tooltip>
                </Box>
              );
            })}
          </Box>
        </Box>
      </Box>
    </Box>
  );
}
