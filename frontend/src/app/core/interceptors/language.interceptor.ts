import { HttpInterceptorFn } from '@angular/common/http';

export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = localStorage.getItem('lang') || 'en';
  const newReq = req.clone({
    headers: req.headers.set('Accept-Language', lang)
  });
  return next(newReq);
};
