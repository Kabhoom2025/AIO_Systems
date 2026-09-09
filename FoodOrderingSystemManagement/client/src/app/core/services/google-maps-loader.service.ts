import { Injectable } from '@angular/core';

declare global {
  interface Window { google: any; initMap: () => void; }
}

@Injectable({ providedIn: 'root' })
export class GoogleMapsLoaderService {
  private loaded = false;
  private pending: Promise<void> | null = null;

  /**
   * Dynamically loads the Maps JS SDK using the standard callback pattern:
   *   https://maps.googleapis.com/maps/api/js?key=…&libraries=places&callback=initMap
   * Safe to call multiple times — only one <script> tag is ever injected.
   */
  load(apiKey: string): Promise<void> {
    if (this.loaded && window.google?.maps) return Promise.resolve();
    if (this.pending) return this.pending;

    this.pending = new Promise<void>((resolve, reject) => {
      // If a previous page-load already injected the tag, just wait for the object
      if (document.getElementById('gmaps-sdk')) {
        const poll = setInterval(() => {
          if (window.google?.maps) { clearInterval(poll); this.loaded = true; resolve(); }
        }, 100);
        return;
      }

      // Google calls this global function once the SDK is ready
      window.initMap = () => { this.loaded = true; resolve(); };

      const script = document.createElement('script');
      script.id    = 'gmaps-sdk';
      script.src   = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places&callback=initMap`;
      script.async = true;
      script.defer = true;
      script.onerror = () => reject(new Error('Google Maps failed to load. Verify the API key and that Maps JS API is enabled.'));
      document.head.appendChild(script);
    });

    return this.pending;
  }

  get isLoaded(): boolean { return this.loaded && !!window.google?.maps; }
}
