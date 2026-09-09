import { zodResolver } from "@hookform/resolvers/zod";
import { Alert, Box, Button, CircularProgress, Stack, Typography } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { authApi } from "../../api/auth";
import { FormTextField } from "../../components/FormTextField";
import { useAuthStore } from "../../store/authStore";
import { AuthLayout } from "./AuthLayout";
import { registerSchema, type RegisterFormValues } from "./schemas";

export function RegisterPage() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);

  const { control, handleSubmit } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { firstName: "", lastName: "", email: "", password: "", confirmPassword: "" },
  });

  const registerMutation = useMutation({
    mutationFn: (values: RegisterFormValues) =>
      authApi.register({
        email: values.email,
        password: values.password,
        firstName: values.firstName,
        lastName: values.lastName,
      }),
    onSuccess: (data) => {
      login(data.accessToken, data.refreshToken, data.user);
      navigate("/");
    },
  });

  const onSubmit = (values: RegisterFormValues) => registerMutation.mutate(values);

  return (
    <AuthLayout title="Create your account" subtitle="Start managing your projects with your team">
      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Stack spacing={2}>
          {registerMutation.error && (
            <Alert severity="error">{registerMutation.error.message}</Alert>
          )}
          <Stack direction="row" spacing={2}>
            <FormTextField name="firstName" control={control} label="First name" autoComplete="given-name" />
            <FormTextField name="lastName" control={control} label="Last name" autoComplete="family-name" />
          </Stack>
          <FormTextField name="email" control={control} label="Email" type="email" autoComplete="email" />
          <FormTextField
            name="password"
            control={control}
            label="Password"
            type="password"
            autoComplete="new-password"
          />
          <FormTextField
            name="confirmPassword"
            control={control}
            label="Confirm password"
            type="password"
            autoComplete="new-password"
          />
          <Button
            type="submit"
            variant="contained"
            size="large"
            disabled={registerMutation.isPending}
            startIcon={registerMutation.isPending ? <CircularProgress size={18} /> : undefined}
          >
            Create account
          </Button>
          <Typography variant="body2" textAlign="center" color="text.secondary">
            Already have an account?{" "}
            <Typography
              component={RouterLink}
              to="/login"
              variant="body2"
              sx={{ textDecoration: "none", color: "primary.main", fontWeight: 600 }}
            >
              Sign in
            </Typography>
          </Typography>
        </Stack>
      </Box>
    </AuthLayout>
  );
}
