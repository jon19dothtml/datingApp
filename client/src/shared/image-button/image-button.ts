import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-image-button',
  imports: [],
  templateUrl: './image-button.html',
  styleUrl: './image-button.css',
})
export class ImageButton {
  click= output<Event>(); //emettiamo un evento di click al componente padre
  fill= input<boolean>(); //impostiamo il fill di default a 'none'
  disabled= input<boolean>(false); //impostiamo disabled di default a false

  onClick(event: Event){
    this.click.emit(event); //quando viene cliccato il bottone, emettiamo l'evento di click
  }


}
