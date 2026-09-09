---
sidebar_position: 14
---

# Real-Time Push (SignalR)

Closes a gap explicitly deferred from the Notifications phase: alerts now reach the client the moment they're generated, instead of only being discovered on the next poll.

## What it does

![Notification bell dropdown](/img/screenshots/08-notifications.png)
*This is the same notification bell UI from the Notifications module — SignalR is what updates it instantly, without the page polling or reloading.*

- **Background sweep** — a `BackgroundService` (`NotificationSweepService`) scans every active organization every 60 seconds, proactively generating any new low-stock/expiring-batch alerts — closing the other Notifications-phase gap, where generation only happened lazily when a client asked for the list.
- **SignalR hub** — `NotificationsHub` groups each connection by organization (read from the same `organizationId` JWT claim every other endpoint uses), so a push reaches only the users in that org.
- **Instant client update** — when the sweep finds new alerts, it pushes them straight to the relevant org's connected clients. The Angular shell updates the bell badge immediately and shows a toast per new alert — a real-time push a user actually sees happen, not just a silently-updated number.
- **Resilient fallback** — the old 30-second poll is kept, just slowed to every 5 minutes, purely as a safety net in case a client's SignalR connection drops and hasn't reconnected yet.

## Key files

- Backend: `Pharmacy.API/Hubs/NotificationsHub.cs`, `Pharmacy.API/BackgroundServices/NotificationSweepService.cs`, a JWT bearer `Events.OnMessageReceived` extension in `Pharmacy.API/Program.cs` that accepts the token via query string for the hub path only (every other endpoint still uses the standard header).
- Frontend: `client/src/app/core/notifications-hub.service.ts`, wired into `client/src/app/pharmacy-shell/`.

## How it was verified

Rather than just checking the connection opened, the feature was proven with an isolation test: the browser session sat idle after login, a medicine's stock was dropped below its reorder level via a direct database change (simulating another user's action the browser had no way to know about on its own), and a toast reading the new low-stock alert appeared 26 seconds later — with zero page reloads and no manual API calls from the page. Since the fallback poll is 5 minutes and the test window was under a minute, the push was the only possible source of that update.

## Scope

Wired into Notifications only, not a generic real-time event bus for every module. The sweep runs on a 60-second timer, not sub-second — appropriate for conditions (stock levels, expiry dates) that don't change that fast.
