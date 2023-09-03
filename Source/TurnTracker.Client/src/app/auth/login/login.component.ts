import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Credentials } from './Credentials';
import { AuthService } from '../auth.service';
import { Overlay } from '@angular/cdk/overlay';
import { MatSpinner } from '@angular/material/progress-spinner';
import { ComponentPortal } from '@angular/cdk/portal';
import { WebauthnService } from '../webauthn.service';
import { Subject } from 'rxjs';
import { first, takeUntil } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {

  public loginForm: FormGroup<{username: FormControl<string>, password: FormControl<string>, save: FormControl<boolean>}>;
  public error: string = null;
  private destroyed = new Subject<void>();
  private overlayRef = this.overlay.create({
    positionStrategy: this.overlay.position()
     .global()
     .centerHorizontally()
     .centerVertically()
    });

  constructor(
    formBuilder: FormBuilder,
    public authService: AuthService,
    private overlay: Overlay,
    public webauthn: WebauthnService,
    private route: ActivatedRoute) {
    this.loginForm = formBuilder.group(this.getDefaultFormValues());
  }

  private getDefaultFormValues() {
    const save = this.authService.shouldSaveUsername;
    return {
      username: this.authService.savedUsername || '',
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

    this.webauthn.isAvailable$.pipe(
      first(available => available && (this.loginForm.controls.username.valid || !this.webauthn.usernameRequired)),
      takeUntil(this.destroyed)
    ).subscribe(() => this.deviceLogin());
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
    this.authService.login(this.loginForm.value as Credentials, this.route.snapshot.queryParams.redirectUrl)
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
    this.webauthn.assertDevice$(this.loginForm.value.username, this.route.snapshot.queryParams.redirectUrl).subscribe(
      () => this.stopSpinner(),
      () => this.stopSpinner());
  }
}
