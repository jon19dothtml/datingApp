import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { Observable, of, tap } from 'rxjs';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private accountService = inject(AccountService);
  private likeService= inject(LikesService)

  init() {
    return this.accountService.refreshToken().pipe( //richiamiamo il metodo che crea il refreshToken
      tap(user=> {
        if(user){
          this.accountService.setCurrentUser(user); //il signal currentUser non è più null e viene settato con l'utente ottenuto dal localstorage
          // this.likeService.getLikeIds(); //ci richiamiamo il metodo che richiama l api che ritornava la lista di tutti i membri a cui il currentuser ha messo like
          this.accountService.startTokenRefreshInterval(); //richiamo il metodo che crea un token ogni 5 min
        }
      })  
    )
    
    //return of(null); //ritorniamo un observable che emette null per indicare che l'inizializzazione è completa
  }
}
