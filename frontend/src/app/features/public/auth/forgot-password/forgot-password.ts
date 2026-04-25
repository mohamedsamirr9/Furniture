import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule, TranslateModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  step: number = 1;
  savedEmail: string = '';

  forgotForm: FormGroup;
  resetForm: FormGroup;
  
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.resetForm = this.fb.group({
      token: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('newPassword')?.value;
    const confirm = control.get('confirmPassword')?.value;
    return password === confirm ? null : { passwordMismatch: true };
  }

  onSubmitForgot() {
    if (this.forgotForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const email = this.forgotForm.value.email;

    this.authService.forgotPassword(email).subscribe({
      next: () => {
        this.isLoading = false;
        this.savedEmail = email;
        this.step = 2;
        this.successMessage = 'A reset token has been sent to your email.';
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Something went wrong. Please try again.';
      }
    });
  }

  onSubmitReset() {
    if (this.resetForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const payload = {
      email: this.savedEmail,
      token: this.resetForm.value.token,
      newPassword: this.resetForm.value.newPassword
    };

    this.authService.resetPassword(payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Password reset successful! Redirecting to login...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Invalid token or password rules not met.';
      }
    });
  }
}
