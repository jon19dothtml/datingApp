export type Member = {
  id: string
  dateOfBirth: string
  imageUrl?: string
  displayName: string
  created: string
  lastActive: string
  gender: string
  description: string
  city: string
  country: string
}

export type Photo = {
  id: number
  url: string
  publicId?: string
  isApproved: boolean
  memberId: string
}

export type EditableMember = {
  displayName: string;
  description?: string;
  city: string;
  country:string
}

export class MemberParams { //definiamo una classe e non un tipo perchè essa può 
  gender?: string;          // contenere valori che possiamo inizializzare
  minAge= 18;
  maxAge= 100;
  pageSize=10;
  pageNumber=1;
  orderBy= 'lastActive'
  city?: string;
  country?: string;
}