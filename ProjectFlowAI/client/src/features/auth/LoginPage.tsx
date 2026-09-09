import { zodResolver } from "@hookform/resolvers/zod";
import { Alert, Box, Button, CircularProgress, Divider, Stack, Typography } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { authApi } from "../../api/auth";
import { FormTextField } from "../../components/FormTextField";
import { useAuthStore } from "../../store/authStore";
import { AuthLayout } from "./AuthLayout";
import { OAuthButtons } from "./OAuthButtons";
import { loginSchema, type LoginFormValues } from "./schemas";
import type { LoginResponse, RequiresTwoFactorResponse } from "../../types";

function isTwoFactorRequired(data: LoginResponse): data is RequiresTwoFactorResponse {
  return (data as RequiresTwoFactorResponse).requiresTwoFactor === true;
}

export function LoginPage() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);

  const { control, handleSubmit } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const handleAuthResult = (data: LoginResponse) => {
    if (isTwoFactorRequired(data)) {
      navigate("/2fa/verify", { state: { tempToken: data.tempToken } });
      return;
    }
    login(data.accessToken, data.refreshToken, data.user);
    navigate("/");
  };

  const loginMutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: handleAuthResult,
  });

  const googleMutation = useMutation({
    mutationFn: (idToken: string) => authApi.oauthGoogle({ idToken }),
    onSuccess: handleAuthResult,
  });

  const microsoftMutation = useMutation({
    mutationFn: (accessToken: string) => authApi.oauthMicrosoft({ accessToken }),
    onSuccess: handleAuthResult,
  });

  const onSubmit = (values: LoginFormValues) => loginMutation.mutate(values);

  const anyError =
    loginMutation.error?.message ||
    googleMutation.error?.message ||
    microsoftMutation.error?.message;

  return (
    <AuthLayout title="Welcome back" subtitle="Sign in to continue to ProjectFlow AI">
      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Stack spacing={2}>
          {anyError && <Alert severity="error">{anyError}</Alert>}
          <FormTextField
            name="email"
            control={control}
            label="Email"
            type="email"
            autoComplete="email"
          />
          <FormTextField
            name="password"
            control={control}
            label="Password"
            type="password"
            autoComplete="current-password"
          />
          <Box sx={{ textAlign: "right" }}>
            <Typography
              component={RouterLink}
              to="/forgot-password"
              variant="body2"
              sx={{ textDecoration: "none", color: "primary.main" }}
            >
              Forgot password?
            </Typography>
          </Box>
          <Button
            type="submit"
            variant="contained"
            size="large"
            disabled={loginMutation.isPending}
            startIcon={loginMutation.isPending ? <CircularProgress size={18} /> : undefined}
          >
            Sign in
          </Button>

          <Divider>or continue with</Divider>

          <OAuthButtons
            onGoogleIdToken={(idToken) => googleMutation.mutate(idToken)}
            onMicrosoftAccessToken={(accessToken) => microsoftMutation.mutate(accessToken)}
            disabled={googleMutation.isPending || microsoftMutation.isPending}
          />

          <Typography variant="body2" textAlign="center" color="text.secondary">
            Don't have an account?{" "}
            <Typography
              component={RouterLink}
              to="/register"
              variant="body2"
              sx={{ textDecoration: "none", color: "primary.main", fontWeight: 600 }}
            >
              Sign up
            </Typography>
          </Typography>
        </Stack>
      </Box>
    </AuthLayout>
  );
}
