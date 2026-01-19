import { Component, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipe/age-pipe';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  //questo componente sarà figlio di member-list e quindi usiamo il 
  // signal input per ricevere il member da visualizzare. In pratica quando il member-list itera sui members
  // per ogni member crea un member-card e gli passa il member corrente come input
  member= input.required<Member>();
  
}
