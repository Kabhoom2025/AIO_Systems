import { PublicClientApplication, type Configuration } from "@azure/msal-browser";

export const MICROSOFT_CLIENT_ID = import.meta.env.VITE_MICROSOFT_CLIENT_ID ?? "";

const msalConfig: Configuration = {
  auth: {
    clientId: MICROSOFT_CLIENT_ID || "00000000-0000-0000-0000-000000000000",
    authority: "https://login.microsoftonline.com/common",
    redirectUri: typeof window !== "undefined" ? window.location.origin : undefined,
  },
  cache: {
    cacheLocation: "localStorage",
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);
