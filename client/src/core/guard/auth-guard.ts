import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account-service';
import { inject } from '@angular/core';
import { ToastService } from '../services/toast-service';

export const authGuard: CanActivateFn = () => {
  const accountService = inject(AccountService);
  const toastService = inject(ToastService);

  if(accountService.currentUser()) return true;
  else{
    toastService.errorToast("You shall not pass! Login required.");
    return false;
  }
};
