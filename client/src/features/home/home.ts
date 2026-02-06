import { Component, inject, Input, signal } from '@angular/core';
import { Register } from "../account/register/register";
import { User } from '../../types/user';
import { AccountService } from '../../core/services/account-service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-home',
  imports: [Register, TranslatePipe],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  // @Input({required: true}) membersFromApp: User[]= []; //riceve i dati dall'app component
  protected registerMode= signal(false)
  protected accountService= inject(AccountService)

  showRegister(value: boolean) {
    this.registerMode.set(value)
  }
}
