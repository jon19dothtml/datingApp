import { Component, input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  imports: [ReactiveFormsModule],
  templateUrl: './text-input.html',
  styleUrl: './text-input.css',
})
export class TextInput implements ControlValueAccessor {

  label= input<string>('');
  type= input<string>('text')
  maxDate= input<string>('')

  constructor(@Self() public ngControl: NgControl){
    this.ngControl.valueAccessor= this; //stiamo dicendo che questa classe (TextInput) sarà il value accessor per il controllo del form associato a questo componente
    //cioè verrà qui ad essere controllato il valore di un input di un form
    //con @Self() stiamo dicendo di cercare NgControl solo su questo componente e non risalire nella gerarchia dei componenti
    
  }

  writeValue(obj: any): void {
  }
  registerOnChange(fn: any): void {
  }
  registerOnTouched(fn: any): void {
  }

  get control(): FormControl{ //get control() significa che stiamo creando una proprietà di sola lettura chiamata control. 
    return this.ngControl.control as FormControl; 
    //ritorniamo il controllo del form associato a questo componente convertito in FormControl
    //lo convertiamo perchè ngControl.control è di tipo AbstractControl che è una classe base più generica
    //questo lo facciamo per rendere più semplice l'accesso al controllo del form nel template HTML
  }
}
