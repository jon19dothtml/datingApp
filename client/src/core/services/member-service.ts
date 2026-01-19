import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EditableMember, Member, Photo } from '../../types/member';
import { tap } from 'rxjs/internal/operators/tap';


@Injectable({
  providedIn: 'root',
})
export class MemberService {
  protected http= inject(HttpClient)
  // private accountService= inject(AccountService)
  protected baseUrl= environment.apiUrl;
  editMode= signal(false);
  member= signal<Member | null>(null);

  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + 'members')
  }

  getMember(id: string){
    return this.http.get<Member>(this.baseUrl +  'members/' + id).pipe(
      tap(member=> { //usiamo tap per eseguire un effetto collaterale senza modificare il flusso di dati
        this.member.set(member); //salviamo il membro ottenuto nel signal member
      })
    )
  }

  getMemberPhotos(id:string){
    return this.http.get<Photo[]>(this.baseUrl + 'members/' + id + '/photos')
  }

  updateMember(member: EditableMember){
    return this.http.put(this.baseUrl + 'members', member)
  }

  // private getHttpOptions(){ no longer required since we have an interceptor to handle this
  //   return{
  //     headers: new HttpHeaders({
  //       Authorization: 'Bearer ' + this.accountService.currentUser()?.token
  //     })
  //   }
  // }
}
