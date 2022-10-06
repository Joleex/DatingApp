import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
//import { userInfo } from 'os';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { MemberListComponent } from '../members/member-list/member-list.component';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { MembersService } from './members.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl= environment.apiUrl;
  //baseUrl='https://localhost:5001/api/';
  private currentUserSource=new ReplaySubject<User>(1);
  currentUser$ =this.currentUserSource.asObservable();
  
  memberCache=new Map();
  

  constructor(private http: HttpClient) {
   }
  
  login(model:any){
    
    return this.http.post(this.baseUrl+'account/login', model).pipe(
      map((response: User) => {
        const user=response;
        if(user){
          this.setCurrentUser(user);
        }
      })
    )
  
      
  }

  register(model:any){
    return this.http.post(this.baseUrl+'account/register',model).pipe(
      map((user:User)=> {
        if(user){
          this.setCurrentUser(user);
        }
      })
    )
  }

  setCurrentUser(user: User){
    user.roles=[];
    const roles=this.getDecodedToken(user.token).role;

    Array.isArray(roles)? user.roles=roles:user.roles.push(roles);

    localStorage.setItem('user',JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

  getDecodedToken(token){
    /*And this is just going to allow us to decode the information inside what the token is returned.
      As the token is not encrypted, the signature is the only part that's encrypted.
      And then we pass the data to this method, which is a string.-our token is a string*/
    return JSON.parse(atob(token.split('.')[1]));
  }
}
