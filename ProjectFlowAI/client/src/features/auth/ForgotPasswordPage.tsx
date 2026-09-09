import { zodResolver } from "@hookform/resolvers/zod";
import { Alert, Box, Button, CircularProgress, Stack, Typography } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link as RouterLink } from "react-router-dom";
import { authApi } from "../../api/auth";
import { FormTextField } from "../../components/FormTextField";
import { AuthLayout } from "./AuthLayout";
import { forgotPasswordSchema, type ForgotPasswordFormValues } from "./schemas";

export function ForgotPasswordPage() {
  const { control, handleSubmit } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" },
  });

  const mutation = useMutation({
    mutationFn: authApi.forgotPassword,
  });

  const onSubmit = (values: ForgotPasswordFormValues) => mutation.mutate(values);

  return (
    <AuthLayout
      title="Forgot your password?"
      subtitle="Enter your email and we'll send you a reset link"
    >
      {mutation.isSuccess ? (
        <Alert severity="success">
          If an account exists for that email, a password reset link has been sent.
        </Alert>
      ) : (
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            {mutation.error && <Alert severity="error">{mutation.error.message}</Alert>}
            <FormTextField name="email" control={control} label="Email" type="email" autoComplete="email" />
            <Button
              type="submit"
              variant="contained"
              size="large"
              disabled={mutation.isPending}
              startIcon={mutation.isPending ? <CircularProgress size={18} /> : undefined}
            >
              Send reset link
            </Button>
          </Stack>
        </Box>
      )}
      <Typography variant="body2" textAlign="center" color="text.secondary" sx={{ mt: 2 }}>
        <Typography
          component={RouterLink}
          to="/login"
          variant="body2"
          sx={{ textDecoration: "none", color: "primary.main", fontWeight: 600 }}
        >
          Back to sign in
        </Typography>
      </Typography>
    </AuthLayout>
  );
}
