import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoryService } from '../../../../../core/services/category.service';

import { CategoryCreateUpdateDto } from '../../../../../core/models/category-create-update-dto.model';

@Component({
  selector: 'app-categories',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categories.html',
  styleUrl: './categories.css',
})
export class Categories implements OnInit {
  categories: any = [];
  isLoading = true;
  showModal = false;
  isEditing = false;
  editingCategoryId: number | null = null;
  isSubmitting = false;
  successMessage = '';
  errorMessage = '';
  isUploading: boolean = false;

  categoryForm!: FormGroup;

  constructor(
    private categoryService: CategoryService,
    private fb: FormBuilder,
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
  }

  initForm(): void {
    this.categoryForm = this.fb.group({
      nameEn: ['', Validators.required],
      nameAr: [''],
      descriptionEn: ['', Validators.required],
      descriptionAr: [''],
      image: [''],
    });
  }

  loadCategories(): void {
    this.isLoading = true;
    this.categoryService.getAllCategories(1, 100).subscribe({
      next: (res: any) => {
        this.categories = res;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error fetching categories', err);
        this.isLoading = false;
      },
    });
  }
  onImageSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    this.isUploading = true;

    this.categoryService.uploadImage(file).subscribe((res: any) => {
      this.categoryForm.patchValue({
        image: res.secure_url,
      });
      this.isUploading = false;
    });
  }
  openAddModal(): void {
    this.isEditing = false;
    this.editingCategoryId = null;
    this.categoryForm.reset({
      nameEn: '',
      nameAr: '',
      descriptionEn: '',
      descriptionAr: '',
      image: '',
    });
    this.clearMessages();
    this.showModal = true;
  }

  openEditModal(cat: any): void {
    this.isEditing = true;
    this.editingCategoryId = cat.id;
    this.clearMessages();

    // Fetch full category details
    this.categoryService.getCategoryById(cat.id).subscribe({
      next: (details: any) => {
        this.categoryForm.patchValue({
          nameEn: details.nameEn,
          nameAr: details.nameAr || '',
          descriptionEn: details.descriptionEn || '',
          descriptionAr: details.descriptionAr || '',
          image: details.image || '',
        });
        this.showModal = true;
      },
      error: (err: any) => {
        console.error('Error fetching category details', err);
        // Fallback: use list data
        this.categoryForm.patchValue({
          nameEn: cat.nameEn || cat.name,
          nameAr: cat.nameAr || '',
          descriptionEn: cat.descriptionEn || '',
          descriptionAr: cat.descriptionAr || '',
          image: cat.image || '',
        });
        this.showModal = true;
      },
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.clearMessages();
  }

  onSubmit(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.clearMessages();

    const formValue = this.categoryForm.value;
    const dto: CategoryCreateUpdateDto = {
      nameEn: formValue.nameEn,
      nameAr: formValue.nameAr,
      descriptionEn: formValue.descriptionEn,
      descriptionAr: formValue.descriptionAr,
      image: formValue.image
    };

    if (this.isEditing && this.editingCategoryId !== null) {
      this.categoryService.updateCategory(this.editingCategoryId, dto).subscribe({
        next: () => {
          this.successMessage = 'Category updated successfully!';
          this.isSubmitting = false;
          this.loadCategories();
          setTimeout(() => this.closeModal(), 1200);
        },
        error: (err: any) => {
          this.errorMessage = err.error?.message || 'Failed to update category.';
          this.isSubmitting = false;
        },
      });
    } else {
      this.categoryService.createCategory(dto).subscribe({
        next: () => {
          this.successMessage = 'Category created successfully!';
          this.isSubmitting = false;
          this.loadCategories();
          setTimeout(() => this.closeModal(), 1200);
        },
        error: (err: any) => {
          this.errorMessage = err.error?.message || 'Failed to create category.';
          this.isSubmitting = false;
        },
      });
    }
  }

  deleteCategory(id: number): void {
    if (confirm('Are you sure you want to delete this category?')) {
      this.categoryService.deleteCategory(id).subscribe({
        next: () => {
          this.loadCategories();
        },
        error: (err: any) => {
          console.error('Error deleting category', err);
        },
      });
    }
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
