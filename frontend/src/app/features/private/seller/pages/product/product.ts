import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../../../../core/services/product.service';
import { CategoryService } from '../../../../../core/services/category.service';

import { ProductCreateUpdateDto } from '../../../../../core/models/product-create-update-dto.model';

import { TranslateModule } from '@ngx-translate/core';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-product',
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './product.html',
  styleUrl: './product.css',
})
export class Product implements OnInit {
  readonly maxImages = 5;
  products: any = [];
  categories: any[] = [];
  isLoading = true;
  showModal = false;
  isEditing = false;
  editingProductId: number | null = null;
  isSubmitting = false;
  isUploading = false;
  successMessage = '';
  errorMessage = '';

  productForm!: FormGroup;

  existingImages: string[] = [];
  selectedFiles: File[] = [];
  selectedPreviews: string[] = [];

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private fb: FormBuilder,
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProducts();
    this.loadCategories();
  }

  initForm(): void {
    this.productForm = this.fb.group({
      nameEn: ['', Validators.required],
      nameAr: [''],
      descriptionEn: ['', Validators.required],
      descriptionAr: [''],
      price: [0, [Validators.required, Validators.min(0.01)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],

      categoryId: [null, Validators.required],
      sellerId: [''],
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getSellerProducts({ page: 1, pageSize: 100 }).subscribe({
      next: (res: any) => {
        this.products = res.data || res;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error fetching products', err);
        this.isLoading = false;
      },
    });
  }

  loadCategories(): void {
    this.categoryService.getAllCategories(1, 100).subscribe({
      next: (res: any) => {
        this.categories = Array.isArray(res) ? res : res.items || [];
      },
      error: (err: any) => {
        console.error('Error fetching categories', err);
      },
    });
  }

  openAddModal(): void {
    this.isEditing = false;
    this.editingProductId = null;
    this.resetImages();
    this.productForm.reset({
      nameEn: '',
      nameAr: '',
      descriptionEn: '',
      descriptionAr: '',
      price: 0,
      stockQuantity: 0,

      categoryId: null,
      sellerId: '',
    });
    this.clearMessages();
    this.showModal = true;
  }

  openEditModal(product: any): void {
    this.isEditing = true;
    this.editingProductId = product.id;
    this.clearMessages();
    this.resetImages();

    // Fetch the full product details to populate form
    this.productService.getProductById(product.id).subscribe({
      next: (details: any) => {
        this.existingImages = (details.images || details.Images || []).slice(0, this.maxImages);
        this.productForm.patchValue({
          nameEn: details.nameEn,
          nameAr: details.nameAr || '',
          descriptionEn: details.descriptionEn || '',
          descriptionAr: details.descriptionAr || '',
          price: details.price,
          stockQuantity: details.stockQuantity,

          categoryId: details.categoryId || null,
          sellerId: details.sellerId || '',
        });
        this.showModal = true;
      },
      error: (err: any) => {
        console.error('Error fetching product details', err);
        // Fallback: use the list data
        this.productForm.patchValue({
          nameEn: product.nameEn || product.name,
          nameAr: product.nameAr || '',
          descriptionEn: product.descriptionEn || '',
          descriptionAr: product.descriptionAr || '',
          price: product.price,
          stockQuantity: product.stockQuantity || 0,

          categoryId: null,
          sellerId: '',
        });
        this.existingImages = product.mainImage ? [product.mainImage] : [];
        this.showModal = true;
      },
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.resetImages();
    this.clearMessages();
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files || []);
    if (files.length === 0) return;

    this.clearMessages();

    const allowedSlots = this.maxImages - (this.existingImages.length + this.selectedFiles.length);
    if (allowedSlots <= 0) {
      this.errorMessage = `You can upload at most ${this.maxImages} images.`;
      input.value = '';
      return;
    }

    const toAdd = files.slice(0, allowedSlots);
    for (const file of toAdd) {
      const validation = this.productService.validateImageFile(file);
      if (!validation.valid) {
        this.errorMessage = validation.error || 'Invalid image file';
        continue;
      }
      this.selectedFiles.push(file);
      this.selectedPreviews.push(URL.createObjectURL(file));
    }

    input.value = '';
  }

  removeExistingImage(url: string) {
    this.existingImages = this.existingImages.filter((x) => x !== url);
  }

  removeSelectedImage(index: number) {
    const preview = this.selectedPreviews[index];
    if (preview?.startsWith('blob:')) URL.revokeObjectURL(preview);
    this.selectedFiles.splice(index, 1);
    this.selectedPreviews.splice(index, 1);
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.clearMessages();

    const formValue = this.productForm.value;
    const submitWithUrls = (allImageUrls: string[]) => {
      if (allImageUrls.length > this.maxImages) {
        this.errorMessage = `You can upload at most ${this.maxImages} images.`;
        this.isSubmitting = false;
        return;
      }

      const dto: ProductCreateUpdateDto = {
        nameEn: formValue.nameEn,
        nameAr: formValue.nameAr,
        descriptionEn: formValue.descriptionEn,
        descriptionAr: formValue.descriptionAr,
        price: formValue.price,
        stockQuantity: formValue.stockQuantity,
        categoryId: formValue.categoryId,
        sellerId: formValue.sellerId,
        imageUrls: allImageUrls,
      };

      if (this.isEditing && this.editingProductId !== null) {
        this.productService.updateProduct(this.editingProductId, dto).subscribe({
          next: () => {
            this.successMessage = 'Product updated successfully!';
            this.isSubmitting = false;
            this.loadProducts();
            setTimeout(() => this.closeModal(), 1200);
          },
          error: (err: any) => {
            this.errorMessage = err.error?.message || 'Failed to update product.';
            this.isSubmitting = false;
          },
        });
      } else {
        this.productService.createProduct(dto).subscribe({
          next: () => {
            this.successMessage = 'Product created successfully!';
            this.isSubmitting = false;
            this.loadProducts();
            setTimeout(() => this.closeModal(), 1200);
          },
          error: (err: any) => {
            this.errorMessage = err.error?.message || 'Failed to create product.';
            this.isSubmitting = false;
          },
        });
      }
    };

    if (this.selectedFiles.length > 0) {
      this.isUploading = true;
      this.productService
        .uploadImages(this.selectedFiles)
        .pipe(finalize(() => (this.isUploading = false)))
        .subscribe({
          next: (uploadedUrls) => submitWithUrls([...this.existingImages, ...uploadedUrls]),
          error: (err) => {
            console.error('Image upload failed', err);
            this.errorMessage = 'Failed to upload images. Please try again.';
            this.isSubmitting = false;
          },
        });
      return;
    }

    submitWithUrls([...this.existingImages]);
  }

  private resetImages() {
    for (const p of this.selectedPreviews) {
      if (p?.startsWith('blob:')) URL.revokeObjectURL(p);
    }
    this.existingImages = [];
    this.selectedFiles = [];
    this.selectedPreviews = [];
  }

  get totalImagesCount(): number {
    return this.existingImages.length + this.selectedFiles.length;
  }

  deleteProduct(id: number): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productService.deleteProduct(id).subscribe({
        next: () => {
          this.loadProducts();
        },
        error: (err: any) => {
          console.error('Error deleting product', err);
        },
      });
    }
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}
