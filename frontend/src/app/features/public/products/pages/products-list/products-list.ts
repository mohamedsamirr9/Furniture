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
  page = 1;
  offset = 0;
  limit = 10;
  searchTerm: string = '';
  selectedCategory: string = '';

  constructor(
    private productService: ProductService,
    private router: Router,
    public route: ActivatedRoute,
  ) {}
  ngOnInit() {
    this.loadCategories();
    this.route.queryParams.subscribe((params) => {
      if (!params['page']) {
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { page: 1, limit: 10 },
          queryParamsHandling: 'merge',
        });
        return;
      }
      this.page = Number(params['page']) || 1;
      this.limit = Number(params['limit']) || 10;
      const title = params['title'] || '';
      const categorySlug = params['categorySlug'] || '';

      this.offset = (this.page - 1) * this.limit;

      this.loadProducts(title, categorySlug);
    });
  }

  loadProducts(title: string = '', categorySlug: string = '') {
    this.productService
      .getProducts({
        offset: this.offset,
        limit: this.limit,
        title: title,
        categorySlug: categorySlug,
      })
      .subscribe({
        next: (res: any) => {
          this.products = res;
        },
        error: (err) => console.log(err),
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
        page: this.page + 1,
        limit: this.limit,
      },
      queryParamsHandling: 'merge',
    });
  }
  prevPage() {
    if (this.page > 1) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: {
          page: this.page - 1,
          limit: this.limit,
        },
        queryParamsHandling: 'merge',
      });
    }
  }
  search() {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        title: this.searchTerm,
        page: 1,
      },
      queryParamsHandling: 'merge',
    });
  }
  filterByCategory(slug: string) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        categorySlug: slug,
        page: 1,
      },
      queryParamsHandling: 'merge',
    });
  }
  clearCategory() {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        categorySlug: null,
        page: 1,
      },
      queryParamsHandling: 'merge',
    });
  }
}
