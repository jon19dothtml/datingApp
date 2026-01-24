import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http= inject(HttpClient);
  currentUser= signal<User | null>(null);
  private likeService= inject(LikesService)
  private baseUrl= environment.apiUrl;

  register(creds: RegisterCreds){
    return this.http.post<User>(this.baseUrl + 'account/register', creds).pipe(
      tap(user => {
        if(user){
          this.setCurrentUser(user);
        }
      }
      )
    )
  }

  login(creds: LoginCreds){ //siccome stiamo usando httpclient, il metodo ritorna un observable
    return this.http.post<User>(this.baseUrl + 'account/login', creds).pipe(
      tap(user => {
        if(user){
          this.setCurrentUser(user);
        }
      }
      )
    ); 
  }

  setCurrentUser(user: User){
    localStorage.setItem('user', JSON.stringify(user)); //il metodo setItem salva nel localstorage del browser una chiave e un valore
    this.currentUser.set(user); //in questo caso il nostro http post non sa cosa ritorna, quindi per sapere che si tratta di un user bisogna specificarlo sopra tra <>
    this.likeService.getLikeIds() //ogni volta che ci logghiamo recuperiamo gli id dei membri a cui abbiamo messo like. ripopoliamo il signal
  }

  logout(){
    localStorage.removeItem('user');
    localStorage.removeItem('filters');
    this.currentUser.set(null);
    this.likeService.clearLikeIds(); //puliamo il signal quando facciamo logout
  }
}
