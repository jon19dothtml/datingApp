import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RegisterCreds, User } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  //membersFromHome= input.required<User[]>(); //signal di input per ricevere i dati dal home component
  cancelRegister= output<boolean>(); //signal di output per notificare al componente padre che la registrazione è stata annullata
  private accountService= inject(AccountService);
  protected creds = {} as RegisterCreds;

  register(){
    this.accountService.register(this.creds).subscribe({
      next: response => {
        console.log(response); 
        this.cancel()
      },
      error: error => {alert(error.message)},
    })
  }

  cancel(){
    this.cancelRegister.emit(false); //emette un evento al componente padre (home) per notificare che la registrazione è stata annullata
  }
}
