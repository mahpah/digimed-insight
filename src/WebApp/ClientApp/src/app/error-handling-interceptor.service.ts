import {Observable, throwError} from 'rxjs'
import {catchError} from 'rxjs/operators'
import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http'

export class ErrorHandlingInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError(e => {
        if (e instanceof HttpErrorResponse && e.status === 400) {
          return throwError(e.error)
        }

        return throwError(e)
      })
    )
  }
}
