export const environment = {
  production: false,
  // All calls go through the API Gateway (localhost:5000), which proxies to
  // AIO_Systems (superadmin-cluster), FoodOrder.API (restaurant-cluster) and
  // Pharmacy.API (pharmacy-cluster). Only the gateway's CORS policy matters
  // for browser requests — the gateway-to-backend hop is server-to-server.
  apiUrl: 'http://localhost:5000/api',
  appName: 'AIO Systems — Super Admin',
  tokenKey: 'aio_admin_token',
  userKey: 'aio_admin_user',
};
