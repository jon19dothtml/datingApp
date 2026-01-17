import { HttpInterceptorFn } from '@angular/common/http';
import { catchError } from 'rxjs/internal/operators/catchError';
import { ToastService } from '../services/toast-service';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast= inject(ToastService);
  const router= inject(Router);
  return next(req).pipe(
    catchError((error) => {
      if (error){
        switch (error.status){
          case 400:
            if(error.error.errors){ //qui andiamo a prendere i dettagli della proprietà error che possiamo vedere nella console
              const modelStateError = [];
              for(const key in error.error.errors){
                if(error.error.errors[key]){
                  modelStateError.push(error.error.errors[key]);
                }
              }
              // throw modelStateError.flat(); 
              //il metodo flat permette di concatenare più sub-array in un unico array
              //[Email; [errEmail1, errEmail2], Pass; [errPass1, errPass2], displayName[errDsNm1]]  ==>
              //[errEmail1, errEmail2, errPass1, errPass2, errDsNm1] e solo in questo modo lo posso stampare
            }
            else{
              toast.errorToast(error.error, error.status)
            }
            break;
          case 401:
            toast.errorToast('Unauthorized');
            break;
          case 404:
            toast.errorToast('Not Found')
            router.navigateByUrl('/not-found')
            break;
          case 500:
            const navigationExtras: NavigationExtras = {state: {error: error.error}}
            router.navigateByUrl('/server-errors', navigationExtras)
            break;
          default:
            toast.errorToast('Something went wrong');
            break;
        }
      }

      throw error;
    })
  );
};
