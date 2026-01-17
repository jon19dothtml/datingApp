import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiErorr } from '../../../types/error';

@Component({
  selector: 'app-server-error',
  imports: [],
  templateUrl: './server-error.html',
  styleUrl: './server-error.css',
})
export class ServerError {
  protected error: ApiErorr
  private router= inject(Router)
  protected showDetails = false;

  constructor(){
    const navigation= this.router.currentNavigation()
    this.error = navigation?.extras?.state?.['error']
  }

  detailsToggle(){
    this.showDetails= !this.showDetails
  }

}
