import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EditableMember, Member, MemberParams, Photo } from '../../types/member';
import { tap } from 'rxjs/internal/operators/tap';
import { PaginatedResult } from '../../types/paginations';


@Injectable({
  providedIn: 'root',
})
export class MemberService {
  protected http= inject(HttpClient)
  // private accountService= inject(AccountService)
  protected baseUrl= environment.apiUrl;
  editMode= signal(false);
  member= signal<Member | null>(null);

  getMembers(memberParams: MemberParams){
    let params= new HttpParams() //questa classe ci permetteranno di inviare qualcosa come parametri della stringa di query alla nostra API;
    params= params.append('pageNumber', memberParams.pageNumber);
    params= params.append('pageSize', memberParams.pageSize)
    params= params.append('minAge', memberParams.minAge);
    params= params.append('maxAge', memberParams.maxAge)
    params= params.append('orderBy', memberParams.orderBy)
    if(memberParams.gender) params= params.append('gender', memberParams.gender)

    return this.http.get<PaginatedResult<Member>>(this.baseUrl + 'members', {params}).pipe(
      tap(() => {
        localStorage.setItem('filters', JSON.stringify(memberParams))
      })
    )
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

  uploadPhoto(file: File){
    const formData= new FormData(); //FormData() è una API web che consente di costruire facilmente 
    // un set di coppie chiave/valore rappresentanti i campi di un modulo e i loro 
    // valori, che possono essere facilmente inviati utilizzando 
    // la richiesta XMLHttpRequest o fetch. In questo caso, lo usiamo per inviare un file.
    formData.append('file', file); // 'file' è il nome del campo che il backend si aspetta
    return this.http.post<Photo>(this.baseUrl + 'members/add-photo', formData) 
  }


  setMainPhoto(photo: Photo){
    return this.http.put<Photo>(this.baseUrl + 'members/set-main-photo/' + photo.id, {} )
  }

  deletePhoto(photoId: number){
    return this.http.delete(this.baseUrl + 'members/delete-photo/' + photoId)
  }

  // private getHttpOptions(){ no longer required since we have an interceptor to handle this
  //   return{
  //     headers: new HttpHeaders({
  //       Authorization: 'Bearer ' + this.accountService.currentUser()?.token
  //     })
  //   }
  // }
}
