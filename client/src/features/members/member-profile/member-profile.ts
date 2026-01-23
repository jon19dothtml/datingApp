import { Component, HostListener, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { EditableMember, Member } from '../../../types/member';
import { DatePipe } from '@angular/common';
import { MemberService } from '../../../core/services/member-service';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastService } from '../../../core/services/toast-service';
import { AccountService } from '../../../core/services/account-service';
import { TimeAgoPipe } from '../../../core/pipe/time-ago-pipe';

@Component({
  selector: 'app-member-profile',
  imports: [DatePipe, FormsModule, TimeAgoPipe],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css',
})
export class MemberProfile implements OnInit, OnDestroy {

  @ViewChild('editForm') editForm?: NgForm //il viewchild ci permette di accedere ai dati del form nel template
  @HostListener('window:beforeunload', ['$event']) notify($event: BeforeUnloadEvent) {
    if (this.editForm?.dirty) {
      $event.preventDefault(); //previene la chiusura della finestra
    }
  } //con HostName ascoltiamo l'evento di chiusura della finestra del browser e se il form e' dirty usiamo preventDefault per mostrare un avviso di conferma

  accountService= inject(AccountService)

  private toast = inject(ToastService)
  protected memberService = inject(MemberService)
  protected editableMember: EditableMember = { //oggetto per il two-way binding con il form, risolviamo il problema dell'undefined inizializzandolo con stringhe vuote
    displayName: '',
    description: '',
    city: '',
    country: ''
  }

  ngOnInit(): void {

    
    this.editableMember = { //avendo inizializzato con undefined all inizio del metodo 
      // qui possiamo inizializzare l'oggetto editableMember con i dati del membro corrente
      displayName: this.memberService.member()?.displayName || '', //prendiamo member dal memberService che quando eseguirà l'onInit avra' gia' caricato il membro presente dalla route resolver
      description: this.memberService.member()?.description || '',
      city: this.memberService.member()?.city || '',
      country: this.memberService.member()?.country || '',
    }
    //abbiamo messo questo metodo nell'OnInit E non nel costruttore perchè in questo modo siamo 
    // sicuri che il membro sia gia' stato caricato dal resolver prima di inizializzare 
    // l'oggetto editableMember
  }



  updateProfile() {
    if (!this.memberService.member()) return;
    const updatedMember = {
      ...this.memberService.member(),//copiamo tutte le proprieta' del membro attuale
      ...this.editableMember //sovrascriviamo con le proprieta' modificate
    }
    this.memberService.updateMember(this.editableMember).subscribe({
      next: () => {
        this.toast.successToast('profile updated successfully');
        this.memberService.editMode.set(false); //uscire dalla modalita' di modifica
        this.memberService.member.set(updatedMember as Member); //aggiorniamo il signal member con i nuovi dati
        this.editForm?.reset(updatedMember); //resettare il form con i nuovi dati
        const currentUser= this.accountService.currentUser() //aggiorniamo anche il displayName nell'accountService se e' stato modificato (riguardante la navbar)
        if( currentUser && updatedMember.displayName !== currentUser?.displayName) { //double check se l'utente e' presente (not undefined) e se il displayName e' stato modificato
          currentUser.displayName = updatedMember.displayName; //allora aggiorniamo il displayName con quello updated
          this.accountService.setCurrentUser(currentUser); //aggiorniamo il currentUser nel accountService
        }
      }
    })
  }

  ngOnDestroy(): void { //quando passo ad un altro componente quello vecchio viene distrutto quindi devo resettare la modalita' di edit
    if (this.memberService.editMode()) {
      this.memberService.editMode.set(false);
    }
  }
}

  //Un modo alternativo per ottenere i dati risolti dal resolver invece che usare il service, 
  // prendiamo i dati dalla ruote che proviene dal componente padre
    // this.route.parent?.data.subscribe({ //il resolver e' definito nella route padre
    //   next: data => this.member.set(data['member']) //impostiamo il signal member con i dati risolti