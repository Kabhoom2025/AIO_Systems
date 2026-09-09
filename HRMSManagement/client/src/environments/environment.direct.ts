// Local-only dev config that talks straight to HRMS.API, bypassing the ApiGateway.
// Use `npm run start:direct` — only `HRMS.API` needs to be running, not the gateway
// or any other backend. Do not use this for anything that needs cross-service routing
// (e.g. testing from the Super Admin/Restaurant clients) — those still need the gateway.
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5003/api',
  signalrUrl: 'http://localhost:5003',
  tokenKey: 'hrms_token',
  refreshTokenKey: 'hrms_refresh_token',
  userKey: 'hrms_user'
};
