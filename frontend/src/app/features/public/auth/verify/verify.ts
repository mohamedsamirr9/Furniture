import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-verify',
  imports: [],
  templateUrl: './verify.html',
  styleUrl: './verify.css',
})
export class Verify {
  constructor(private router: Router) {}

  goHome() {
    this.router.navigate(['']);
  }
}
