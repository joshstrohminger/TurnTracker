import { OverlayModule } from '@angular/cdk/overlay';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule, MAT_SNACK_BAR_DEFAULT_OPTIONS } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BrowserModule, Title } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Routes, RouterModule } from '@angular/router';
import { ServiceWorkerModule } from '@angular/service-worker';

import { AppComponent } from './app.component';
import { environment } from '../environments/environment';
import { NotFoundComponent } from './anonymous/not-found/not-found.component';
import { ProfileComponent } from './auth/profile/profile.component';
import { AboutComponent } from './anonymous/about/about.component';
import { LoginComponent } from './auth/login/login.component';
import { AuthGuard } from './auth/auth.guard';
import { AuthInterceptor } from './auth/auth.interceptor';
import { LoginGuard } from './auth/login.guard';
import { ActivitiesComponent } from './user/activities/activities.component';
import { ActivityComponent } from './user/activity/activity.component';
import { TakeTurnDialog } from './user/take-turn/take-turn.dialog';
import { NotificationPipe } from './user/notification.pipe';
import { TimerComponent } from './user/timer/timer.component';
import { TurnDetailsDialog } from './user/turn-details/turn-details.dialog';
import { EditActivityComponent } from './user/edit-activity/edit-activity.component';
import { DangerDialog } from './user/danger-dialog/danger.dialog';
import { SettingsComponent } from './user/settings/settings.component';
import { ActivityInterceptor } from './anonymous/activity.interceptor';
import { NotificationsComponent } from './user/notifications/notifications.component';
import { ReloadComponent } from './user/reload/reload.component';
import { ManageUsersComponent } from './admin/manage-users/manage-users.component';
import { AdminGuard } from './auth/admin.guard';

const routes: Routes = [
  {
    path: 'activity',
    canActivate: [AuthGuard],
    children: [
      { path: 'add', component: EditActivityComponent, data: {title: 'Add Activity'} },
      { path: ':id/edit', component: EditActivityComponent, data: {title: 'Edit Activity'} },
      { path: ':id', component: ActivityComponent, data: {title: 'Activity'} },
      { path: ':id/taketurn', component: ActivityComponent, data: {title: 'Activity'} },
      { path: ':id/turn/:turnId', component: ActivityComponent, data: {title: 'Activity'} },
    ]
  },
  { path: 'admin',
    canActivate: [AdminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'users' },
      { path: 'users', component: ManageUsersComponent, data: {title: 'Manage Users'}}
    ]},
  { path: 'activities', component: ActivitiesComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard], data: {title: 'Profile'} },
  { path: 'settings', component: SettingsComponent, canActivate: [AuthGuard], data: {title: 'Settings'} },
  { path: 'login', component: LoginComponent, canActivate: [LoginGuard], data: {title: 'Login'} },
  { path: 'about', component: AboutComponent, data: {title: 'About'} },
  { path: '', redirectTo: '/activities', pathMatch: 'full' },
  { path: '**', component: NotFoundComponent, data: {title: 'Not Found'} }
];

@NgModule({
  declarations: [
    AppComponent,
    ActivitiesComponent,
    NotFoundComponent,
    ProfileComponent,
    AboutComponent,
    LoginComponent,
    ActivitiesComponent,
    ActivityComponent,
    TakeTurnDialog,
    NotificationPipe,
    TimerComponent,
    TurnDetailsDialog,
    EditActivityComponent,
    DangerDialog,
    SettingsComponent,
    NotificationsComponent,
    ReloadComponent,
    ManageUsersComponent
  ],
  imports: [
    BrowserAnimationsModule,
    BrowserModule,
    FlexLayoutModule,
    FormsModule,
    HttpClientModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDialogModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSidenavModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
    MatToolbarModule,
    MatTooltipModule,
    OverlayModule,
    ReactiveFormsModule,
    RouterModule.forRoot(routes),
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production })
  ],
  providers: [
    Title,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ActivityInterceptor, multi: true },
    { provide: MAT_SNACK_BAR_DEFAULT_OPTIONS, useValue: {duration: 5000 }}
  ],
  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
