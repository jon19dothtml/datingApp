import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipe/age-pipe';
import { LikesService } from '../../../core/services/likes-service';

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
  private likeService= inject(LikesService);
  protected hasLiked= computed(()=> this.likeService.likeIds().includes(this.member().id)) // con questo computed signal calcoliamo se la card che viene mostrata è piaciuta al currentUser

  toggleLike(event:Event){
    event.stopPropagation(); //evita la propagazione delle route verso il profilo del member
    this.likeService.toggleLike(this.member().id).subscribe({
      next: () => {
        if(this.hasLiked()){
          this.likeService.likeIds.update(ids=> ids.filter(x=> x!== this.member().id)) //rimuoviamo il like

        }else{
          this.likeService.likeIds.update(ids=> [...ids, this.member().id])
        }
      }
    })
  }
}
