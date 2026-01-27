import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PaginatedResult } from '../../types/paginations';
import { Message } from '../../types/message';
import { AccountService } from './account-service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private baseUrl= environment.apiUrl;
  private http= inject(HttpClient)
  private accountService= inject(AccountService)
  private hubUrl= environment.hubUrl
  private hubConnection?: HubConnection
  messageThread= signal<Message[]>([]);

  createHubConnection(otherUserId: string){
    const currentUser= this.accountService.currentUser();
    if(!currentUser) return;
    this.hubConnection= new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'messages?userId=' + otherUserId, {
        accessTokenFactory: () => currentUser.token
      })
      .withAutomaticReconnect()
      .build()

    this.hubConnection.start().catch(error => console.log(error))

    this.hubConnection.on('RecieveMessageThread', (messages: Message[]) => {
      this.messageThread.set(messages.map(message=> ({ //map ci permette di avere un callBack function su ogni prop di message 
          ...message, //qui settiamo tutte le altre proprietà che riceviamo dall'http
          currentUserSender: message.senderId !== otherUserId //ma qui settiamo questa prop aggiuntiva che ci dice che se il senderId è diverso 
          // dal memberId(quello del destinatario) allora currentUserSender sarà true cioè siamo riconosciuti come sender. Ci servirà nel template
        })))
    });

    this.hubConnection.on('NewMessage', (message: Message) =>{ //settimao l'arrivo di un nuovo mess
      message.currentUserSender = message.senderId=== currentUser.id //verifichiamo se il sender è l'utente loggato
      this.messageThread.update(messages => [...messages, message])
    })
  }

  stopHubConnection(){
    if(this.hubConnection?.state === HubConnectionState.Connected){
      this.hubConnection.stop().catch(error => console.log(error))
    }
  }

  getMessages(container: string, pageNumber: number, pageSize: number){
    let params= new HttpParams();
    params= params.append('pageNumber', pageNumber)
    params= params.append('pageSize', pageSize)
    params= params.append('container', container)
    return this.http.get<PaginatedResult<Message>>(this.baseUrl + 'messages' , {params})
  }

  getMessageThread(memberId: string){
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + memberId)
  }

  sendMessage(recipientId: string, content: string){
    // return this.http.post<Message>(this.baseUrl + 'messages' , {recipientId, content})
    return this.hubConnection?.invoke('SendMessage' , {recipientId, content})
  }
  deleteMessage(id: string){
    return this.http.delete(this.baseUrl + 'messages/' + id)
  }
}
