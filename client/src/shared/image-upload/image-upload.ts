import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-image-upload',
  imports: [],
  templateUrl: './image-upload.html',
  styleUrl: './image-upload.css',
})
export class ImageUpload {
  protected imageSrc = signal<string | ArrayBuffer | null | undefined>(null); //variabile per memorizzare l'anteprima dell'immagine
  protected isDragging = false;
  private fileToUpload: File | null = null; //variabile per memorizzare il file da caricare
  uploadFile = output<File>(); //mandare dati al componente padre(member-photo)!!!!!! 
  loading = input<boolean>(false) //ricevere dal componente padre(member-photo)!!!!!! 


  onDragOver(event: DragEvent) {
    event.preventDefault(); //serve a prevenire il comportamento di default del browser
    event.stopPropagation(); //serve a fermare la propagazione dell'evento
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault(); //serve a prevenire il comportamento di default del browser
    event.stopPropagation(); //serve a fermare la propagazione dell'evento
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault(); //serve a prevenire il comportamento di default del browser
    event.stopPropagation(); //serve a fermare la propagazione dell'evento
    this.isDragging = false;
    if(event.dataTransfer?.files.length){ //controllo se ci sono file
      const file= event.dataTransfer.files[0]; //prendo il primo file
      this.previewImage(file); //mostro l'anteprima dell'immagine
      this.fileToUpload= file; //memorizzo il file da caricare
    }
  }

  onCancel(){
    this.fileToUpload= null; //resetto il file da caricare
    this.imageSrc.set(null); //resetto l'anteprima dell'immagine
  }

  onUpload(){
    if(this.fileToUpload){
      this.uploadFile.emit(this.fileToUpload); //invio il file da caricare al componente padre(member-photo)
    }
  }

  private previewImage(file: File){
    const reader= new FileReader(); //oggetto che legge il contenuto del file
    reader.onload= (e) => this.imageSrc.set(e.target?.result); //quando il file è caricato, in imageSrc metto il risultato della lettura
    reader.readAsDataURL(file); //legge il contenuto del file e lo converte in un data URL
  }
}

