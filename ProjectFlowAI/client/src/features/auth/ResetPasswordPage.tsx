import { zodResolver } from "@hookform/resolvers/zod";
import { Alert, Box, Button, CircularProgress, Stack } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { useNavigate, useSearchParams } from "react-router-dom";
import { authApi } from "../../api/auth";
import { FormTextField } from "../../components/FormTextField";
import { AuthLayout } from "./AuthLayout";
import { resetPasswordSchema, type ResetPasswordFormValues } from "./schemas";

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") ?? "";
  const navigate = useNavigate();

  const { control, handleSubmit } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: "", confirmPassword: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: ResetPasswordFormValues) =>
      authApi.resetPassword({ token, newPassword: values.newPassword }),
    onSuccess: () => {
      setTimeout(() => navigate("/login"), 1500);
    },
  });

  const onSubmit = (values: ResetPasswordFormValues) => mutation.mutate(values);

  return (
    <AuthLayout title="Reset your password" subtitle="Choose a new password for your account">
      {!token && <Alert severity="warning" sx={{ mb: 2 }}>Missing or invalid reset token.</Alert>}
      {mutation.isSuccess ? (
        <Alert severity="success">Password reset. Redirecting to sign in...</Alert>
      ) : (
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            {mutation.error && <Alert severity="error">{mutation.error.message}</Alert>}
            <FormTextField
              name="newPassword"
              control={control}
              label="New password"
              type="password"
              autoComplete="new-password"
            />
            <FormTextField
              name="confirmPassword"
              control={control}
              label="Confirm new password"
              type="password"
              autoComplete="new-password"
            />
            <Button
              type="submit"
              variant="contained"
              size="large"
              disabled={mutation.isPending || !token}
              startIcon={mutation.isPending ? <CircularProgress size={18} /> : undefined}
            >
              Reset password
            </Button>
          </Stack>
        </Box>
      )}
    </AuthLayout>
  );
}
