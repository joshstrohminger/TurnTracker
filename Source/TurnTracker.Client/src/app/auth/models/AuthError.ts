import { HttpErrorResponse } from '@angular/common/http';

export class AuthError extends HttpErrorResponse {
  constructor(message: string) {
    super({
      error: message,
      status: 401,
      statusText: message
    });
  }
}
