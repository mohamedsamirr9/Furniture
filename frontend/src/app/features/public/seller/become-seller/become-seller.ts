import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-become-seller',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './become-seller.html',
  styleUrls: ['./become-seller.css']
})
export class BecomeSellerComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  becomeSellerForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.becomeSellerForm = this.fb.group({
      storeName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]]
    });
  }

  ngOnInit(): void {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (this.becomeSellerForm.invalid) {
      this.becomeSellerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const formValue = this.becomeSellerForm.value;

    this.authService.becomeSeller({ storeName: formValue.storeName }).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Your seller account has been created successfully! You can now start selling products.';
        this.becomeSellerForm.reset();
        
        // Auto-navigate to seller profile after 3 seconds
        setTimeout(() => {
          this.router.navigate(['/sellers/me']);
        }, 3000);
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err?.error?.message || err?.message || 'Failed to become a seller. Please try again.';
      }
    });
  }
}