import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  private toastService = inject(ToastService);
  protected creds: any = {}
  private router = inject(Router);

  login(){
    this.accountService.login(this.creds).subscribe({ //ricordarsi di sottoscrivere l'observable ritornato dal metodo login
      next: () => { 
        this.router.navigateByUrl('/members');
        this.toastService.successToast('Login successful');
        this.creds = {};
      },
      error: error => {
        this.toastService.errorToast(error.error); 
        //usiamo error.error perchè l'errore ritornato dal backend è 
        // un HttpErrorResponse che contiene varie proprietà, 
        // tra cui error che contiene il messaggio di errore vero e proprio
      }
    })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
