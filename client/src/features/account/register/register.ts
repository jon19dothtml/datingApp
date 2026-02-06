import { Component, inject, output, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { RegisterCreds, User } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';
import { JsonPipe } from '@angular/common';
import { ValidationError } from '@angular/forms/signals';
import { TextInput } from "../../../shared/text-input/text-input";
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, TextInput, TranslatePipe],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  //membersFromHome= input.required<User[]>(); //signal di input per ricevere i dati dal home component
  cancelRegister = output<boolean>(); //signal di output per notificare al componente padre che la registrazione è stata annullata
  private accountService = inject(AccountService);
  private router = inject(Router)
  private fb = inject(FormBuilder)
  protected creds = {} as RegisterCreds;
  protected credentialsForm: FormGroup; //registerFrom proprietà di FormGroup
  protected profileForm: FormGroup;
  protected currentStep = signal(1);
  protected validationErrors = signal<string[]>([]);


  constructor() {
    this.credentialsForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4),
      Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValue('password')]], //usiamo la nostra custom validator per confrontare il valore con quello del campo password
    })

    this.profileForm = this.fb.group({
      gender: ['male', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
    })

    this.credentialsForm.controls['password'].valueChanges.subscribe(() => { //ci iscriviamo ai cambiamenti del campo password
      this.credentialsForm.controls['confirmPassword'].updateValueAndValidity(); //aggiorniamo il valore e la validità del campo confirmPassword ogni volta che cambia il valore di password per rieseguire la validazione
      //in pratica ogni volta che cambia la password, viene rieseguita la validazione su confirmPassword per verificare se corrisponde ancora
    })
  }

  matchValue(matchTo: string): ValidatorFn { //ritorniamo una validator function
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent; //prendiamo il parent che sarà il nostro fromGroup che contiene i nostri campi tra cui quello che dovremo confrontare
      if (!parent) return null; //se non c'è il parent non possiamo fare il confronto quindi ritorniamo null (nessun errore)
      const matchValue = parent.get(matchTo)?.value //prendiamo il valore del campo da confrontare
      return control.value === matchValue ? null : { passwordMismatch: true } //se i valori sono uguali ritorniamo null (nessun errore), altrimenti ritorniamo un oggetto con l'errore
    }
  }

  nextStep() {
    if (this.credentialsForm.valid) {
      this.currentStep.update(prevStep => prevStep + 1)
    }
  }

  prevStep() {
    if (this.credentialsForm.valid) {
      this.currentStep.update(prevStep => prevStep - 1)
    }
  }

  getMaxDate() {
    const today = new Date();
    today.setFullYear(today.getFullYear() - 18);
    return today.toISOString().split('T')[0] //splittiamo prima del carattere T perche 
    //l'iso ci ritorna anche l'ora ma a noi interessa la data che vengono prima della T
    //Lo split ci ritorna un array e a noi interessa solo la prima parte dell'array
    //a cui accediamo con 0
  }

  register() { //pulsante registrati
    if (this.profileForm.valid && this.credentialsForm.valid) { //controlliamo se entrambi i form sono validati
      const formData = {
        ...this.credentialsForm.value,
        ...this.profileForm.value
      };
      this.accountService.register(formData).subscribe({
        next: () => {
          this.router.navigateByUrl('/members')
          //this.cancel() con la navigazione del router il component si autodistrugge e non abbiamo più bisogno 
          //di richiamare il cancel
        },
        error: error => { 
          alert(error.message)
          this.validationErrors.set(error) },
      })
    }

  }

  cancel() {
    this.cancelRegister.emit(false); //emette un evento al componente padre (home) per notificare che la registrazione è stata annullata
  }
}
