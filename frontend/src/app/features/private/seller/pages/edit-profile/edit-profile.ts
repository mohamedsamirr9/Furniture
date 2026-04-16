import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css',
})
export class EditProfile {
  profileForm: FormGroup;

  isLoadingProfile = false;
  successProfile = false;
  errorProfile = '';

  imagePreviewUrl: string | null = 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop';
  selectedFile: File | null = null;

  isEditing = false;

  seller = {
    name: 'Marcus Woodwell',
    location: 'Brooklyn, NY',
    bio: 'Master craftsman with over 15 years of experience in bespoke furniture design. Specializing in solid wood pieces that blend Scandinavian minimalism with timeless craftsmanship. Every piece is handmade in my Brooklyn workshop using sustainably sourced materials.',
    specialties: ['Dining Tables', 'Bookshelves', 'Coffee Tables', 'Custom Cabinetry'],
    completedOrders: 142,
    rating: 4.9,
    reviewsCount: 87,
  };

  portfolio = [
    { id: 1, category: 'TABLES', title: 'Solid Oak Dining Table', description: '8-seater dining table crafted from sustainably sourced European oak.', imageUrl: 'https://images.unsplash.com/photo-1617806118233-18e1de247200?w=600&auto=format&fit=crop' },
    { id: 2, category: 'STORAGE', title: 'Floating Wall Shelves', description: 'Minimalist floating shelves with hidden brackets.', imageUrl: 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&auto=format&fit=crop' },
    { id: 3, category: 'CHAIRS', title: 'Sage Velvet Armchair', description: 'Mid-century inspired armchair in premium sage green velvet.', imageUrl: 'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&auto=format&fit=crop' },
  ];

  constructor(private fb: FormBuilder) {
    this.profileForm = this.fb.group({
      name: [this.seller.name, [Validators.required, Validators.minLength(2)]],
      location: [this.seller.location, [Validators.required]],
      bio: [this.seller.bio, [Validators.required, Validators.minLength(10)]],
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      const reader = new FileReader();
      reader.onload = () => { this.imagePreviewUrl = reader.result as string; };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  saveProfile() {
    if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
    this.isLoadingProfile = true;
    this.errorProfile = '';
    setTimeout(() => {
      this.seller.name = this.profileForm.value.name;
      this.seller.location = this.profileForm.value.location;
      this.seller.bio = this.profileForm.value.bio;
      this.isLoadingProfile = false;
      this.successProfile = true;
      this.isEditing = false;
      setTimeout(() => (this.successProfile = false), 3000);
    }, 1000);
  }

  cancelEdit() {
    this.profileForm.patchValue({
      name: this.seller.name,
      location: this.seller.location,
      bio: this.seller.bio,
    });
    this.isEditing = false;
  }
}