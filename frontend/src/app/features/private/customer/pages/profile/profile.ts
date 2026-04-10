import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class ProfileComponent {
  personalForm: FormGroup;
  emailForm: FormGroup;
  passwordForm: FormGroup;
  addressForm: FormGroup;

  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  isLoadingPersonal = false;
  isLoadingEmail = false;
  isLoadingPassword = false;
  isLoadingAddress = false;

  successPersonal = false;
  successEmail = false;
  successPassword = false;
  successAddress = false;

  errorPersonal = '';
  errorEmail = '';
  errorPassword = '';
  errorAddress = '';

  constructor(private fb: FormBuilder) {
    this.personalForm = this.fb.group({
      fullName: ['Ahmed Ali', [Validators.required, Validators.minLength(2)]],
      phone: ['+20 1054123974', [Validators.required]],
    });

    this.emailForm = this.fb.group({
      newEmail: ['', [Validators.required, Validators.email]],
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    });

    this.addressForm = this.fb.group({
      street: ['123 King Fahd Road', [Validators.required]],
      city: ['Riyadh', [Validators.required]],
      state: ['Riyadh Region', [Validators.required]],
      zip: ['12345', [Validators.required]],
      country: ['Saudi Arabia', [Validators.required]],
    });
  }

  savePersonal() {
    if (this.personalForm.invalid) { this.personalForm.markAllAsTouched(); return; }
    this.isLoadingPersonal = true;
    setTimeout(() => {
      this.isLoadingPersonal = false;
      this.successPersonal = true;
      setTimeout(() => (this.successPersonal = false), 3000);
    }, 1000);
  }

  updateEmail() {
    if (this.emailForm.invalid) { this.emailForm.markAllAsTouched(); return; }
    this.isLoadingEmail = true;
    setTimeout(() => {
      this.isLoadingEmail = false;
      this.successEmail = true;
      this.emailForm.reset();
      setTimeout(() => (this.successEmail = false), 3000);
    }, 1000);
  }

  changePassword() {
    if (this.passwordForm.invalid) { this.passwordForm.markAllAsTouched(); return; }
    const { newPassword, confirmPassword } = this.passwordForm.value;
    if (newPassword !== confirmPassword) { this.errorPassword = 'Passwords do not match.'; return; }
    this.isLoadingPassword = true;
    this.errorPassword = '';
    setTimeout(() => {
      this.isLoadingPassword = false;
      this.successPassword = true;
      this.passwordForm.reset();
      setTimeout(() => (this.successPassword = false), 3000);
    }, 1000);
  }

  saveAddress() {
    if (this.addressForm.invalid) { this.addressForm.markAllAsTouched(); return; }
    this.isLoadingAddress = true;
    setTimeout(() => {
      this.isLoadingAddress = false;
      this.successAddress = true;
      setTimeout(() => (this.successAddress = false), 3000);
    }, 1000);
  }
}