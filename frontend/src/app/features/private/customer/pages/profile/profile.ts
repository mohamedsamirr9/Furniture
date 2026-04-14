import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../../core/services/auth.service';
import { UserDto } from '../../../../../core/models/auth.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class ProfileComponent implements OnInit {
  personalForm: FormGroup;
  passwordForm: FormGroup;
  addressForm: FormGroup;
  becomeSellerForm: FormGroup;

  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  isLoadingPersonal = false;
  isLoadingPassword = false;
  isLoadingAddress = false;
  isLoadingBecomeSeller = false;

  successPersonal = false;
  successPassword = false;
  successAddress = false;
  successBecomeSeller = false;

  errorPersonal = '';
  errorPassword = '';
  errorAddress = '';
  errorBecomeSeller = '';

  user: UserDto | null = null;
  nationalIdImageBase64: string | null = null;

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.personalForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    });

    this.addressForm = this.fb.group({
      address: ['', [Validators.required]],
    });

    this.becomeSellerForm = this.fb.group({
      nationalId: [null, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe((user: UserDto | null) => {
      this.user = user;
      if (user) {
        this.personalForm.patchValue({ name: user.name });
        this.addressForm.patchValue({ address: user.address || '' });
      }
    });

    // Fetch latest user data from server
    this.authService.getCurrentUser().subscribe();
  }

  savePersonal() {
    if (this.personalForm.invalid) { this.personalForm.markAllAsTouched(); return; }
    this.isLoadingPersonal = true;
    this.errorPersonal = '';

    const updateData = {
      name: this.personalForm.value.name,
      address: this.addressForm.value.address
    };

    this.authService.updateProfile(updateData as any).subscribe({
      next: () => {
        this.isLoadingPersonal = false;
        this.successPersonal = true;
        setTimeout(() => (this.successPersonal = false), 3000);
      },
      error: (err: any) => {
        this.isLoadingPersonal = false;
        this.errorPersonal = err.error?.message || 'Failed to update profile.';
      }
    });
  }

  changePassword() {
    if (this.passwordForm.invalid) { this.passwordForm.markAllAsTouched(); return; }
    const { newPassword, confirmPassword } = this.passwordForm.value;
    if (newPassword !== confirmPassword) { this.errorPassword = 'Passwords do not match.'; return; }
    
    this.isLoadingPassword = true;
    this.errorPassword = '';
    
    this.authService.changePassword(this.passwordForm.value).subscribe({
      next: () => {
        this.isLoadingPassword = false;
        this.successPassword = true;
        this.passwordForm.reset();
        setTimeout(() => (this.successPassword = false), 3000);
      },
      error: (err: any) => {
        this.isLoadingPassword = false;
        this.errorPassword = err.error?.message || 'Failed to change password.';
      }
    });
  }

  saveAddress() {
    if (this.addressForm.invalid) { this.addressForm.markAllAsTouched(); return; }
    this.isLoadingAddress = true;
    this.errorAddress = '';

    const updateData = {
      name: this.personalForm.value.name,
      address: this.addressForm.value.address
    };

    this.authService.updateProfile(updateData as any).subscribe({
      next: () => {
        this.isLoadingAddress = false;
        this.successAddress = true;
        setTimeout(() => (this.successAddress = false), 3000);
      },
      error: (err: any) => {
        this.isLoadingAddress = false;
        this.errorAddress = err.error?.message || 'Failed to update address.';
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        this.nationalIdImageBase64 = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  submitBecomeSeller() {
    if (!this.nationalIdImageBase64) {
      this.errorBecomeSeller = 'Please upload your National ID image.';
      return;
    }

    this.isLoadingBecomeSeller = true;
    this.errorBecomeSeller = '';

    this.authService.becomeSeller({ nationalIdImageBase64: this.nationalIdImageBase64 }).subscribe({
      next: () => {
        this.isLoadingBecomeSeller = false;
        this.successBecomeSeller = true;
      },
      error: (err: any) => {
        this.isLoadingBecomeSeller = false;
        this.errorBecomeSeller = err.error?.message || 'Failed to submit request.';
      }
    });
  }
}