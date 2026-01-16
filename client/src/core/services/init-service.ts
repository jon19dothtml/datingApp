import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);

  init() {
    const userString = localStorage.getItem('user');
    if (!userString) return of(null);
    const user = JSON.parse(userString); //convertiamo la stringa in un oggetto JSON
    this.accountService.currentUser.set(user); //il signal currentUser non è più null e viene settato con l'utente ottenuto dal localstorage
  
    return of(null); //ritorniamo un observable che emette null per indicare che l'inizializzazione è completa
  }
}
