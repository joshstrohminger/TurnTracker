import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Credentials } from './Credentials';
import { AuthService } from '../auth.service';
import { Overlay } from '@angular/cdk/overlay';
import { MatSpinner } from '@angular/material/progress-spinner';
import { ComponentPortal } from '@angular/cdk/portal';
import { WebauthnService } from '../webauthn.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

  public loginForm: FormGroup;
  public error: string = null;
  private destroyed = new Subject<void>();
  private overlayRef = this.overlay.create({
    positionStrategy: this.overlay.position()
     .global()
     .centerHorizontally()
     .centerVertically()
    });

  constructor(formBuilder: FormBuilder, public authService: AuthService, private overlay: Overlay, public webauthn: WebauthnService) {
    this.loginForm = formBuilder.group(this.getDefaultFormValues());
  }

  private getDefaultFormValues() {
    const save = this.authService.shouldSaveUsername;
    return {
      username: save ? (this.authService.savedUsername || '') : '',
      password: '',
      save: save
    };
  }

  ngOnDestroy(): void {
    this.destroyed.complete();
  }

  ngOnInit() {
    this.loginForm.controls.save.valueChanges
      .pipe(takeUntil(this.destroyed))
      .subscribe(save => this.authService.shouldSaveUsername = save);
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
          this.loginForm.reset(this.getDefaultFormValues());
        }
      }, error => {
        console.error('failed to login', error);
        this.error = 'Unknown error';
        this.stopSpinner();
      }, () => {
        this.stopSpinner();
      });
  }

  deviceLogin() {
    this.error = null;
    this.showSpinner();
    this.webauthn.assertDevice$(this.loginForm.value.username).subscribe(
      () => this.stopSpinner(),
      () => this.stopSpinner());
  }
}
