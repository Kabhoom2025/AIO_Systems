const { withNativeFederation, shareAll, DEFAULT_SKIP_LIST } = require('@angular-architects/native-federation/config');

// @primeng/themes ships a wildcard export ("./*") for its presets (aura, lara, ...),
// and those presets dynamically import per-component theme files (e.g.
// @primeng/themes/aura/accordion). Native federation's secondary-export discovery only
// enumerates literal export keys, so it can't see those wildcard-resolved deep imports —
// sharing @primeng/themes ends up externalizing imports the runtime import map never
// gets entries for. Keeping the whole package out of `shared` lets esbuild bundle it
// directly instead, which resolves its internal dynamic imports normally.
const skipPrimeNgThemes = (pkg) => pkg === '@primeng/themes' || pkg.startsWith('@primeng/themes/');

const sharedSkipList = [
  ...DEFAULT_SKIP_LIST,
  'rxjs/ajax',
  'rxjs/fetch',
  'rxjs/testing',
  'rxjs/webSocket',
  'primeng/chart',
  'primeicons',
  skipPrimeNgThemes,
  // qrcode's main entry pulls in a Node-only fs-dependent renderer (lib/server.js);
  // its package.json "browser" field remaps this for bundlers, but native federation's
  // shared-package preparation step doesn't apply that remap, so sharing it crashes at
  // runtime with "Dynamic require of fs is not supported". Keeping it out of `shared`
  // lets our own app bundle it directly, where the browser field IS respected.
  'qrcode',
];

// shareAll()'s auto-discovery of a shared package's secondary entry points (e.g.
// primeng's "primeng/chart", "primeng/chart.js"-backed export) is filtered against
// native federation's own internal default skip list, NOT the sharedSkipList passed
// in here — so packages like 'primeng/chart' slip back into `shared` even though
// they're named explicitly above. Strip them out again after the fact so they
// actually get bundled inline instead of left as unresolved runtime externals.
const shared = { ...shareAll({ singleton: true, strictVersion: true, requiredVersion: 'auto' }, sharedSkipList) };
for (const key of Object.keys(shared)) {
  if (sharedSkipList.some(s => (typeof s === 'function' ? s(key) : s === key))) {
    delete shared[key];
  }
}

module.exports = withNativeFederation({

  name: 'pharmacy',

  exposes: {
    './Routes': './src/app/pharmacy.routes.ts',
  },

  shared,

  skip: sharedSkipList

  // Please read our FAQ about sharing libs:
  // https://shorturl.at/jmzH0

});
