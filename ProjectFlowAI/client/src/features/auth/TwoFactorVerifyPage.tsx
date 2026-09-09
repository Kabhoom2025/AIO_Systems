import { zodResolver } from "@hookform/resolvers/zod";
import { Alert, Box, Button, CircularProgress, Stack } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { useLocation, useNavigate } from "react-router-dom";
import { authApi } from "../../api/auth";
import { FormTextField } from "../../components/FormTextField";
import { useAuthStore } from "../../store/authStore";
import { AuthLayout } from "./AuthLayout";
import { twoFactorVerifySchema, type TwoFactorVerifyFormValues } from "./schemas";

interface LocationState {
  tempToken?: string;
}

export function TwoFactorVerifyPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const login = useAuthStore((s) => s.login);
  const tempToken = (location.state as LocationState | null)?.tempToken;

  const { control, handleSubmit } = useForm<TwoFactorVerifyFormValues>({
    resolver: zodResolver(twoFactorVerifySchema),
    defaultValues: { code: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: TwoFactorVerifyFormValues) =>
      authApi.verifyTwoFactor({ tempToken: tempToken ?? "", code: values.code }),
    onSuccess: (data) => {
      login(data.accessToken, data.refreshToken, data.user);
      navigate("/");
    },
  });

  const onSubmit = (values: TwoFactorVerifyFormValues) => mutation.mutate(values);

  return (
    <AuthLayout
      title="Two-factor verification"
      subtitle="Enter the 6-digit code from your authenticator app"
    >
      {!tempToken && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Missing session context. Please sign in again.
        </Alert>
      )}
      <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Stack spacing={2}>
          {mutation.error && <Alert severity="error">{mutation.error.message}</Alert>}
          <FormTextField
            name="code"
            control={control}
            label="Authentication code"
            textFieldProps={{ inputMode: "numeric" }}
          />
          <Button
            type="submit"
            variant="contained"
            size="large"
            disabled={mutation.isPending || !tempToken}
            startIcon={mutation.isPending ? <CircularProgress size={18} /> : undefined}
          >
            Verify
          </Button>
        </Stack>
      </Box>
    </AuthLayout>
  );
}
