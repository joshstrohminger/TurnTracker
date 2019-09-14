import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ProfileService } from 'src/app/services/profile.service';
import { Credentials } from './Credentials';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  public loginForm: FormGroup;

  constructor(formBuilder: FormBuilder, private profileService: ProfileService) {
    this.loginForm = formBuilder.group(new Credentials());
  }

  ngOnInit() {
  }

  onSubmit() {
    const creds = this.loginForm.value as Credentials;
    this.profileService.login(creds.username, creds.password);
    this.loginForm.reset();
  }
}
