import { Component, inject, input, OnInit, signal, ViewChild } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { Member, MemberParams } from '../../../types/member';
import { MemberCard } from '../member-card/member-card';
import { PaginatedResult } from '../../../types/paginations';
import { Paginator } from '../../../shared/paginator/paginator';
import { FilterModal } from '../../filter-modal/filter-modal';
import { filter } from 'rxjs';
import { ToastService } from '../../../core/services/toast-service';

@Component({
  selector: 'app-member-list',
  imports: [MemberCard, Paginator, FilterModal],
  templateUrl: './member-list.html',
  styleUrl: './member-list.css',
})
export class MemberList implements OnInit{
  private memberService= inject(MemberService)
  // protected paginatedMembers$?: Observable<PaginatedResult<Member>> //proprietà che memorizza un observable di members
  // //utilizzo di async pipe per gestire l'observable direttamente nel template
  protected paginatedMembers= signal<PaginatedResult<Member> | null>(null);
  @ViewChild('filterModal') modal!: FilterModal //passiamo il riferimento di filter modal al padre del componente che è padre del template dove si trova filterModal
  protected memberParams= new MemberParams();
  private updatedParams= new MemberParams();
  protected cities =signal<string[]>([]);
  protected countries =signal<string[]>([]);
  protected toast= inject(ToastService)

  constructor(){
    const filters= localStorage.getItem('filters')
    if(filters){
      this.memberParams= JSON.parse(filters)
      this.updatedParams= JSON.parse(filters)
    }
  }

  ngOnInit(): void {
    this.loadMembers()
    this.getCities()
  }

  loadMembers(){
    this.memberService.getMembers(this.memberParams).subscribe({
      next: result =>{
        this.paginatedMembers.set(result)
      }
    })
  }

  onPageChange(event: {pageNumber: number, pageSize: number}){
    this.memberParams.pageSize= event.pageSize
    this.memberParams.pageNumber= event.pageNumber
    this.loadMembers();
  }

  openModal(){
    this.modal.open();
  }

  onClose(){
    console.log('Modal closed')
  }

  onFilterChange(data:MemberParams){
    console.log("memberParams", this.memberParams.country)
    // foreach(item in countries()){
    //   if(this.memberParams.country!== item){

    //   }
    // }
    this.memberParams= {...data}; //creo una copia di dati che quindi mi ritorna i dati non aggiornati fin quando non spingo su submit
    this.updatedParams={...data};
    this.loadMembers()
  }

  resetFilters(){
    this.memberParams= new MemberParams();
    this.updatedParams= new MemberParams();
    this.loadMembers();
  }

  get displayMessage() : string{
    const defaultParams = new MemberParams();
    const filters: string[] = []

    if(this.updatedParams.gender){
      filters.push(this.updatedParams.gender + 's')
    }else{
      filters.push('Males, Females')
    }

    if(this.updatedParams.minAge !== defaultParams.minAge || this.updatedParams.maxAge !== defaultParams.maxAge){
      filters.push(` ages ${this.updatedParams.minAge}-${this.updatedParams.maxAge}`)
    }

    filters.push(this.updatedParams.orderBy === 'lastActive' ? 'Recently active' : 'Newest members')

    return filters.length > 0 ? `Selected ${filters.join(' | ')}` : 'All Members' 
  }

  getCities(){
    this.memberService.getCities().subscribe({
      next: response => {
        // console.log(response),
        this.cities.set(response)
        // console.log(this.cities())
      }
    })
  }

  getCountries(){
    this.memberService.getCountries().subscribe({
      next: response => {
        this.countries.set(response)
      }
    })
  }
}
function foreach(arg0: boolean) {
  throw new Error('Function not implemented.');
}

