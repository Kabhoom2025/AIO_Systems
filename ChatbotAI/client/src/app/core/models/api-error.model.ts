export interface ApiErrorResponse {
  success: boolean;
  message: string;
  errorCode: string;
  errors?: Record<string, string[]>;
}
