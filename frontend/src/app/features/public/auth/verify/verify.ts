import { Component } from '@angular/core';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-verify',
  standalone: true,
  imports: [RouterModule, TranslateModule, CommonModule, FormsModule],
  templateUrl: './verify.html',
  styleUrl: './verify.css',
})
export class Verify {
  otp: string[] = ['', '', '', '', '', ''];
  email: string = '';
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService
  ) {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
    });
  }

  onInput(event: any, index: number) {
    const input = event.target;
    const value = input.value;
    
    if (value && index < 5) {
      const nextInput = input.nextElementSibling;
      if (nextInput) nextInput.focus();
    }
    
    this.otp[index] = value;
  }

  onKeyDown(event: any, index: number) {
    if (event.key === 'Backspace' && !this.otp[index] && index > 0) {
      const prevInput = event.target.previousElementSibling;
      if (prevInput) prevInput.focus();
    }
  }

  verify() {
    const otpCode = this.otp.join('');
    if (otpCode.length < 6 || !this.email) {
      this.errorMessage = 'Please enter all digits and ensure email is provided.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    this.authService.verifyOtp(this.email, otpCode).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Email verified successfully! Redirecting to login...';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Verification failed. Please check your code.';
      }
    });
  }

  resendOtp() {
    if (!this.email) return;

    this.isLoading = true;
    this.authService.sendOtp(this.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Check your email for a new code.';
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to resend code.';
      }
    });
  }

  goHome() {
    this.router.navigate(['']);
  }
}
