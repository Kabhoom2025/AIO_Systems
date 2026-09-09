import { Alert, Box, Button, CircularProgress, Stack } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { authApi } from "../../api/auth";
import { AuthLayout } from "./AuthLayout";

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") ?? "";
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: () => authApi.verifyEmail({ token }),
  });

  useEffect(() => {
    if (token) {
      mutation.mutate();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  return (
    <AuthLayout title="Verify your email" subtitle="Confirming your email address">
      <Stack spacing={2} alignItems="center">
        {!token && <Alert severity="warning">Missing verification token.</Alert>}
        {mutation.isPending && <CircularProgress size={28} />}
        {mutation.isSuccess && (
          <Alert severity="success" sx={{ width: "100%" }}>
            Your email has been verified. You can now sign in.
          </Alert>
        )}
        {mutation.isError && (
          <Alert severity="error" sx={{ width: "100%" }}>
            {mutation.error.message || "Verification failed. The link may have expired."}
          </Alert>
        )}
        <Box sx={{ width: "100%" }}>
          <Button fullWidth variant="contained" onClick={() => navigate("/login")}>
            Go to sign in
          </Button>
        </Box>
      </Stack>
    </AuthLayout>
  );
}
