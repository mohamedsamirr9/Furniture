import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CartService } from '../../../../../core/services/cart.service';
import { ProductService } from '../../../../../core/services/product.service';

@Component({
  selector: 'app-product-details',
  imports: [CommonModule, RouterModule],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails implements OnInit {
  product: any = null;
  isLoading: boolean = false;
  notFoundMessage: string = '';

  isAdding: boolean = false;
  addedSuccess: boolean = false;
  errorMessage: string = '';

  constructor(
    private cartService: CartService,
    private productService: ProductService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loadProduct(+id);
      } else {
        this.notFoundMessage = 'Invalid product ID.';
      }
    });
  }

  loadProduct(id: number) {
    this.isLoading = true;
    this.notFoundMessage = '';
    this.productService.getProductById(id).subscribe({
      next: (res: any) => {
        this.product = res;
        this.isLoading = false;
      },
      error: (err: any) => {
        this.isLoading = false;
        this.notFoundMessage = 'Product not found.';
        console.error(err);
      }
    });
  }

  addToCart() {
    if (!this.product) return;
    this.isAdding = true;
    this.addedSuccess = false;
    this.errorMessage = '';
    
    this.cartService.addToCart(this.product.id, 1).subscribe({
      next: () => {
        this.isAdding = false;
        this.addedSuccess = true;
        setTimeout(() => this.addedSuccess = false, 3000);
      },
      error: (err: any) => {
        this.isAdding = false;
        this.errorMessage = err.error?.error || 'Failed to add item to cart.';
        console.error(err);
      }
    });
  }
}
