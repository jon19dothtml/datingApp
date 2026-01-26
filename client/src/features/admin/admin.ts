import { NgIf } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { UserManagement } from './user-management/user-management';
import { PhotoManagement } from "./photo-management/photo-management";

@Component({
  selector: 'app-admin',
  imports: [UserManagement, PhotoManagement], //ngIf ormai deprecato ritorna un valore bool che ci indica o meno se il component può essere attivato. Elimina l'elemento dal DOM
  // Directives are used to extend the behavior or    
  // appearance of HTML elements and components. They 
  // allow you to manipulate the DOM, add custom      
  // attributes, and respond to events.
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export class Admin {
  protected accountService= inject(AccountService);
  activeTab = 'photos'
  tabs = [
    {label: 'Photo moderation', value:'photos'},
    {label: 'User management', value:'roles'}
  ]

  setTab(tab:string){
    this.activeTab= tab;
  }
}
