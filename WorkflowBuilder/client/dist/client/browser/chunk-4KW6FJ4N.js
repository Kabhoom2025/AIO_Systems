import {
  HttpClient,
  environment
} from "./chunk-MGCDEZWU.js";
import {
  Injectable,
  setClassMetadata,
  tap,
  ɵɵdefineInjectable,
  ɵɵinject
} from "./chunk-RUVEZ3RD.js";

// src/app/core/auth.service.ts
var AuthService = class _AuthService {
  http;
  constructor(http) {
    this.http = http;
  }
  login(email, password) {
    return this.http.post(`${environment.apiUrl}/auth/login`, { email, password }).pipe(tap((res) => this.persistSession(res)));
  }
  logout() {
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.userKey);
  }
  persistSession(res) {
    localStorage.setItem(environment.tokenKey, res.token);
    localStorage.setItem(environment.userKey, JSON.stringify({ name: res.name, email: res.email, role: res.role }));
  }
  static \u0275fac = function AuthService_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _AuthService)(\u0275\u0275inject(HttpClient));
  };
  static \u0275prov = /* @__PURE__ */ \u0275\u0275defineInjectable({ token: _AuthService, factory: _AuthService.\u0275fac, providedIn: "root" });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(AuthService, [{
    type: Injectable,
    args: [{ providedIn: "root" }]
  }], () => [{ type: HttpClient }], null);
})();

export {
  AuthService
};
//# sourceMappingURL=chunk-4KW6FJ4N.js.map
