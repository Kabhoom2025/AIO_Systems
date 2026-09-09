import { Alert, Box, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { QRCodeSVG } from "qrcode.react";
import { useNavigate } from "react-router-dom";
import { authApi } from "../../api/auth";
import { AppShell } from "../../components/AppShell";

export function TwoFactorSetupPage() {
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: authApi.enableTwoFactor,
  });

  return (
    <AppShell>
      <Stack spacing={3} sx={{ maxWidth: 480 }}>
        <Typography variant="h5" fontWeight={700}>
          Set up two-factor authentication
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Add an extra layer of security to your account using an authenticator app such as
          Google Authenticator or Microsoft Authenticator.
        </Typography>

        {mutation.error && <Alert severity="error">{mutation.error.message}</Alert>}

        {mutation.data ? (
          <Paper variant="outlined" sx={{ p: 3, textAlign: "center" }}>
            <Box sx={{ display: "inline-block", bgcolor: "#fff", p: 2, borderRadius: 2 }}>
              <QRCodeSVG value={mutation.data.qrCodeUri} size={200} />
            </Box>
            <Typography variant="body2" sx={{ mt: 2, wordBreak: "break-all" }}>
              Secret: <strong>{mutation.data.secret}</strong>
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Scan this QR code with your authenticator app, then verify a code to finish setup.
            </Typography>
            <Box sx={{ mt: 2 }}>
              <Button variant="contained" onClick={() => navigate("/2fa/verify")}>
                Continue to verification
              </Button>
            </Box>
          </Paper>
        ) : (
          <Button
            variant="contained"
            size="large"
            onClick={() => mutation.mutate()}
            disabled={mutation.isPending}
            startIcon={mutation.isPending ? <CircularProgress size={18} /> : undefined}
            sx={{ alignSelf: "flex-start" }}
          >
            Generate QR code
          </Button>
        )}
      </Stack>
    </AppShell>
  );
}
