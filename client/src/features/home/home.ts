import { Component, inject, Input, signal } from '@angular/core';
import { Register } from "../account/register/register";
import { User } from '../../types/user';
import { AccountService } from '../../core/services/account-service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-home',
  imports: [Register, TranslatePipe],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  // @Input({required: true}) membersFromApp: User[]= []; //riceve i dati dall'app component
  protected registerMode= signal(false)
  protected accountService= inject(AccountService)
  protected showcaseMessages = [
    {
      author: 'Giulia',
      initials: 'GF',
      time: '09:30',
      content: 'Campo prenotato per sabato alle 10. Porto anche le palline nuove 🎾',
      currentUser: false,
    },
    {
      author: 'Tu',
      initials: 'MR',
      time: '09:32',
      content: 'Perfetto, ci sono. Dopo la partita andiamo a fare brunch vicino al club?',
      currentUser: true,
    },
    {
      author: 'Giulia',
      initials: 'GF',
      time: '09:34',
      content: 'Assolutamente sì. Ti mando anche la posizione del circolo 🌿',
      currentUser: false,
    },
  ];
  protected featuredPlayer = {
    name: 'Sofia Ricci',
    age: 29,
    city: 'Napoli',
    country: 'Italy',
    level: 'Intermediate+',
    availability: 'Evenings & weekends',
    description: 'Topspin da fondo campo, tanta energia e voglia di organizzare match reali senza chat infinite.',
  };

  showRegister(value: boolean) {
    this.registerMode.set(value)
  }
}
