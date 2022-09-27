import { HttpClient } from '@angular/common/http';
import { Component, OnInit , Input, Output, EventEmitter} from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Route, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

 @Output() cancelRegister=new EventEmitter();
 //public model:any ={};
 registerForm: FormGroup;
 maxDate:Date;
 validationErrors: string[]=[];

  constructor(private accountService:AccountService, private toastr: ToastrService, 
    private fb: FormBuilder, private router: Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate=new Date();
    //can't register if u have less than 18
    this.maxDate.setFullYear(this.maxDate.getFullYear()-18);
  }

  initializeForm(){
     /* this.registerForm=new FormGroup({
      username: new FormControl('',Validators.required),
      password: new FormControl('', [Validators.required, 
                                    Validators.minLength(4), 
                                    Validators.maxLength(8)]),
      confirmPassword: new FormControl('', [Validators.required, this.matchValues('password')])
    
    })*/
    this.registerForm=this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required,
        Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['',[Validators.required, this.matchValues('password')]]
    })

    //pratimo trenutno stanje password-a, da prilikom svake izmene
    //proverimo poklapanje sa confirmPassword-om
    this.registerForm.controls.password.valueChanges.subscribe(()=>{
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })
  }

  matchValues(matchTo: string): ValidatorFn{
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.controls[matchTo].value
      ?null:{isMatching: true}
    }
  }
  register(){
    //console.log(this.registerForm.value);
   this.accountService.register(this.registerForm.value).subscribe(response =>{
      this.router.navigateByUrl('/members');
      
   }, error =>{
      //this.toastr.error(error.error); ne treba nam tostr vise jer vraca error iz intorseptora
      this.validationErrors=error;
   })
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

}
