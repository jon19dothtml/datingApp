import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal} from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Nav } from '../layout/nav/nav';
import { AccountService } from '../core/services/account-service';
import { Home } from "../features/home/home";
import { User } from '../types/user';


@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit{
  private http= inject(HttpClient);
  private accountService= inject(AccountService);
  protected readonly title = 'Dating App';
  protected members= signal<User []>([]);

  async ngOnInit() { //rappresenta ilciclo di vita del componente
    //ovvero tutto cio che deve essere eseguito quando il componente viene inizializzato
    //in questo caso vogliamo fare una chiamata http per ottenere gli utenti
    //per richiamare un componenente http dobbiamo iniettarlo nel costruttore 
    //e per usare una proprietà dobbiamo usare il this.
    //questa http ci ritorna un observable quindi dobbiamo iscriverci a questo observable
      // this.http.get('https://localhost:5001/api/members').subscribe({ //subscribe per gestire la risposta asincrona
      //   next: response => this.members.set(response), //il subscribe gestisce tre casi: next, error, complete
      //   error: error =>console.log(error),
      //   complete: () =>console.log('Request completed') //quando completiamo una richiesta si disiscrive automaticamente
      // })
      this.members.set(await this.getMembers());
      this.setCurrentUser();
    }

    setCurrentUser(){
      const userString= localStorage.getItem('user');
      if(!userString) return;
      const user= JSON.parse(userString); //convertiamo la stringa in un oggetto JSON
      this.accountService.currentUser.set(user); //il signal currentUser non è più null e viene settato con l'utente ottenuto dal localstorage
    }

    async getMembers(){
      try{
        return lastValueFrom(this.http.get<User[]>('https://localhost:5001/api/members'))
      }catch(error){
        console.log(error);
        throw error;
      }
    }
    //invece di usare subscribe possiamo usare lastValueFrom che converte un observable in una promise
    //una promise rappresenta un valore che potrebbe essere disponibile ora, in futuro o mai e racchiude una subscription.
    //si usa l'await per aspettare che la promise venga risolta e ritorna il valore risolto della promise
}
  