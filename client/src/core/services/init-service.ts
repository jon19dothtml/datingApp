import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { Observable, of } from 'rxjs';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);
  private likeService= inject(LikesService)

  init() {
    const userString = localStorage.getItem('user');
    if (!userString) return of(null);
    const user = JSON.parse(userString); //convertiamo la stringa in un oggetto JSON
    this.accountService.currentUser.set(user); //il signal currentUser non è più null e viene settato con l'utente ottenuto dal localstorage
    this.likeService.getLikeIds(); //ci richiamiamo il metodo che richiama l api che ritornava la lista di tutti i membri a cui il currentuser ha messo like
  
    return of(null); //ritorniamo un observable che emette null per indicare che l'inizializzazione è completa
  }
}
