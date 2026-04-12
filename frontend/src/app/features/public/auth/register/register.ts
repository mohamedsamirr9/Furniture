import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-register',
  imports: [RouterModule, TranslateModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  constructor(private router: Router) {}

  goHome() {
    this.router.navigate(['']);
  }
}
