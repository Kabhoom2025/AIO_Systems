---
sidebar_position: 7
---

# Barcode / QR Codes

Label generation/printing for medicines, and real barcode-scanner support at POS.

## What it does

![Print Label dialog](/img/screenshots/07-barcode-label.png)
*The Print Label dialog — a Code128 barcode and QR code generated client-side, ready to print.*

![Scan input on POS](/img/screenshots/07-pos-scan-input.png)
*The auto-focused "Scan barcode…" input on POS — point any USB/Bluetooth scanner at it and it just works.*

- **Auto-generated SKU & barcode** — every medicine gets a unique `SKU-{id:D6}` and a 12-digit numeric barcode the first time it's saved, unless explicit values are supplied (so an org can migrate in real existing barcodes later).
- **Print Label** — a per-medicine dialog renders a Code128 barcode (via `JsBarcode`, to an inline `<svg>`) and a QR code (via the `qrcode` package, to a `<canvas>`) encoding the medicine's barcode value, printable via the browser's print dialog.
- **Scanner integration at POS** — a dedicated, auto-focused "Scan barcode…" input at the top of the POS page. USB/Bluetooth barcode scanners are keyboard-wedge devices — they just type the scanned code followed by Enter into whatever input has focus — so this requires no scanner SDK at all, just listening for Enter on that input and calling a barcode-lookup endpoint.

## Key files

- Backend: `Medicine.Sku`/`Medicine.Barcode` fields, `GET medicines/barcode/{code}/org/{orgId}` in `Pharmacy.API/Controllers/MedicineController.cs`.
- Frontend: `client/src/app/features/medicines/` (label dialog), `client/src/app/features/pos/` (scan input).

## A packaging gotcha worth knowing

Both `jsbarcode` and `qrcode` needed special handling in `client/federation.config.js`'s shared-package configuration to work under Angular's Native Federation build. `qrcode`'s default entry point pulls in a Node-only, filesystem-dependent renderer that crashes in the browser if it's shared via the federation runtime import map instead of bundled directly — the fix was adding it to the federation config's skip list so it's bundled inline (where its `browser` field remap is honored) instead of shared.

Scoped out: camera-based barcode scanning (using a phone/webcam to visually decode a barcode) — that needs a computer-vision decoding library and camera-permission flow, a meaningfully separate feature from the physical-scanner-hardware support built here.
