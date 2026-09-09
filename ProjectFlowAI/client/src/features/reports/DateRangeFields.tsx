import { Stack, TextField } from "@mui/material";

interface DateRangeFieldsProps {
  from: string;
  to: string;
  onFromChange: (value: string) => void;
  onToChange: (value: string) => void;
}

/** Matches the plain type="date" TextField convention already used in TimesheetsPage
 * and SprintsPage rather than introducing the (installed but unused elsewhere)
 * @mui/x-date-pickers dependency for these filters. */
export function DateRangeFields({ from, to, onFromChange, onToChange }: DateRangeFieldsProps) {
  return (
    <Stack direction="row" spacing={2}>
      <TextField
        size="small"
        type="date"
        label="From"
        value={from}
        onChange={(e) => onFromChange(e.target.value)}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <TextField
        size="small"
        type="date"
        label="To"
        value={to}
        onChange={(e) => onToChange(e.target.value)}
        slotProps={{ inputLabel: { shrink: true } }}
      />
    </Stack>
  );
}
