import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-submit-success',
  imports: [],
  templateUrl: './submit-success.html',
  styleUrl: './submit-success.css',
})
export class SubmitSuccess {
  constructor(private router: Router) {}

  submitAnother() {
    this.router.navigate(['/customer']);
  }
}