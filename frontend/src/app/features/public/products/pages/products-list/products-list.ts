import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ProductService, ImageSearchResult } from '../../../../../core/services/product.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ProductQueryParams } from '../../../../../core/models/product-query-params.model';

import { TranslateModule } from '@ngx-translate/core';

type SearchMode = 'text' | 'image';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, TranslateModule],
  templateUrl: './products-list.html',
  styleUrl: './products-list.css',
})
export class ProductsList implements OnInit, OnDestroy {
  private productService = inject(ProductService);
  private router = inject(Router);
  public route = inject(ActivatedRoute);

  // Products data
  products: any[] = [];
  categories: any[] = [];

  // Query params state
  searchTerm = '';
  selectedCategoryId: number | null = null;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sortOption = '';
  pageIndex = 1;
  pageSize = 10;
  totalCount = 0;

  loading = false;

  // ======= IMAGE SEARCH STATE =======
  searchMode: SearchMode = 'text';
  selectedImage: File | null = null;
  imagePreview: string | null = null;
  imageSearchResults: ImageSearchResult[] = [];
  imageSearchLoading = false;
  imageSearchError: string | null = null;
  imageSearched = false;
  isDragOver = false;
  // =================================

  private searchSubject = new Subject<string>();
  private subscriptions: Subscription[] = [];

  constructor() {}

  ngOnInit() {
    this.loadCategories();

    // Debounced search input
    this.subscriptions.push(
      this.searchSubject
        .pipe(debounceTime(400), distinctUntilChanged())
        .subscribe((value) => {
          // Only trigger text search, not image search
          if (this.searchMode === 'text') {
            this.updateQueryParams({ search: value || null, page: 1 });
          }
        })
    );

    // Single source of truth: query params drive everything
    this.subscriptions.push(
      this.route.queryParams.subscribe((params) => {
        this.pageIndex = +params['page'] || 1;
        this.pageSize = +params['pageSize'] || 10;
        this.searchTerm = params['search'] || '';
        this.selectedCategoryId = params['categoryId'] ? +params['categoryId'] : null;
        this.minPrice = params['minPrice'] ? +params['minPrice'] : null;
        this.maxPrice = params['maxPrice'] ? +params['maxPrice'] : null;
        this.sortOption = params['sort'] || '';

        // Only load products if in text search mode
        if (this.searchMode === 'text') {
          this.loadProducts();
        }
      })
    );
  }

  ngOnDestroy() {
    this.subscriptions.forEach((s) => s.unsubscribe());
    // Clean up object URL to prevent memory leaks
    if (this.imagePreview && this.imagePreview.startsWith('blob:')) {
      URL.revokeObjectURL(this.imagePreview);
    }
  }

  /** Navigate with merged query params, stripping null/empty values */
  private updateQueryParams(params: { [key: string]: any }) {
    const cleaned: { [key: string]: any } = {};
    for (const key of Object.keys(params)) {
      const val = params[key];
      cleaned[key] = val !== null && val !== undefined && val !== '' ? val : null;
    }
    this.router.navigate(['/products'], {
      queryParams: cleaned,
      queryParamsHandling: 'merge',
    });
  }

  // --- Search Type Toggle ---
  switchToTextSearch() {
    this.searchMode = 'text';
    this.clearImageSearch();
    // Restore text search from query params
    this.loadProducts();
  }

  switchToImageSearch() {
    this.searchMode = 'image';
    // Clear text search results when switching to image mode
    this.products = [];
    this.totalCount = 0;
  }

  // --- Text Search ---
  onSearchInput(value: string) {
    this.searchTerm = value;
    if (this.searchMode === 'text') {
      this.searchSubject.next(value);
    }
  }

  search() {
    if (this.searchMode === 'text') {
      this.updateQueryParams({ search: this.searchTerm || null, page: 1 });
    }
  }

