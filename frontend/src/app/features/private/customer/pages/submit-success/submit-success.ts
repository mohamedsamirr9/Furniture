import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-submit-success',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './submit-success.html',
  styleUrl: './submit-success.css',
})
export class SubmitSuccess {
  constructor(private router: Router) {}

  submitAnother() {
    this.router.navigate(['/customer']);
  }
}