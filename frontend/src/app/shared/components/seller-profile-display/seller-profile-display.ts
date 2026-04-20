import { CommonModule } from '@angular/common';
import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { SellerProfileViewModel } from '../../../core/models/seller-profile.model';
import { SellerService } from '../../../core/services/seller.service';

@Component({
  selector: 'app-seller-profile-display',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './seller-profile-display.html',
  styleUrl: './seller-profile-display.css',
})
export class SellerProfileDisplayComponent implements OnChanges {
  @Input() seller: SellerProfileViewModel | null = null;
  @Input() loading = false;
  @Input() error = '';
  /** Dashboard: enables edit UI, avatar upload, and save to API. */
  @Input() editMode = false;
  /** Public storefront CTA (hidden on dashboard). */
  @Input() showRequestCustomCta = true;

  @Output() profileUpdated = new EventEmitter<SellerProfileViewModel>();

  isEditing = false;
  isSaving = false;
  avatarUploading = false;
  saveError = '';
  sellerAvatarLoadFailed = false;

  private readonly fb = inject(FormBuilder);
  private readonly sellerService = inject(SellerService);

  profileForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    location: ['', [Validators.required]],
    bio: ['', [Validators.required, Validators.minLength(10)]],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['seller'] && this.seller) {
      this.sellerAvatarLoadFailed = false;
      this.patchFormFromSeller();
    }
  }

  private patchFormFromSeller(): void {
    if (!this.seller) return;
    this.profileForm.patchValue({
      name: this.seller.name,
      location: this.seller.location,
      bio: this.seller.bio,
    });
  }

  shouldShowProfileImage(): boolean {
    const url = this.seller?.profileImageUrl?.trim();
    return !!url && !this.sellerAvatarLoadFailed;
  }

  onProfileAvatarError(): void {
    this.sellerAvatarLoadFailed = true;
  }

  getAvatarInitial(name?: string): string {
    const t = (name ?? '').trim();
    if (!t) return '?';
    return t.charAt(0).toUpperCase();
  }

  getAvatarBackgroundStyle(name?: string): { [key: string]: string } {
    const key = (name ?? '').trim() || '?';
    let hash = 0;
    for (let i = 0; i < key.length; i++) {
      hash = key.charCodeAt(i) + ((hash << 5) - hash);
    }
    const hue = Math.abs(hash) % 360;
    return {
      'background-color': `hsl(${hue}, 52%, 46%)`,
      color: '#ffffff',
    };
  }

  toggleEdit(): void {
    if (!this.editMode) return;
    this.isEditing = !this.isEditing;
    this.saveError = '';
    if (!this.isEditing) {
      this.patchFormFromSeller();
    }
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.patchFormFromSeller();
    this.saveError = '';
  }

  saveProfile(): void {
    if (!this.editMode || this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.isSaving = true;
    this.saveError = '';
    const v = this.profileForm.getRawValue();
    this.sellerService
      .updateMyProfile({
        name: v.name ?? undefined,
        location: v.location ?? undefined,
        bio: v.bio ?? undefined,
      })
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (vm) => {
          this.isEditing = false;
          this.profileUpdated.emit(vm);
        },
        error: (err) => {
          console.error(err);
          this.saveError =
            err?.error?.message ?? 'Unable to save profile. Please try again.';
        },
      });
  }

  onAvatarFileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || !this.editMode) return;
    this.avatarUploading = true;
    this.saveError = '';
    this.sellerService
      .uploadProfileImage(file)
      .pipe(
        switchMap((res) =>
          this.sellerService.updateMyProfile({ profileImageUrl: res.secure_url })
        ),
        finalize(() => {
          this.avatarUploading = false;
        })
      )
      .subscribe({
        next: (vm) => {
          this.sellerAvatarLoadFailed = false;
          this.profileUpdated.emit(vm);
        },
        error: (err) => {
          console.error(err);
          this.saveError =
            err?.error?.message ??
            'Image upload or profile update failed. Please try again.';
        },
      });
  }
}