  // --- Image Search Handlers ---
  onImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.handleImageFile(file);
  }

  onImageDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = true;
  }

  onImageDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
    const files = event.dataTransfer?.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    this.handleImageFile(file);
  }

  private handleImageFile(file: File) {
    // Validate
    const validation = this.productService.validateImageFile(file);
    if (!validation.valid) {
      this.imageSearchError = validation.error || 'Invalid file';
      return;
    }

    this.imageSearchError = null;
    this.selectedImage = file;
    
    // Create preview
    this.productService.fileToBase64(file).then((base64) => {
      this.imagePreview = base64;
    }).catch(() => {
      this.imagePreview = null;
    });

    // Clear previous results
    this.imageSearchResults = [];
    this.imageSearched = false;
  }

  clearImageSearch() {
    this.selectedImage = null;
    this.imagePreview = null;
    this.imageSearchResults = [];
    this.imageSearchError = null;
    this.imageSearched = false;
    this.isDragOver = false;
  }

  searchByImage() {
    if (!this.selectedImage) return;

    this.imageSearchLoading = true;
    this.imageSearchError = null;
    this.imageSearchResults = [];

    this.productService.searchByImage(this.selectedImage, 10).subscribe({
      next: (response: any) => {
        // Handle both wrapped response {success, message, data} and direct array
        if (Array.isArray(response)) {
          // Direct array response from backend
          this.imageSearchResults = response;
        } else if (response.success && response.data) {
          // Wrapped response format
          this.imageSearchResults = response.data;
        } else if (response.data) {
          // Has data but different format
          this.imageSearchResults = response.data;
        } else {
          this.imageSearchError = 'Search failed';
        }
        this.imageSearchLoading = false;
        this.imageSearched = true;
      },
      error: (err) => {
        console.error('Image search error:', err);
        this.imageSearchError = 'Failed to search. Please try again.';
        this.imageSearchLoading = false;
        this.imageSearched = true;
      },
    });
  }

  // --- Category ---
  filterByCategory(id: number) {
    this.updateQueryParams({ categoryId: id, page: 1 });
  }

  clearCategory() {
    this.updateQueryParams({ categoryId: null, page: 1 });
  }

  // --- Price ---
  applyPriceFilter() {
    this.updateQueryParams({
      minPrice: this.minPrice,
      maxPrice: this.maxPrice,
      page: 1,
    });
  }

  clearPriceFilter() {
    this.minPrice = null;
    this.maxPrice = null;
    this.updateQueryParams({ minPrice: null, maxPrice: null, page: 1 });
  }

  // --- Sort ---
  onSortChange(sort: string) {
    this.updateQueryParams({ sort: sort || null, page: 1 });
  }

  // --- Pagination ---
  nextPage() {
    if (this.pageIndex * this.pageSize < this.totalCount) {
      this.updateQueryParams({ page: this.pageIndex + 1 });
    }
  }

  prevPage() {
    if (this.pageIndex > 1) {
      this.updateQueryParams({ page: this.pageIndex - 1 });
    }
  }

  // --- Data Loading ---
  loadProducts() {
    this.loading = true;

    const filters: ProductQueryParams = {
      page: this.pageIndex,
      pageSize: this.pageSize,
      search: this.searchTerm || null,
      categoryId: this.selectedCategoryId,
      minPrice: this.minPrice,
      maxPrice: this.maxPrice,
      sort: this.sortOption || null,
    };

    this.productService.getProducts(filters).subscribe({
      next: (res: any) => {
        const data = res.Data || res.data || res;
        this.products = data.map((product: any) => ({
          ...product,
          averageRating: product.reviews && product.reviews.length > 0
            ? product.reviews.reduce((sum: number, r: any) => sum + r.rating, 0) / product.reviews.length
            : 0
        }));
        this.totalCount = res.TotalCount || res.totalCount || 0;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      },
    });
  }

  loadCategories() {
    this.productService.getCategories().subscribe({
      next: (res) => (this.categories = res),
      error: (err) => console.error(err),
    });
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize) || 1;
  }
}
