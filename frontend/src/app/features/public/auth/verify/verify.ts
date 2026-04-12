import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-verify',
  imports: [RouterModule, TranslateModule, CommonModule],
  templateUrl: './verify.html',
  styleUrl: './verify.css',
})
export class Verify {
  constructor(private router: Router) {}

  goHome() {
    this.router.navigate(['']);
  }
}
