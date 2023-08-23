import { OverlayModule } from '@angular/cdk/overlay';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatLegacyAutocompleteModule as MatAutocompleteModule } from '@angular/material/legacy-autocomplete';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyCardModule as MatCardModule } from '@angular/material/legacy-card';
import { MatLegacyCheckboxModule as MatCheckboxModule } from '@angular/material/legacy-checkbox';
import { MatLegacyChipsModule as MatChipsModule } from '@angular/material/legacy-chips';
import { MatLegacyDialogModule as MatDialogModule } from '@angular/material/legacy-dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatLegacyFormFieldModule as MatFormFieldModule } from '@angular/material/legacy-form-field';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';
import { MatLegacyListModule as MatListModule } from '@angular/material/legacy-list';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { MatLegacyProgressSpinnerModule as MatProgressSpinnerModule } from '@angular/material/legacy-progress-spinner';
import { MatLegacySelectModule as MatSelectModule } from '@angular/material/legacy-select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatLegacySlideToggleModule as MatSlideToggleModule } from '@angular/material/legacy-slide-toggle';
import { MatLegacySnackBarModule as MatSnackBarModule, MAT_LEGACY_SNACK_BAR_DEFAULT_OPTIONS as MAT_SNACK_BAR_DEFAULT_OPTIONS } from '@angular/material/legacy-snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatLegacyTooltipModule as MatTooltipModule } from '@angular/material/legacy-tooltip';
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
import { LogsComponent } from './anonymous/logs/logs.component';
import { LuxonPipe } from './pipes/luxon.pipe';

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
      { path: 'users', component: ManageUsersComponent, data: {title: 'Manage Users'}},
    ]},
  { path: 'activities', component: ActivitiesComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard], data: {title: 'Profile'} },
  { path: 'settings', component: SettingsComponent, canActivate: [AuthGuard], data: {title: 'Settings'} },
  { path: 'login', component: LoginComponent, canActivate: [LoginGuard], data: {title: 'Login'} },
  { path: 'logs', component: LogsComponent, data: {title: 'Client Logs'}},
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
    ManageUsersComponent,
    LogsComponent,
    LuxonPipe
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
