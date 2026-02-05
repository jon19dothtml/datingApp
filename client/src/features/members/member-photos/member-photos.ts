import { Component, inject, OnInit, signal } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { Member, Photo } from '../../../types/member';
import { AsyncPipe, NgClass } from '@angular/common';
import { ImageUpload } from "../../../shared/image-upload/image-upload";
import { AccountService } from '../../../core/services/account-service';
import { User } from '../../../types/user';
import { ImageButton } from "../../../shared/image-button/image-button";
import { DeleteButton } from "../../../shared/delete-button/delete-button";

@Component({
  selector: 'app-member-photos',
  imports: [ImageUpload, ImageButton, DeleteButton],
  templateUrl: './member-photos.html',
  styleUrl: './member-photos.css',
})
export class MemberPhotos implements OnInit {
  protected memberService = inject(MemberService)
  protected accountService = inject(AccountService)
  private route = inject(ActivatedRoute)
  protected photos = signal<Photo[]>([])
  protected loading = signal(false); //da non confondere con quello di image-upload che invece è un input

  ngOnInit(): void {
    const memberId = this.route.parent?.snapshot.paramMap.get('id')
    if (memberId) {
      this.memberService.getMemberPhotos(memberId).subscribe({ // ci iscriviamo all'osservabile restituito da getMemberPhotos
        next: photos => this.photos.set(photos) // aggiorniamo il signal photos con le foto ottenute
      })
    }
  }

  onUploadImage(file: File) { //questo metodo viene chiamato quando l'utente carica una nuova immagine
    this.loading.set(true); // impostiamo lo stato di caricamento a true, cioe stiamo caricando l'immagine
    this.memberService.uploadPhoto(file).subscribe({ // ci iscriviamo all'osservabile restituito da uploadPhoto
      next: photo => {
        this.memberService.editMode.set(false); // disabilitiamo la modalità di modifica
        this.loading.set(false); // impostiamo lo stato di caricamento a false, cioe abbiamo finito di caricare l'immagine
        this.photos.update(photos => [...photos, photo]); // aggiungiamo la nuova foto all'array di foto esistente
        if(!this.memberService.member()?.imageUrl && photo.isApproved){ //controllo quando carico una foto se c'è o meno una foto profilo
          this.setMainLocalPhoto(photo) //se non c'è imposto come foto principale
        }
      },
      error: error => {
        console.error('Error uploading photo:', error);
        this.loading.set(false); // impostiamo lo stato di caricamento a false in caso di errore
      }
    })
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: () => {
        this.setMainLocalPhoto(photo)
      }
    })
  }

  deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: () => { //qui devi aggiornare la lista di foto senza la foto che elimini
        this.photos.update(photos => photos.filter(x => x.id !== photoId)) //riaggiorna
        //  la lista con tutti gli elementi che non matchano la stringa 
        // che gli abbiamo passato
      }
    })
  }

  // get photoMocks(){
  //   return Array.from({length: 20}, (_, I)=>({
  //     url: '/user.png'
  //   }))
  // }


  private setMainLocalPhoto(photo: Photo) {
    const currentUser = this.accountService.currentUser(); //ci andiamo a recuperare il current user 
    if (currentUser) currentUser.imageUrl = photo.url //se currentUser() true => impostiamo l'imageUrl = alla photo che stiamo passando al metodo
    this.accountService.setCurrentUser(currentUser as User); //reimpostiamo l'user con le impostazioni aggiornate. 
    // Mettiamo as User per aggirare il problema di typeScript che non accetta Undefined in questo caso
    // perche nella firma del metodo setCurrentUser l'argomento deve essere di tipo User
    this.memberService.member.update(member => ({ //aggiorniamo anche l'imageURL del member
      ...member, //lasciamo tutti gli altri dati invariati
      imageUrl: photo.url //qui aggiorniamo invece solo la photoUrl
    }) as Member)
  }
}
