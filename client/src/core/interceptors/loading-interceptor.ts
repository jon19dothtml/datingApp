import { HttpEvent, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../services/busy-service';
import { finalize } from 'rxjs/internal/operators/finalize';
import { delay, of, tap } from 'rxjs';

const cache= new Map<string, HttpEvent<unknown>>(); //mappa per memorizzare le risposte in cache

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService= inject(BusyService);

  if(req.method === 'GET'){
    const cachedResponse= cache.get(req.urlWithParams);
    if(cachedResponse){
      return of(cachedResponse) //restituisce la risposta memorizzata in cache come observable in quanto stiamo interagendo con un HttpClient
    }
  }


  busyService.busy(); //incrementa il contatore delle richieste in corso
  
  return next(req).pipe( //la next() continua la catena degli interceptor ed è un observable
    delay(500), //simuliamo un ritardo di 500ms per mostrare il caricamento
    tap(response=> {
      cache.set(req.url, response) //la response è di tipo HttpEvent<unknown> quindi la mappa che stiamo facendo avrà una stringa come chiave e HttpEvent<unknown> come valore
    }),
    finalize(() =>{
      busyService.idle(); //decrementa il contatore delle richieste in corso
    })
  );
};
