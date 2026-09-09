import { Injectable, NgZone, signal } from '@angular/core';
import { Subject } from 'rxjs';

/**
 * Thin wrapper around the browser Web Speech API's SpeechSynthesis. Falls back gracefully
 * (isSupported = false, speak() becomes a no-op) when the API is unavailable.
 */
@Injectable({ providedIn: 'root' })
export class TextToSpeechService {
  readonly isSupported = typeof window !== 'undefined' && 'speechSynthesis' in window;
  readonly isSpeaking = signal(false);
  readonly isPaused = signal(false);
  readonly voices = signal<SpeechSynthesisVoice[]>([]);

  readonly speechEnded$ = new Subject<void>();
  readonly speechError$ = new Subject<string>();

  private currentUtterance: SpeechSynthesisUtterance | null = null;
  private startTimeoutHandle: ReturnType<typeof setTimeout> | null = null;

  constructor(private readonly zone: NgZone) {
    if (!this.isSupported) {
      return;
    }
    this.loadVoices();
    window.speechSynthesis.onvoiceschanged = () => this.zone.run(() => this.loadVoices());
  }

  speak(text: string, options?: { voiceName?: string; rate?: number; pitch?: number; volume?: number; lang?: string }): void {
    if (!this.isSupported || !text.trim()) {
      return;
    }

    this.stop();

    const requestedLang = options?.lang ?? 'en-US';
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.rate = options?.rate ?? 1;
    utterance.pitch = options?.pitch ?? 1;
    utterance.volume = options?.volume ?? 1;
    utterance.lang = requestedLang;

    const voice = options?.voiceName
      ? this.voices().find((v) => v.name === options.voiceName)
      : undefined;
    if (voice) {
      utterance.voice = voice;
    }

    // If neither a specific voice was picked nor any installed voice matches the requested
    // language, most browsers just silently produce no sound and never fire `onerror` — the
    // worst possible failure mode, since it looks identical to "nothing happened". Tell the
    // user proactively instead of leaving them guessing (mirrors the 'no-speech' fix in
    // VoiceRecognitionService for the same reason).
    const langPrefix = requestedLang.split('-')[0].toLowerCase();
    const hasMatchingVoice = !!voice || this.voices().some((v) => v.lang.toLowerCase().startsWith(langPrefix));
    if (!hasMatchingVoice && this.voices().length > 0) {
      this.speechError$.next(
        `No text-to-speech voice for "${requestedLang}" is installed on this device — the response may not be read aloud correctly. ` +
          'Check your OS speech/language settings to install one.'
      );
    }

    utterance.onstart = () => this.zone.run(() => {
      this.clearStartTimeout();
      this.isSpeaking.set(true);
    });
    utterance.onend = () => this.zone.run(() => {
      this.clearStartTimeout();
      this.isSpeaking.set(false);
      this.isPaused.set(false);
      this.currentUtterance = null;
      this.speechEnded$.next();
    });
    utterance.onerror = (event) => this.zone.run(() => {
      this.clearStartTimeout();
      this.isSpeaking.set(false);
      this.isPaused.set(false);
      this.currentUtterance = null;
      if (event.error !== 'interrupted' && event.error !== 'canceled') {
        this.speechError$.next('Unable to play the AI response as speech.');
      }
    });

    this.currentUtterance = utterance;
    window.speechSynthesis.speak(utterance);

    // Safety net for the silent-failure case above: if speech genuinely never starts, `onend`/
    // `onerror` may never fire either, leaving isSpeaking stuck at false with no explanation —
    // and, for callers like VoiceConversationComponent that only advance their listen/speak
    // loop on `speechEnded$`, stuck in "speaking" forever. Treat the timeout as an end-of-speech
    // event (so the loop can continue) in addition to reporting the error.
    this.startTimeoutHandle = setTimeout(() => {
      if (this.currentUtterance === utterance && !this.isSpeaking()) {
        window.speechSynthesis.cancel();
        this.currentUtterance = null;
        this.speechError$.next('Unable to play the AI response as speech on this device.');
        this.speechEnded$.next();
      }
    }, 3000);
  }

  private clearStartTimeout(): void {
    if (this.startTimeoutHandle !== null) {
      clearTimeout(this.startTimeoutHandle);
      this.startTimeoutHandle = null;
    }
  }

  pause(): void {
    if (this.isSupported && this.isSpeaking()) {
      window.speechSynthesis.pause();
      this.isPaused.set(true);
    }
  }

  resume(): void {
    if (this.isSupported && this.isPaused()) {
      window.speechSynthesis.resume();
      this.isPaused.set(false);
    }
  }

  stop(): void {
    if (!this.isSupported) {
      return;
    }
    this.clearStartTimeout();
    window.speechSynthesis.cancel();
    this.isSpeaking.set(false);
    this.isPaused.set(false);
    this.currentUtterance = null;
  }

  private loadVoices(): void {
    this.voices.set(window.speechSynthesis.getVoices());
  }
}
