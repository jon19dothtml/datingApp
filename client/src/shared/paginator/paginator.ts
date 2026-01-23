import { Component, computed, input, model, output } from '@angular/core';

@Component({
  selector: 'app-paginator',
  imports: [],
  templateUrl: './paginator.html',
  styleUrl: './paginator.css',
})
export class Paginator {
  pageNumber= model(10)
  pageSize= model(10)
  totalCount= input(0);
  totalPages= input(0)
  pageSizeOptions= input([5,10,20,50,100]);

  pageChange= output<{pageNumber: number, pageSize: number}>();

  lastItemIndex= computed(() => {
    return Math.min(this.pageNumber() * this.pageSize(), this.totalCount());  
    // Qui calcola l'indice dell'ultimo elemento visualizzato. 
    // Questo calcolo prende il numero di pagina corrente moltiplicato per la
    //  dimensione della pagina e lo confronta con il conteggio totale degli elementi, 
    // restituendo il valore minore tra i due.
  })

  onPageChange(newPage?: number, pageSize?: EventTarget | null){
    if(newPage) this.pageNumber.set(newPage)
    if(pageSize) {
      const size= Number((pageSize as HTMLSelectElement).value)
       //l'evento change non accetta un number ma un event,
       //  quindi dobbiamo fare un cast a HTMLSelectElement, 
       // che è il numero selezionato nella dropdown <select>
      this.pageSize.set(size) // update page size if provided
    } 

    this.pageChange.emit({ //emettiamo un valore per l'output
      pageNumber: this.pageNumber(), // get current page number
      pageSize: this.pageSize() // get current page size
    })
  }
}
