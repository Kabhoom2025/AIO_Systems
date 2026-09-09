---
sidebar_position: 10
title: Printer Setup
---

# Printer Setup

FoodOrder POS can print receipts and kitchen tickets directly to a USB, network, or Zebra label printer — with **no print preview dialog** — once a small local helper app called **QZ Tray** is installed on the till/POS machine.

## Why QZ Tray?

Web browsers can't talk to USB printers or print silently on their own for security reasons. QZ Tray runs in the background on the till PC and gives the browser a safe, permitted way to do both.

## One-time setup (per POS machine)

1. Download and install [QZ Tray](https://qz.io) on the computer connected to the printer.
2. Launch QZ Tray — it runs quietly in the system tray.
3. Open FoodOrder POS. Look at the **printer icon** in the top header bar, next to the theme and notification icons:
   - **Grey** (`print_disabled`) — QZ Tray isn't running, or the app can't reach it.
   - **Amber** — QZ Tray is connected, but no printer has been assigned yet.
   - **Green** — connected and ready to print.
4. Click the printer icon to open the printer panel. It lists every printer QZ Tray can see on this machine.
5. For each printer, choose **Receipt** (for bills and kitchen tickets) or **Label** (for Zebra barcode labels). A confirmation message appears once assigned.

Printer assignment is saved **per machine** (in the browser), not shared across the organization — each till can have its own printer.

## Screenshot

![Printer status icon and dropdown](/img/screenshots/printer-setup.png)

## Printing

Once a printer is assigned, the existing Print buttons throughout the app — receipts, bills, kitchen tickets, and barcode labels — send the job straight to the printer with no dialog. If QZ Tray isn't connected or no printer is assigned, the app automatically falls back to the browser's normal print dialog, so nothing is ever blocked.

## Troubleshooting

- **Icon stays grey** — confirm QZ Tray is actually running (check the system tray), then reload the page.
- **QZ Tray shows a security prompt** — the first time the app connects, QZ Tray may ask you to allow the connection. Approve it (and tick "remember" to avoid seeing it every session).
- **Printer not in the list** — make sure the printer is installed and shows up as a printer in Windows (or macOS) first; QZ Tray only sees printers the operating system already knows about.
