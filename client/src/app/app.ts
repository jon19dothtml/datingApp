import { Component, inject } from '@angular/core';
import { Nav } from '../layout/nav/nav';
import { Router, RouterOutlet } from '@angular/router';




@Component({
  selector: 'app-root',
  imports: [Nav, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected router = inject(Router);


  // protected readonly title = 'Dating App';
  // protected members= signal<User []>([]);

  // async ngOnInit() { //rappresenta ilciclo di vita del componente
  //   //ovvero tutto cio che deve essere eseguito quando il componente viene inizializzato
  //   //in questo caso vogliamo fare una chiamata http per ottenere gli utenti
  //   //per richiamare un componenente http dobbiamo iniettarlo nel costruttore 
  //   //e per usare una proprietà dobbiamo usare il this.
  //   //questa http ci ritorna un observable quindi dobbiamo iscriverci a questo observable
  //     // this.http.get('https://localhost:5001/api/members').subscribe({ //subscribe per gestire la risposta asincrona
  //     //   next: response => this.members.set(response), //il subscribe gestisce tre casi: next, error, complete
  //     //   error: error =>console.log(error),
  //     //   complete: () =>console.log('Request completed') //quando completiamo una richiesta si disiscrive automaticamente
  //     // })

  //   }

  // async getMembers(){
  //   try{
  //     return lastValueFrom(this.http.get<User[]>('https://localhost:5001/api/members'))
  //   }catch(error){
  //     console.log(error);
  //     throw error;
  //   }
  // }
  //invece di usare subscribe possiamo usare lastValueFrom che converte un observable in una promise
  //una promise rappresenta un valore che potrebbe essere disponibile ora, in futuro o mai e racchiude una subscription.
  //si usa l'await per aspettare che la promise venga risolta e ritorna il valore risolto della promise
}
