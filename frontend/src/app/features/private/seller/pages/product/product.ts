import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../../../../core/services/product.service';
import { CategoryService } from '../../../../../core/services/category.service';

interface ProductCreateUpdateDto {
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  isAvailable: boolean;
  isCustomized: boolean;
  categoryId: number;
  sellerId: string;
}

@Component({
  selector: 'app-product',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product.html',
  styleUrl: './product.css',
})
export class Product implements OnInit {
  products: any = [];
  categories: any[] = [];
  isLoading = true;
  showModal = false;
  isEditing = false;
  editingProductId: number | null = null;
  isSubmitting = false;
  successMessage = '';
  errorMessage = '';

  productForm!: FormGroup;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProducts();
    this.loadCategories();
  }

  initForm(): void {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      price: [0, [Validators.required, Validators.min(0.01)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      isAvailable: [true],
      isCustomized: [false],
      categoryId: [null, Validators.required],
      sellerId: ['seller-1'],
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getProducts({ pageIndex: 1, pageSize: 100 }).subscribe({
      next: (res: any) => {
        this.products = res;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error fetching products', err);
        this.isLoading = false;
      }
    });
  }

  loadCategories(): void {
    this.categoryService.getAllCategories(1, 100).subscribe({
      next: (res: any) => {
        this.categories = Array.isArray(res) ? res : (res.items || []);
      },
      error: (err: any) => {
        console.error('Error fetching categories', err);
      }
    });
  }

  openAddModal(): void {
    this.isEditing = false;
    this.editingProductId = null;
    this.productForm.reset({
      name: '',
      description: '',
      price: 0,
      stockQuantity: 0,
      isAvailable: true,
      isCustomized: false,
      categoryId: null,
      sellerId: 'seller-1',
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
          name: details.name,
          description: details.description || '',
          price: details.price,
          stockQuantity: details.stockQuantity,
          isAvailable: details.isAvailable,
          isCustomized: details.isCustomized,
          categoryId: details.categoryId || null,
          sellerId: details.sellerId || 'seller-1',
        });
        this.showModal = true;
      },
      error: (err: any) => {
        console.error('Error fetching product details', err);
        // Fallback: use the list data
        this.productForm.patchValue({
          name: product.name,
          description: '',
          price: product.price,
          stockQuantity: product.stockQuantity || 0,
          isAvailable: true,
          isCustomized: false,
          categoryId: null,
          sellerId: 'seller-1',
        });
        this.showModal = true;
      }
    });
  }

  closeModal(): void {
    this.showModal = false;
    this.clearMessages();
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.clearMessages();

    const dto: ProductCreateUpdateDto = this.productForm.value;

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
        }
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
        }
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
        }
      });
    }
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}