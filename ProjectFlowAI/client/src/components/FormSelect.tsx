import { FormControl, FormHelperText, InputLabel, MenuItem, Select } from "@mui/material";
import { Controller, type Control, type FieldValues, type Path } from "react-hook-form";

export interface FormSelectOption {
  value: string;
  label: string;
}

interface FormSelectProps<T extends FieldValues> {
  name: Path<T>;
  control: Control<T>;
  label: string;
  options: FormSelectOption[];
  fullWidth?: boolean;
}

export function FormSelect<T extends FieldValues>({
  name,
  control,
  label,
  options,
  fullWidth = true,
}: FormSelectProps<T>) {
  const labelId = `select-label-${name}`;
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <FormControl fullWidth={fullWidth} error={!!fieldState.error}>
          <InputLabel id={labelId}>{label}</InputLabel>
          <Select
            {...field}
            labelId={labelId}
            label={label}
            value={field.value ?? ""}
            inputProps={{ "aria-label": label }}
          >
            {options.map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </Select>
          {fieldState.error?.message && <FormHelperText>{fieldState.error.message}</FormHelperText>}
        </FormControl>
      )}
    />
  );
}
