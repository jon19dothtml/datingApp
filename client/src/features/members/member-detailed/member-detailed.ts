import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MemberService } from '../../../core/services/member-service';
import { Member } from '../../../types/member';
import { filter } from 'rxjs/internal/operators/filter';
import { AgePipe } from '../../../core/pipe/age-pipe';

@Component({
  selector: 'app-member-detailed',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, AgePipe],  //routerlinkactive si usa per styling dei link
  templateUrl: './member-detailed.html',
  styleUrl: './member-detailed.css',
})
export class MemberDetailed implements OnInit {
  private memberService= inject(MemberService)
  private route= inject(ActivatedRoute) //usiamo la activetedRoute per accedere ai parametri che la route ci passa 
  //per accedere al resolver devo iniettare ActivatedRoute
  private router= inject(Router) 
  // protected member$?: Observable<Member>; //usiamo con async pipe nel template
  //ma in questo caso non ci serve piu' perche' usiamo il resolver
  protected member= signal<Member | undefined>(undefined);
  protected title$= signal<string | undefined>('Profile');

    ngOnInit(): void {
    this.route.data.subscribe({ //data e' un observable che contiene i dati risolti dalla route
      next: data=> this.member.set(data['member'])  //assegniamo il membro risolto al signal. data e' un oggetto che contiene tutte le proprieta' risolte, in questo caso solo 'member'
    })
    this.title$.set(this.route.firstChild?.snapshot?.title); //impostiamo il titolo iniziale in base alla route figlia attiva

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe({
      next: ()=> {
        this.title$.set(this.route.firstChild?.snapshot?.title);
      }
    })
  }

  // loadMember(){
  //   const id= this.route.snapshot.paramMap.get('id');
  //   if(!id) return;
  //   return this.memberService.getMember(id)
  // }
}
