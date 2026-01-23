import { Component, ElementRef, model, output, ViewChild } from '@angular/core';
import { MemberParams } from '../../types/member';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-filter-modal',
  imports: [FormsModule],
  templateUrl: './filter-modal.html',
  styleUrl: './filter-modal.css',
})
export class FilterModal {
  @ViewChild('filterModal') modalRef!: ElementRef<HTMLDialogElement> //con ViewChild prendo il riferimento all'attributo dialog HTML. 
  // Usiamo ViewChild per accedere agli elementi del DOM nel template del componente Angular.
  closeModal=output(); //per chiudere modal
  submitData= output<MemberParams>()
  memberParams= model(new MemberParams());

  constructor(){
    const filters= localStorage.getItem('filters')
    if(filters){
      this.memberParams.set(JSON.parse(filters))
    }
  }

  open(){
    this.modalRef.nativeElement.showModal(); //prendiamo il modale
  }

  close(){
    this.modalRef.nativeElement.close();
    this.closeModal.emit();
  }

  submit(){
    this.submitData.emit(this.memberParams()); //emit lo usiamo per mandare i dati al componente padre. new MemberParams() crea un nuovo oggetto MemberParams
    this.close();
  }

  onMinAgeChange(){
    if(this.memberParams().minAge < 18) this.memberParams().minAge= 18;
  }

  onMaxAgeChange(){
    if(this.memberParams().maxAge < this.memberParams().minAge){
      this.memberParams().maxAge = this.memberParams().minAge
    }
  }
  
}
