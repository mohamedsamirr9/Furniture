import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-role',
  imports: [],
  templateUrl: './role.html',
  styleUrl: './role.css',
})
export class Role {
  constructor(private router: Router) {}

  goHome() {
    this.router.navigate(['']);
  }
}
