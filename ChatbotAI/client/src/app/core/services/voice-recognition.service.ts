import { Injectable, NgZone, signal } from '@angular/core';
import { Subject } from 'rxjs';

interface SpeechRecognitionAlternativeLike {
  transcript: string;
  confidence: number;
}

interface SpeechRecognitionResultLike {
  isFinal: boolean;
  length: number;
  [index: number]: SpeechRecognitionAlternativeLike;
}

interface SpeechRecognitionEventLike {
  resultIndex: number;
  results: { length: number;[index: number]: SpeechRecognitionResultLike };
}

interface SpeechRecognitionErrorEventLike {
  error: string;
  message?: string;
}

interface SpeechRecognitionLike extends EventTarget {
  lang: string;
  continuous: boolean;
  interimResults: boolean;
  maxAlternatives: number;
  start(): void;
  stop(): void;
  abort(): void;
  onstart: ((this: SpeechRecognitionLike, ev: Event) => void) | null;
  onresult: ((this: SpeechRecognitionLike, ev: SpeechRecognitionEventLike) => void) | null;
  onerror: ((this: SpeechRecognitionLike, ev: SpeechRecognitionErrorEventLike) => void) | null;
  onend: ((this: SpeechRecognitionLike, ev: Event) => void) | null;
}

type SpeechRecognitionConstructor = new () => SpeechRecognitionLike;

function resolveSpeechRecognitionCtor(): SpeechRecognitionConstructor | null {
  const globalWindow = window as unknown as {
    SpeechRecognition?: SpeechRecognitionConstructor;
    webkitSpeechRecognition?: SpeechRecognitionConstructor;
  };
  return globalWindow.SpeechRecognition ?? globalWindow.webkitSpeechRecognition ?? null;
}

/**
 * Thin wrapper around the browser Web Speech API's SpeechRecognition. Every browser quirk
 * (missing API, permission denial, no-speech timeout) is caught and surfaced through
 * `error$`/`isSupported` rather than thrown, so the rest of the app never has to guard
 * against an unsupported browser crashing the chat experience.
 */
@Injectable({ providedIn: 'root' })
export class VoiceRecognitionService {
  private recognition: SpeechRecognitionLike | null = null;
  private readonly ctor = resolveSpeechRecognitionCtor();

  readonly isSupported = this.ctor !== null;
  readonly isListening = signal(false);
  readonly interimTranscript = signal('');

  readonly finalResult$ = new Subject<string>();
  readonly error$ = new Subject<string>();

  constructor(private readonly zone: NgZone) {}

  startListening(language = 'en-US'): void {
    if (!this.isSupported) {
      this.error$.next('Voice recognition is not supported in this browser.');
      return;
    }

    if (this.isListening()) {
      return;
    }

    const recognition = new this.ctor!();
    recognition.lang = language;
    recognition.continuous = false;
    recognition.interimResults = true;
    recognition.maxAlternatives = 1;

    recognition.onstart = () => this.zone.run(() => {
      this.isListening.set(true);
      this.interimTranscript.set('');
    });

    recognition.onresult = (event) => this.zone.run(() => {
      let interim = '';
      let finalText = '';
      for (let i = event.resultIndex; i < event.results.length; i++) {
        const result = event.results[i];
        const transcript = result[0]?.transcript ?? '';
        if (result.isFinal) {
          finalText += transcript;
        } else {
          interim += transcript;
        }
      }

      if (finalText.trim()) {
        this.finalResult$.next(finalText.trim());
      }
      this.interimTranscript.set(interim);
    });

    recognition.onerror = (event) => this.zone.run(() => {
      // 'aborted' is expected whenever we call cancelListening()/stopListening() ourselves —
      // not a real problem, so it stays silent. Every other error (including 'no-speech',
      // which previously failed silently and looked exactly like "the mic isn't working")
      // surfaces a message so the user isn't left staring at a mic that just stopped.
      if (event.error === 'aborted') {
        return;
      }
      this.error$.next(this.friendlyErrorMessage(event.error));
    });

    recognition.onend = () => this.zone.run(() => {
      this.isListening.set(false);
      this.interimTranscript.set('');
      this.recognition = null;
    });

    this.recognition = recognition;
    try {
      recognition.start();
    } catch {
      this.isListening.set(false);
      this.error$.next('Unable to start voice recognition. Please try again.');
    }
  }

  stopListening(): void {
    this.recognition?.stop();
  }

  cancelListening(): void {
    this.recognition?.abort();
  }

  toggleListening(language = 'en-US'): void {
    if (this.isListening()) {
      this.stopListening();
    } else {
      this.startListening(language);
    }
  }

  private friendlyErrorMessage(error: string): string {
    switch (error) {
      case 'not-allowed':
      case 'permission-denied':
        return 'Microphone access was denied. Please allow microphone permissions and try again.';
      case 'audio-capture':
        return 'No microphone was found. Please connect a microphone and try again.';
      case 'no-speech':
        return "Didn't catch that — check your microphone is the right input device and isn't muted, then try again.";
      case 'network':
        return 'A network error interrupted voice recognition (this API sends audio to the browser vendor\'s speech service, so it needs internet access).';
      default:
        return 'Voice recognition encountered an error. Please try again.';
    }
  }
}
