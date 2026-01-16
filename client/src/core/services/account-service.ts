import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http= inject(HttpClient);
  currentUser= signal<User | null>(null);

  baseUrl = "https://localhost:5001/api/";

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
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}
