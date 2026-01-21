import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-delete-button',
  imports: [],
  templateUrl: './delete-button.html',
  styleUrl: './delete-button.css',
})
export class DeleteButton {
  clicked = output<Event>(); //emettiamo un evento di click al componente padre
  disabled = input<boolean>(); //impostiamo disabled di default a false

  onClick(event: Event) {
    this.clicked.emit(event); //quando viene cliccato il bottone, emettiamo l'evento di click
  }

}
