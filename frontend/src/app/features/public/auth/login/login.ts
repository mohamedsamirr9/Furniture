import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterModule, TranslateModule, ReactiveFormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.isLoading = false;
        const role = this.authService.getUserRole();
        
        if (role === 'seller') {
          this.router.navigate(['/seller']);
        } else if (role === 'admin') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Login failed. Please check your credentials.';
      }
    });
  }

  goHome() {
    this.router.navigate(['']);
  }
}
