# Tablet Frontend (React + TypeScript + Vite)

This app is a kiosk-style tablet UI for browsing a restaurant menu, placing orders, and tracking status. It integrates with the .NET backend via REST and SignalR, supports offline caching with IndexedDB, and queues orders for retry when offline.

## Scripts
- `npm run dev`: Start Vite dev server (proxying `/api` and `/orderHub` to backend)
- `npm run build`: Type-check and build
- `npm run preview`: Preview production build

## Routing
Routes are defined in `src/App.tsx` using `react-router-dom`:
- `/` → `HomeScreen` (categories)
- `/menu/:category` → `MenuListScreen`
- `/menu/item/:id` → `ItemDetailScreen`
- `/cart` → `CartScreen`
- `/orders` → `OrderStatusScreen`

## Backend integration
- REST base URL: proxied via Vite to `/api`.
- Menu: `GET /api/menu`, `GET /api/menu/categories`, `GET /api/menu/:id`
- Orders: `POST /api/orders` (queued offline when needed)
- SignalR: `/orderHub` for real-time order status updates

See `src/services/apiService.ts`, `src/services/offlineQueue.ts`, and `src/services/signalRService.ts`.

## Offline support
- Menu cached in IndexedDB via Dexie (`src/services/database.ts`).
- Fallback to cache when offline.
- Orders are queued in `localStorage` and retried when connectivity is restored (`src/services/offlineQueue.ts`).

## Kiosk mode styling
General recommendations for large-touch targets and kiosk usage:
- Use large buttons (min 44px height), high contrast colors, big fonts (16–20px+).
- Disable text selection, overscroll, and pull-to-refresh where possible.
- Keep important actions fixed and reachable; provide clear back/confirm buttons.
- Prefer full-bleed screens with sticky headers for navigation.

## Android kiosk (lock app)
For a dedicated Android tablet, use one of these approaches:
1) Screen pinning (no MDM):
   - Enable Screen pinning in Settings → Security.
   - Open the app, go to Overview, pin it. Optionally require PIN to unpin.
2) Lock task mode with Device Owner (ADB, no MDM):
   - Factory reset or use a fresh device.
   - Set your app as device owner via ADB (`dpm set-device-owner ...`).
   - Configure lock task mode so the app runs pinned automatically.
3) Use an MDM (e.g., Android Enterprise/Managed Google Play, Hexnode, Kandji, etc.) to provision the app in kiosk mode, auto-launch, block status bar, disable system keys, control Wi‑Fi, and schedule updates.

Tips:
- Turn on Guided Access equivalents from OEM/MDM to block status bar and home buttons.
- Keep device awake while charging and set app to auto-launch on boot if supported.

## Dev proxy
The Vite config proxies:
- `/api` → `http://localhost:5000`
- `/orderHub` (WebSocket) → `http://localhost:5000`

Adjust targets as needed in `vite.config.ts`.
