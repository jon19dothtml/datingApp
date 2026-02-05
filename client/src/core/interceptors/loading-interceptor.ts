import { HttpEvent, HttpInterceptorFn, HttpParams } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../services/busy-service';
import { finalize } from 'rxjs/internal/operators/finalize';
import { delay, identity, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';

type CacheEntry = {
  response: HttpEvent<unknown>
  timestamp: number
}

const cache= new Map<string, CacheEntry>(); //mappa per memorizzare le risposte in cache
const CAHCE_DURATION_MS = 5 * 60 * 1000;

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService= inject(BusyService);

  const generateCacheKey = (url: string, params: HttpParams): string =>{
    const paramString= params.keys().map(key=> `${key}=${params.get(key)}`).join('&')     //key rappresenta il nome di ogni singolo paremtro dei filtri es(... .append('pageNumber', ....)....)
    return paramString ? `${url}?${paramString}` : url                                    //andiamo a iterare su di esso con map e aggiugniamo ofni volta &
  }

  const cacheKey= generateCacheKey(req.url, req.params)

  const invalidateCache = (urlPattern: string) => {
    for(const key of cache.keys()){
      if (key.includes(urlPattern)){
        cache.delete(key);
        // console.log(`Cache invalidated for: ${key}`)
      }
    }
  }

  if(req.method.includes('POST') &&req.url.includes('/messages')){
    invalidateCache('/messages') //questo metodo cercherà qualsiasi chiave che è uguale a /messages e la invaliderà
  }

    if(req.method.includes('POST') &&req.url.includes('/likes')){
    invalidateCache('/likes') //questo metodo cercherà qualsiasi chiave che è uguale a /likes e la invaliderà
  }

  if(req.method.includes('POST') && req.url.includes('/logout')){
    cache.clear();
  }
  if(req.method === 'GET'){ 
    const cachedResponse= cache.get(cacheKey); //cacheKey
    if(cachedResponse){
      const isExpired = (Date.now() - cachedResponse.timestamp) > CAHCE_DURATION_MS;
      if(!isExpired){
        return of(cachedResponse.response) //restituisce la risposta memorizzata in cache come observable in quanto stiamo interagendo con un HttpClient
      }else{
        cache.delete(cacheKey);
      }    
    }
  }


  busyService.busy(); //incrementa il contatore delle richieste in corso
  
  return next(req).pipe( //la next() continua la catena degli interceptor ed è un observable
    (environment.production ? identity : delay(500)), //simuliamo un ritardo di 500ms per mostrare il caricamento se non siamo in production
    tap(response=> {
      cache.set(cacheKey, {
        response, 
        timestamp: Date.now()
      }) //la response è di tipo HttpEvent<unknown> quindi la mappa che stiamo facendo avrà una stringa come chiave e HttpEvent<unknown> come valore
    }),
    finalize(() =>{
      busyService.idle(); //decrementa il contatore delle richieste in corso
    })
  );
};
