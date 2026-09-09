import { TextField, type TextFieldProps } from "@mui/material";
import { Controller, type FieldValues, type Path, type Control } from "react-hook-form";

interface FormTextFieldProps<T extends FieldValues> {
  name: Path<T>;
  control: Control<T>;
  label: string;
  type?: string;
  fullWidth?: boolean;
  autoComplete?: string;
  multiline?: boolean;
  rows?: number;
  textFieldProps?: Partial<TextFieldProps>;
}

export function FormTextField<T extends FieldValues>({
  name,
  control,
  label,
  type = "text",
  fullWidth = true,
  autoComplete,
  multiline,
  rows,
  textFieldProps,
}: FormTextFieldProps<T>) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <TextField
          {...field}
          {...textFieldProps}
          id={`field-${name}`}
          label={label}
          type={type}
          fullWidth={fullWidth}
          autoComplete={autoComplete}
          multiline={multiline}
          rows={rows}
          error={!!fieldState.error}
          helperText={fieldState.error?.message}
          value={field.value ?? ""}
          slotProps={{
            htmlInput: {
              "aria-label": label,
            },
          }}
        />
      )}
    />
  );
}
