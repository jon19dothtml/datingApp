import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BusyService {
  busyRequestCount = signal(0); //numero di richieste in corso

  busy(){
    this.busyRequestCount.update(current=> current + 1);
  }

  idle(){ //chiamato quando una richiesta è terminata
    this.busyRequestCount.update(current => Math.max(0, current-1)) //controlliamo che non scenda sotto zero
  }
}
