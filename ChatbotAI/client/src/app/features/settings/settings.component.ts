import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SettingsService } from '../../core/services/settings.service';
import { TextToSpeechService } from '../../core/services/text-to-speech.service';
import { UpdateUserSettingsRequest, UserSettings } from '../../core/models/settings.model';

const LANGUAGES = [
  { code: 'en-US', label: 'English (US)' },
  { code: 'en-GB', label: 'English (UK)' },
  { code: 'es-ES', label: 'Spanish' },
  { code: 'fr-FR', label: 'French' },
  { code: 'de-DE', label: 'German' },
  { code: 'hi-IN', label: 'Hindi' }
];

const AI_PROVIDERS = [
  { value: '', label: 'Use server default' },
  { value: 'OpenAI', label: 'OpenAI (your own key)' },
  { value: 'Grok', label: 'Grok / xAI (your own key)' },
  { value: 'Groq', label: 'Groq (your own key)' }
];

const EXAMPLE_MODEL_BY_PROVIDER: Record<string, string> = {
  OpenAI: 'e.g. gpt-4o-mini',
  Grok: 'e.g. grok-2-latest',
  Groq: 'e.g. openai/gpt-oss-120b'
};

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSliderModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit, OnDestroy {
  private readonly settingsService = inject(SettingsService);
  private readonly textToSpeechService = inject(TextToSpeechService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly subscriptions = new Subscription();

  readonly languages = LANGUAGES;
  readonly aiProviders = AI_PROVIDERS;
  readonly draft = signal<UserSettings>(this.settingsService.settings());
  readonly isSaving = signal(false);
  readonly isTtsSupported = this.textToSpeechService.isSupported;

  /** undefined = user hasn't touched the key field this visit (leave unchanged on save). */
  readonly aiApiKeyDraft = signal<string | undefined>(undefined);

  readonly modelPlaceholder = computed(() =>
    EXAMPLE_MODEL_BY_PROVIDER[this.draft().aiProvider ?? ''] ?? 'Provider-specific model name'
  );

  /** Only the voices installed for the currently selected language — showing every voice on
   *  the system regardless of language made it easy to end up with a mismatched selection
   *  (e.g. an English voice left selected after switching Language to Hindi) with no clue why
   *  responses weren't actually speaking Hindi. */
  readonly voicesForLanguage = computed(() => {
    const prefix = this.draft().language.split('-')[0].toLowerCase();
    return this.textToSpeechService.voices().filter((v) => v.lang.toLowerCase().startsWith(prefix));
  });

  readonly hasNoVoiceForLanguage = computed(
    () => this.isTtsSupported && this.textToSpeechService.voices().length > 0 && this.voicesForLanguage().length === 0
  );

  ngOnInit(): void {
    this.settingsService.load().subscribe((settings) => this.draft.set(settings));

    this.subscriptions.add(
      this.textToSpeechService.speechError$.subscribe((message) =>
        this.snackBar.open(message, 'Dismiss', { duration: 6000 })
      )
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  updateField<K extends keyof UserSettings>(key: K, value: UserSettings[K]): void {
    this.draft.update((current) => ({ ...current, [key]: value }));
  }

  onAiProviderChange(value: string): void {
    this.updateField('aiProvider', value === '' ? null : value);
  }

  onLanguageChange(languageCode: string): void {
    this.draft.update((current) => {
      const prefix = languageCode.split('-')[0].toLowerCase();
      const stillValid =
        current.voice === 'default' ||
        this.textToSpeechService.voices().some((v) => v.name === current.voice && v.lang.toLowerCase().startsWith(prefix));
      return { ...current, language: languageCode, voice: stillValid ? current.voice : 'default' };
    });
  }

  clearApiKey(): void {
    this.aiApiKeyDraft.set('');
  }

  close(): void {
    this.router.navigate(['/chat']);
  }

  previewVoice(): void {
    const settings = this.draft();
    this.textToSpeechService.speak('This is how I will sound when reading responses aloud.', {
      voiceName: settings.voice === 'default' ? undefined : settings.voice,
      rate: settings.speechRate,
      pitch: settings.pitch,
      volume: settings.volume,
      lang: settings.language
    });
  }

  save(): void {
    this.isSaving.set(true);

    const { hasCustomAiApiKey, ...rest } = this.draft();
    const payload: UpdateUserSettingsRequest = { ...rest };
    const apiKeyDraft = this.aiApiKeyDraft();
    if (apiKeyDraft !== undefined) {
      payload.aiApiKey = apiKeyDraft;
    }

    this.settingsService.update(payload).subscribe({
      next: (settings) => {
        this.draft.set(settings);
        this.aiApiKeyDraft.set(undefined);
        this.isSaving.set(false);
        this.snackBar.open('Settings saved', undefined, { duration: 2000 });
      },
      error: () => this.isSaving.set(false)
    });
  }
}
