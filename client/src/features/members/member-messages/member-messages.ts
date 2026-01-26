import { Component, effect, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MessageService } from '../../../core/services/message-service';
import { MemberService } from '../../../core/services/member-service';
import { Message } from '../../../types/message';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../../core/pipe/time-ago-pipe';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  imports: [DatePipe, TimeAgoPipe, FormsModule],
  templateUrl: './member-messages.html',
  styleUrl: './member-messages.css',
})
export class MemberMessages implements OnInit{
  @ViewChild('messageEndRef') messageEndRef! : ElementRef
  private messageService= inject(MessageService)
  private memberService= inject(MemberService)
  protected messages= signal<Message[]>([])
  protected messageContent=''

  constructor(){
    effect(() => {
      const currentMessages= this.messages()
      if(currentMessages.length >0){
        this.scrollToBottom(); //quando la lunghezza cambia si innesca il side effect di questo signal
      }
    })   //un altro tipo di signal che può avere un sideeffect quando il component viene caricato
  }

  ngOnInit(): void {
    this.loadMessages()
  }

  loadMessages(){
    const memberId= this.memberService.member()?.id //qui prendiamo l'id dell'altro membro, quello di cui stiamo vedendo i dettagli
    if(memberId){
      this.messageService.getMessageThread(memberId).subscribe({
        next: messages=> this.messages.set(messages.map(message=> ({ //map ci permette di avere un callBack function su ogni prop di message 
          ...message, //qui settiamo tutte le altre proprietà che riceviamo dall'http
          currentUserSender: message.senderId !== memberId //ma qui settiamo questa prop aggiuntiva che ci dice che se il senderId è diverso 
          // dal memberId(quello del destinatario) allora currentUserSender sarà true cioè siamo riconosciuti come sender. Ci servirà nel template
        })))
      })
    }
  }

  sendMessage(){
    const recipientId= this.memberService.member()?.id;
    if(!recipientId) return;
    this.messageService.sendMessage(recipientId, this.messageContent).subscribe({
      next: message => {
        this.messages.update(messages => { //update del thread di messaggi
          message.currentUserSender =true;
          return [...messages, message]
        })
        this.messageContent=''
      }
    })
  }

  scrollToBottom(){
    setTimeout(() => {
      if(this.messageEndRef){
      this.messageEndRef.nativeElement.scrollIntoView({beahavior: 'smooth'})
    }
    })
    
  }
}
