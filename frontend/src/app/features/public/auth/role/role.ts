import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-role',
  imports: [RouterModule, TranslateModule, CommonModule],
  templateUrl: './role.html',
  styleUrl: './role.css',
})
export class Role {
  constructor(private router: Router) {}

  goHome() {
    this.router.navigate(['']);
  }
}
