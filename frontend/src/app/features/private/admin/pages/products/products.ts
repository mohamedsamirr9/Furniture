import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../../../../core/services/product.service';
import { CategoryService } from '../../../../../core/services/category.service';

import { ProductCreateUpdateDto } from '../../../../../core/models/product-create-update-dto.model';

@Component({
  selector: 'app-products',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products implements OnInit {
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
      sellerId: ['seller-1'],
      imageUrl: [''],
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getProducts({ page: 1, pageSize: 100 }).subscribe({
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
    this.productForm.reset({
      nameEn: '',
      nameAr: '',
      descriptionEn: '',
      descriptionAr: '',
      price: 0,
      stockQuantity: 0,

      categoryId: null,
      sellerId: 'seller-1',
      imageUrl: '',
    });
    this.clearMessages();
    this.showModal = true;
  }

  openEditModal(product: any): void {
    this.isEditing = true;
    this.editingProductId = product.id;
    this.clearMessages();

    // Fetch the full product details to populate form
    this.productService.getProductById(product.id).subscribe({
      next: (details: any) => {
        this.productForm.patchValue({
          nameEn: details.nameEn,
          nameAr: details.nameAr || '',
          descriptionEn: details.descriptionEn || '',
          descriptionAr: details.descriptionAr || '',
          price: details.price,
          stockQuantity: details.stockQuantity,

          categoryId: details.categoryId || null,
          sellerId: details.sellerId || 'seller-1',
          imageUrl: details.images && details.images.length > 0 ? details.images[0] : '',
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
          sellerId: 'seller-1',
          imageUrl: product.mainImage || '',
        });
        this.showModal = true;
      },
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.clearMessages();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.isUploading = true;
      this.clearMessages();
      this.productService.uploadImage(file).subscribe({
        next: (res: any) => {
          this.productForm.patchValue({ imageUrl: res.secure_url });
          this.isUploading = false;
          this.successMessage = 'Image uploaded successfully!';
        },
        error: (err: any) => {
          console.error('Image upload failed', err);
          this.errorMessage = 'Failed to upload image. Please try again.';
          this.isUploading = false;
        }
      });
    }
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.clearMessages();

    const formValue = this.productForm.value;
    const dto: ProductCreateUpdateDto = {
      nameEn: formValue.nameEn,
      nameAr: formValue.nameAr,
      descriptionEn: formValue.descriptionEn,
      descriptionAr: formValue.descriptionAr,
      price: formValue.price,
      stockQuantity: formValue.stockQuantity,
      categoryId: formValue.categoryId,
      sellerId: formValue.sellerId,
      imageUrls: formValue.imageUrl ? [formValue.imageUrl] : []
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
