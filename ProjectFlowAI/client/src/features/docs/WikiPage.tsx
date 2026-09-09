import { Stack, Typography } from "@mui/material";
import { AppShell } from "../../components/AppShell";
import { useAuthStore } from "../../store/authStore";
import { DocsWorkspace } from "./DocsWorkspace";

export function WikiPage() {
  const user = useAuthStore((s) => s.user);
  const organizationId = user?.organizationId ?? "";

  return (
    <AppShell>
      <Stack spacing={2} sx={{ height: "calc(100vh - 112px)" }}>
        <Typography variant="h5" fontWeight={700}>
          Wiki
        </Typography>
        <DocsWorkspace organizationId={organizationId} scope="OrgWiki" showCategoryFilter />
      </Stack>
    </AppShell>
  );
}
