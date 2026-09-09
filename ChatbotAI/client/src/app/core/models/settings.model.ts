export interface UserSettings {
  language: string;
  voice: string;
  speechRate: number;
  pitch: number;
  volume: number;
  autoSpeak: boolean;
  voiceMode: boolean;
  theme: 'light' | 'dark' | 'system';
  /** Personal "bring your own key" AI provider override. Null/empty = use the server default. */
  aiProvider: string | null;
  /** Whether the user has a personal API key stored server-side. The key itself is never sent back. */
  hasCustomAiApiKey: boolean;
  /** Personal model override (e.g. "openai/gpt-oss-120b" on Groq). Null/empty = use a built-in default for the provider. */
  aiModel: string | null;
}

export interface UpdateUserSettingsRequest extends Partial<Omit<UserSettings, 'hasCustomAiApiKey'>> {
  /** Write-only. Omit to leave unchanged, "" to clear the stored key, or a new key to set/replace it. */
  aiApiKey?: string;
}
