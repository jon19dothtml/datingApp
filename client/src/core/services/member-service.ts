import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member, Photo } from '../../types/member';
import { AccountService } from './account-service';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  protected http= inject(HttpClient)
  // private accountService= inject(AccountService)
  protected baseUrl= environment.apiUrl;

  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + 'members')
  }

  getMember(id: string){
    return this.http.get<Member>(this.baseUrl +  'members/' + id)
  }

  getMemberPhotos(id:string){
    return this.http.get<Photo[]>(this.baseUrl + 'members/' + id + '/photos')
  }

  // private getHttpOptions(){ no longer required since we have an interceptor to handle this
  //   return{
  //     headers: new HttpHeaders({
  //       Authorization: 'Bearer ' + this.accountService.currentUser()?.token
  //     })
  //   }
  // }
}
