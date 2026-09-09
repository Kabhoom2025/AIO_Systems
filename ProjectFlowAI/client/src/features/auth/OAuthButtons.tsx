import GoogleIcon from "@mui/icons-material/Google";
import MicrosoftIcon from "@mui/icons-material/Window";
import { Button, Stack, Tooltip } from "@mui/material";
import { GoogleLogin, GoogleOAuthProvider } from "@react-oauth/google";
import { useMsal } from "@azure/msal-react";
import { MICROSOFT_CLIENT_ID } from "../../app/msalConfig";

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "";

interface OAuthButtonsProps {
  onGoogleIdToken: (idToken: string) => void;
  onMicrosoftAccessToken: (accessToken: string) => void;
  disabled?: boolean;
}

function MicrosoftButton({
  onMicrosoftAccessToken,
  disabled,
}: {
  onMicrosoftAccessToken: (accessToken: string) => void;
  disabled?: boolean;
}) {
  const { instance } = useMsal();

  const handleClick = async () => {
    try {
      const result = await instance.loginPopup({
        scopes: ["User.Read"],
      });
      if (result.accessToken) {
        onMicrosoftAccessToken(result.accessToken);
      }
    } catch {
      // Popup closed or failed; user can retry.
    }
  };

  return (
    <Button
      fullWidth
      variant="outlined"
      startIcon={<MicrosoftIcon />}
      onClick={handleClick}
      disabled={disabled}
    >
      Sign in with Microsoft
    </Button>
  );
}

export function OAuthButtons({
  onGoogleIdToken,
  onMicrosoftAccessToken,
  disabled,
}: OAuthButtonsProps) {
  return (
    <Stack spacing={1.5}>
      {GOOGLE_CLIENT_ID ? (
        <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
          <Stack alignItems="stretch">
            <GoogleLogin
              width="100%"
              onSuccess={(credentialResponse) => {
                if (credentialResponse.credential) {
                  onGoogleIdToken(credentialResponse.credential);
                }
              }}
              onError={() => {
                // Google reports the failure to the console via its own UI; no-op here.
              }}
            />
          </Stack>
        </GoogleOAuthProvider>
      ) : (
        <Tooltip title="Google OAuth isn't configured yet (missing VITE_GOOGLE_CLIENT_ID)">
          <span>
            <Button fullWidth variant="outlined" startIcon={<GoogleIcon />} disabled>
              Sign in with Google
            </Button>
          </span>
        </Tooltip>
      )}

      {MICROSOFT_CLIENT_ID ? (
        <MicrosoftButton onMicrosoftAccessToken={onMicrosoftAccessToken} disabled={disabled} />
      ) : (
        <Tooltip title="Microsoft OAuth isn't configured yet (missing VITE_MICROSOFT_CLIENT_ID)">
          <span>
            <Button fullWidth variant="outlined" startIcon={<MicrosoftIcon />} disabled>
              Sign in with Microsoft
            </Button>
          </span>
        </Tooltip>
      )}
    </Stack>
  );
}
