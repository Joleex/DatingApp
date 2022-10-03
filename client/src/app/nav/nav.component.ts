import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { MemberListComponent } from '../members/member-list/member-list.component';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from '../_services/account.service';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any={};
  members: Member[];
  pagination: Pagination;
  userParams: UserParams;
  constructor(public accountService: AccountService, private router: Router, 
    private toastr: ToastrService, public memberService: MembersService) {
      //this.resetFilter==new MemberListComponent(memberService);
      
      this.userParams=this.memberService.getUserParams(); 
      
     }

  ngOnInit(): void {
  }

  login(){
    /*the value that we receved from the form */
    //console.log(this.model)

    this.accountService.login(this.model).subscribe(response =>{
      //console.log(response);
      this.router.navigateByUrl('/members');
    }/*,
    error=>{
      console.log(error);
      this.toastr.error(error.error);
    }*/);
  }

  reset(){

    this.userParams=this.memberService.resetUserParams();
    this.memberService.setUserParams(this.userParams);
    this.memberService.getMembers(this.userParams).subscribe(response =>{
      this.members=response.result;
      this.pagination=response.pagination;
      this.members;

    })
    //this.memberService.resetMembers(this.userParams);
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }


}
