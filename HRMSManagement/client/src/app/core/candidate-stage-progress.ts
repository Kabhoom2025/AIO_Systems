/// Shared stage-order/labels for the recruitment progress stepper, mirrored on both
/// the HR recruitment view and the public candidate tracking page.
export const CandidateStageProgress = {
  stages: ['Applied', 'Screening', 'Interview', 'Offered', 'Hired'] as const,

  labels: {
    Applied: 'Applied', Screening: 'Screening', Interview: 'Interview',
    Offered: 'Offer Extended', Hired: 'Hired'
  } as Record<string, string>,

  stepperPoints(currentStage: string, historyStages: string[]) {
    const isRejected = currentStage === 'Rejected';
    const reachedIndex = isRejected
      ? Math.max(-1, ...historyStages.map(s => this.stages.indexOf(s as any)))
      : this.stages.indexOf(currentStage as any);

    return this.stages.map((stage, i) => ({
      stage,
      isDone: i < reachedIndex || (i === reachedIndex && !isRejected),
      isCurrent: i === reachedIndex
    }));
  }
};
