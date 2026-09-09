export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',        // API Gateway — routes to AIO_Systems (5284, auth/orgs/modules), restaurant (5275), or pharmacy (5002)
  apiUrlDirect: 'https://localhost:7063/api', // direct restaurant API fallback (dev only)
  docsUrl: 'http://localhost:3000',           // Docusaurus user guide (run `npm start` in docs-site)
  appName: 'FoodOrder POS',
  tokenKey: 'food_order_token',
  userKey: 'food_order_user',
};
