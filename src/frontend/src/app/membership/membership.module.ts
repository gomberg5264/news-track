import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MemberComponent } from './member/member.component';
import { PanelComponent } from './panel/panel.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AuthGuardService } from '../services/Guards/auth-guard.service';
import { MembershipComponent } from './membership/membership.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { RelationshipExecutorComponent } from './relationship-executor/relationship-executor.component';

@NgModule({
  imports: [
    CommonModule,
    NgbModule,
    TranslateModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: '', component: MembershipComponent, canActivate: [AuthGuardService], children: [
        { path: 'member', component: MemberComponent, canActivate: [AuthGuardService] },
        { path: 'panel', component: PanelComponent, canActivate: [AuthGuardService] }
      ]},
    ])
  ],
  declarations: [MemberComponent, PanelComponent, MembershipComponent, ChangePasswordComponent, RelationshipExecutorComponent]
})
export class MembershipModule { }
