import GroupsOutlinedIcon from "@mui/icons-material/GroupsOutlined";
import { Avatar, Box, Card, CardContent, Stack, Typography } from "@mui/material";
import { useParams } from "react-router-dom";
import { SkeletonCard } from "../../components/Skeletons";
import { useResourceAllocationAi } from "../../hooks/useResourceAllocationAi";
import { AiUnavailableState } from "./AiUnavailableState";

export function ResourceAllocationPage() {
  const { projectId } = useParams<{ projectId: string }>();
  const { data, isLoading, error } = useResourceAllocationAi(projectId);

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={700}>
          AI Resource Allocation
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Recommendations on how to balance work across the team based on current load.
        </Typography>
      </Box>

      {isLoading ? (
        <SkeletonCard count={3} />
      ) : error ? (
        <AiUnavailableState error={error} />
      ) : data ? (
        <Stack spacing={2}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="subtitle1" fontWeight={700} gutterBottom>
                Reasoning
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: "pre-wrap" }}>
                {data.reasoning}
              </Typography>
            </CardContent>
          </Card>

          <Stack spacing={1.5}>
            {data.suggestions.map((s) => (
              <Card key={s.userId} variant="outlined">
                <CardContent>
                  <Stack direction="row" spacing={2} alignItems="flex-start">
                    <Avatar sx={{ bgcolor: "primary.main" }}>{s.userName.slice(0, 2).toUpperCase()}</Avatar>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="subtitle2" fontWeight={700}>
                        {s.userName}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {s.recommendation}
                      </Typography>
                    </Box>
                  </Stack>
                </CardContent>
              </Card>
            ))}
          </Stack>
        </Stack>
      ) : (
        <AiUnavailableState title="No recommendations yet" />
      )}

      {!isLoading && !error && data && data.suggestions.length === 0 && (
        <Card variant="outlined">
          <CardContent>
            <Stack alignItems="center" spacing={1} sx={{ py: 3 }}>
              <GroupsOutlinedIcon sx={{ fontSize: 40 }} color="disabled" />
              <Typography color="text.secondary">No allocation suggestions available.</Typography>
            </Stack>
          </CardContent>
        </Card>
      )}
    </Stack>
  );
}
