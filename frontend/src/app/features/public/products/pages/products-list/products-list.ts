import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../../../../core/services/product.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-products-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './products-list.html',
  styleUrl: './products-list.css',
})
export class ProductsList implements OnInit {
  products: any[] = [];
  categories: any[] = [];
  pageIndex = 1;
  offset = 0;
  pageSize = 10;
  searchTerm: string = '';
  selectedCategory: string = '';
  selectedCategoryId: number | null = null;
  loading = false;
  constructor(
    private productService: ProductService,
    private router: Router,
    public route: ActivatedRoute,
  ) {}
  ngOnInit() {
    this.loadCategories();

    this.route.paramMap.subscribe((params) => {
      const categoryId = params.get('id');

      this.selectedCategoryId = categoryId ? +categoryId : null;

      this.loadCategoryProducts(Number(this.selectedCategoryId));
    });

    this.route.queryParams.subscribe((params) => {
      this.pageIndex = +params['pageIndex'] || 1;
      this.pageSize = +params['pageSize'] || 10;
      this.searchTerm = params['title'] || '';

      this.loadProducts();
    });
  }

  loadProducts() {
    this.loading = true;

    this.productService
      .getProducts({
        pageIndex: this.pageIndex,
        pageSize: this.pageSize,
        search: this.searchTerm,
        categoryId: this.selectedCategory ? Number(this.selectedCategory) : null,
      })
      .subscribe({
        next: (res: any) => {
          this.products = res;
          this.loading = false;
        },
        error: (err) => {
          console.log(err);
          this.loading = false;
        },
      });
  }
  loadCategories() {
    this.productService.getCategories().subscribe({
      next: (res) => {
        console.log(res);
        this.categories = res;
        console.log(res);
      },
      error: (err) => console.log(err),
    });
  }

  nextPage() {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        pageIndex: this.pageIndex + 1,
        pageSize: this.pageSize,
      },
      queryParamsHandling: 'merge',
    });
  }
  prevPage() {
    if (this.pageIndex > 1) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: {
          pageIndex: this.pageIndex - 1,
          pageSize: this.pageSize,
        },
        queryParamsHandling: 'merge',
      });
    }
  }
  search() {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        page: 1,
        limit: this.pageSize,
        title: this.searchTerm,
      },
      queryParamsHandling: 'merge',
    });
  }
  filterByCategory(id: number) {
    this.router.navigate(['/categories', id, 'products']);
  }
  loadCategoryProducts(id: number) {
    this.loading = true;
      if(id == 0){
        return;
      }
    this.productService.getProductsByCategory(id).subscribe({

      next: (res: any) => {
        this.products = res.products;
        this.loading = false;
      },
      error: (err) => {
        console.log(err);
        this.loading = false;
      },
    });
  }
  clearCategory() {
    this.selectedCategoryId = null;
    this.router.navigate(['/products']);

    this.loadProducts();
  }
}
