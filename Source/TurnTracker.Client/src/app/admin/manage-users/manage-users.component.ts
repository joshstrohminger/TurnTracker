import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { Role } from 'src/app/auth/models/Role';
import { MessageService } from 'src/app/services/message.service';
import { UserService } from 'src/app/services/user.service';
import { DangerDialog, IDangerDialogOptions } from 'src/app/user/danger-dialog/danger.dialog';
import { User } from 'src/app/user/models/User';

@Component({
  selector: 'app-manage-users',
  templateUrl: './manage-users.component.html',
  styleUrls: ['./manage-users.component.scss']
})
export class ManageUsersComponent implements OnInit, OnDestroy {

  private readonly destroyed$ = new Subject<void>();
  private _isLoading = false;
  private _users: User[] = [];

  public get isLoading(): boolean {
    return this._isLoading;
  }

  public get users(): User[] {
    return this._users;
  }

  public get roles() {
    return Role;
  }

  public get myUserId(): number {
    return this.userService.currentUser?.id;
  }

  constructor(private http: HttpClient, private messageService: MessageService, private userService: UserService, private matDialog: MatDialog) { }

  ngOnInit(): void {
    this._isLoading = true;
    this.http.get<User[]>('users')
      .pipe(
        takeUntil(this.destroyed$),
        finalize(() => this._isLoading = false))
      .subscribe(users => {
        this._users = users ?? [];
        console.log('users', users);
      });
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  public reset(user: User): void {
    if(this._isLoading || user.id === this.myUserId) {
      return;
    }

    const options: IDangerDialogOptions = {
      action: 'Reset Password',
      prompt: `Are you sure you want to reset the password for ${user.name}?`
    };

    const dialogRef = this.matDialog.open(DangerDialog, {data: options});
    dialogRef.afterClosed().pipe(takeUntil(this.destroyed$)).subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.http.post(`auth/reset/${user.id}`, null)
        .pipe(
          takeUntil(this.destroyed$),
          finalize(() => this._isLoading = false))
        .subscribe(() => {
          this.messageService.success(`Password reset for ${user.name}`);
        });
      } else {
        this._isLoading = false;
      }
    });

    this._isLoading = true;
  }
}
