export const environment = {
  production: true,
  // Routed through ApiGateway (YARP), same convention as every other AIO_Systems client —
  // ApiGateway/appsettings.json's "linkshield-route" strips "/api/linkshield" and forwards
  // the rest as "/api/{...}" to LinkShield.API, so every service here appends "/v1/..."
  // rather than "/api/v1/...".
  apiBaseUrl: 'http://localhost:5000/api/linkshield',
  scanHubUrl: 'http://localhost:5000/hubs/linkshield/scan-progress'
};
