import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginCreds, RegisterCreds, User } from '../../types/user';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';
import { PresenceService } from './presence-service';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http= inject(HttpClient);
  currentUser= signal<User | null>(null);
  private likeService= inject(LikesService)
  private presenceService= inject(PresenceService)
  private baseUrl= environment.apiUrl;

  register(creds: RegisterCreds){
    return this.http.post<User>(this.baseUrl + 'account/register', creds, {withCredentials:true}).pipe(
      tap(user => {
        if(user){
          this.setCurrentUser(user);
          this.startTokenRefreshInterval();
        }
      }
      )
    )
  }

  login(creds: LoginCreds){ //siccome stiamo usando httpclient, il metodo ritorna un observable
    return this.http.post<User>(this.baseUrl + 'account/login', creds, {withCredentials:true}).pipe(//withCredentials per usare il cookie dove sta il refreshToken
      tap(user => {
        if(user){
          this.setCurrentUser(user);
          this.startTokenRefreshInterval();
        }
      }
      )
    ); 
  }

  refreshToken(){
    return this.http.post<User>(this.baseUrl + 'account/refresh-token', {},
      {withCredentials:true})
  }
  
  startTokenRefreshInterval(){ //crea il nuovo token ogni tot di tempo
    setInterval(()=>{
      this.http.post<User>(this.baseUrl + 'account/refresh-token', {},
      {withCredentials:true}).subscribe({ //con withCredentials, il cookie verra inviato automaticamente con la richiesta all endpoint
        next: user =>{ //ci da il nuovo token 
          this.setCurrentUser(user)
        },
        error: () => {this.logout()}
      })
    }, 5*60*1000)
  }


  setCurrentUser(user: User){
    user.roles= this.getRolesFromToken(user);
    //localStorage.setItem('user', JSON.stringify(user)); //il metodo setItem salva nel localstorage del browser una chiave e un valore
    this.currentUser.set(user); //in questo caso il nostro http post non sa cosa ritorna, quindi per sapere che si tratta di un user bisogna specificarlo sopra tra <>
    this.likeService.getLikeIds() //ogni volta che ci logghiamo recuperiamo gli id dei membri a cui abbiamo messo like. ripopoliamo il signal
    if(this.presenceService.hubConnection?.state !== HubConnectionState.Connected){
      this.presenceService.createHubConnection(user)
    }
  }

  logout(){
    //localStorage.removeItem('user');
    this.http.post(this.baseUrl + 'account/logout', {}, {withCredentials: true}).subscribe({
      next: ()=> {
        localStorage.removeItem('filters');
        this.currentUser.set(null);
        this.likeService.clearLikeIds(); //puliamo il signal quando facciamo logout
        this.presenceService.stopHubConnection();
      }
    })
    
  }

  private getRolesFromToken(user:User): string []{
    const payload= user.token.split('.')[1] //nel payload troviamo la parte relativa al roles e ogni parte del token è splittata da un punto
    const decoded= atob(payload) //decodifica il payload
    const jsonPayload= JSON.parse(decoded);
    return Array.isArray(jsonPayload.role) ? jsonPayload.role : [jsonPayload.role]; // check se è un role o piu roles
  }
}
