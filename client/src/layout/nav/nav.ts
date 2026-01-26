import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';
import { themes } from '../themes';
import { BusyService } from '../../core/services/busy-service';
import { HasRole } from '../../shared/directives/has-role';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive, HasRole],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav implements OnInit{
  protected accountService = inject(AccountService);
  private toastService = inject(ToastService);
  protected busyService = inject(BusyService);
  protected creds: any = {}
  private router = inject(Router);
  protected selectedTheme= signal<string>(localStorage.getItem('theme') || 'light')
  protected themes= themes;

  ngOnInit(): void {
    document.documentElement.setAttribute('data-theme', this.selectedTheme())
  }

  handleSelectTheme(theme:string){
    this.selectedTheme.set(theme)
    localStorage.setItem('theme', theme)
    document.documentElement.setAttribute('data-theme', theme)
    const elem= document.activeElement as HTMLDivElement; //per far chiudere la dropdown una volta selezionato
    if(elem) elem.blur()
  }

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
