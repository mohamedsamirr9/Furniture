import { Component, OnInit, OnDestroy } from '@angular/core';
import { ProductService } from '../../../../../core/services/product.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ProductQueryParams } from '../../../../../core/models/product-query-params.model';

@Component({
  selector: 'app-products-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './products-list.html',
  styleUrl: './products-list.css',
})
export class ProductsList implements OnInit, OnDestroy {
  products: any[] = [];
  categories: any[] = [];

  // Template-bound state (restored from query params)
  searchTerm = '';
  selectedCategoryId: number | null = null;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sortOption: string = '';
  pageIndex = 1;
  pageSize = 10;
  totalCount = 0;

  loading = false;

  private searchSubject = new Subject<string>();
  private subscriptions: Subscription[] = [];

  constructor(
    private productService: ProductService,
    private router: Router,
    public route: ActivatedRoute,
  ) {}

  ngOnInit() {
    this.loadCategories();

    // Debounced search input
    this.subscriptions.push(
      this.searchSubject
        .pipe(debounceTime(400), distinctUntilChanged())
        .subscribe((value) => {
          this.updateQueryParams({ search: value || null, page: 1 });
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

        this.loadProducts();
      })
    );
  }

  ngOnDestroy() {
    this.subscriptions.forEach((s) => s.unsubscribe());
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

  // --- Search ---
  onSearchInput(value: string) {
    this.searchTerm = value;
    this.searchSubject.next(value);
  }

  search() {
    this.updateQueryParams({ search: this.searchTerm || null, page: 1 });
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
        const data = res.data || res;
        this.products = data.map((product: any) => ({
          ...product,
          averageRating: product.reviews && product.reviews.length > 0
            ? product.reviews.reduce((sum: number, r: any) => sum + r.rating, 0) / product.reviews.length
            : 0
        }));
        this.totalCount = res.totalCount || 0;
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
