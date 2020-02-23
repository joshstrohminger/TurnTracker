import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Credentials } from './Credentials';
import { AuthService } from '../auth.service';
import { Overlay } from '@angular/cdk/overlay';
import { MatSpinner } from '@angular/material/progress-spinner';
import { ComponentPortal } from '@angular/cdk/portal';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  public loginForm: FormGroup;
  public error: string = null;
  private overlayRef = this.overlay.create({
    //hasBackdrop: true,
    //backdropClass: 'dark-backdrop',
    positionStrategy: this.overlay.position()
     .global()
     .centerHorizontally()
     .centerVertically()
    });

  constructor(formBuilder: FormBuilder, private authService: AuthService, private overlay: Overlay) {
    this.loginForm = formBuilder.group(new Credentials());
  }

  ngOnInit() {
  }

  showSpinner() {
    this.overlayRef.attach(new ComponentPortal(MatSpinner));
    this.loginForm.disable();
  }
  stopSpinner() {
    this.overlayRef.detach();
    this.loginForm.enable();
  }

  onSubmit() {
    this.error = null;
    this.showSpinner();
    this.authService.login(this.loginForm.value as Credentials)
      .subscribe(errorMessage => {
        if (errorMessage) {
          this.error = errorMessage;
        } else {
          this.loginForm.reset();
        }
      }, error => {
        console.error('failed to login', error);
        this.error = 'Unknown error';
        this.stopSpinner();
      }, () => {
        this.stopSpinner();
      });
  }
}
