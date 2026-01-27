import { Component, inject, OnInit, signal } from '@angular/core';
import { Message } from '../../types/message';
import { MessageService } from '../../core/services/message-service';
import { PaginatedResult } from '../../types/paginations';
import { Paginator } from "../../shared/paginator/paginator";
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ConfirmDialog } from '../../shared/confirm-dialog/confirm-dialog';
import { ConfirmDialogService } from '../../core/services/confirm-dialog-service';

@Component({
  selector: 'app-messages',
  imports: [Paginator, RouterLink, DatePipe],
  templateUrl: './messages.html',
  styleUrl: './messages.css',
})
export class Messages implements OnInit {
  private messageService= inject(MessageService)
  private confirmDialog= inject(ConfirmDialogService)
  protected container= 'Inbox'; //ciò che l’utente vuole vedere (Inbox o Outbox)
  protected fetchedContainer= 'Inbox' //ciò che il backend ha già restituito, evita che la ui possa aggiornarsi prima che la risposta arrivi
  protected pageNumber= 1;
  protected pageSize=10;
  protected paginatedMessages= signal<PaginatedResult<Message> | null>(null);

  tabs=[
    {label: 'Inbox', value: 'Inbox'},
    {label: 'Outbox', value: 'Outbox'},
  ]

  ngOnInit(): void {
    this.loadMessages()
  }

  loadMessages(){
    this.messageService.getMessages(this.container, this.pageNumber, this.pageSize).subscribe({
      next: response => {
        this.paginatedMessages.set(response);
        this.fetchedContainer=this.container; // fetchedContainer diventa "Outbox" solo quando i dati dell’Outbox sono davvero arrivati.
      }
    })
  }

  async confirmDelete(event:Event, id:string){
    event.stopPropagation();
    const ok= await this.confirmDialog.confirm('Are you sure you want to delete this message? ')
    if(ok) this.deleteMessage(id)
  }

  deleteMessage(id:string){
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        const current= this.paginatedMessages() //ci facciamo una copia del signal
        if(current?.items){
          this.paginatedMessages.update(prev => {
            if(!prev) return null

            const newItems= prev.items.filter(x=> x.id !== id) || [] // mi mette tutti gli altri items tranne quello che voglio eliminare
            return{
              items: newItems,
              metadata: prev.metadata
            }
          })
        }
      }
    })
  }

  get isInbox(){
    return this.fetchedContainer === 'Inbox'; //“Mostra i dati come Inbox solo se l’ultimo fetch completato era Inbox”. Questo previene il cambio imminente del container quando la chiamata è ancora in atto
  }

  setContainer(container: string){
    this.container= container;
    this.pageNumber=1
    this.loadMessages()
  }

  onPageChange(event: {pageNumber: number, pageSize: number}){
    this.pageSize= event.pageSize
    this.pageNumber= event.pageNumber
    this.loadMessages();
  }
  
}
