import { zodResolver } from "@hookform/resolvers/zod";
import { Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle } from "@mui/material";
import { Box } from "@mui/material";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { FormTextField } from "../../components/FormTextField";
import { useCreateBaseline } from "../../hooks/useBaselines";

const schema = z.object({ name: z.string().min(1, "Name is required") });
type FormValues = z.infer<typeof schema>;

interface CreateBaselineDialogProps {
  projectId: string;
  open: boolean;
  onClose: () => void;
}

export function CreateBaselineDialog({ projectId, open, onClose }: CreateBaselineDialogProps) {
  const createMutation = useCreateBaseline(projectId);
  const { control, handleSubmit, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: "" },
  });

  const submit = (values: FormValues) => {
    createMutation.mutate(values, {
      onSuccess: () => {
        reset({ name: "" });
        onClose();
      },
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Create baseline</DialogTitle>
      <Box component="form" onSubmit={handleSubmit(submit)} noValidate>
        <DialogContent>
          <FormTextField name="name" control={control} label="Baseline name" />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button
            type="submit"
            variant="contained"
            disabled={createMutation.isPending}
            startIcon={createMutation.isPending ? <CircularProgress size={16} /> : undefined}
          >
            Save
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}
