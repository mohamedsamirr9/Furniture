import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OfferService } from '../../../../../core/services/offer.service';

@Component({
  selector: 'app-compare-offers',
  imports: [CommonModule],
  templateUrl: './compare-offers.html',
  styleUrl: './compare-offers.css',
})
export class CompareOffers implements OnInit {
  offers: any[] = [];
  requestId: number = 0;
  isLoading = false;
  acceptSuccess = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private offerService: OfferService
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.requestId = +id;
        this.loadOffers();
      }
    });
  }

  loadOffers(showLoading: boolean = true) {
    if (showLoading) this.isLoading = true;
    this.offerService.getOffersByRequest(this.requestId).subscribe({
      next: (res) => {
        this.offers = res;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Failed to load offers.';
        this.isLoading = false;
      }
    });
  }

  get isAnyOfferAccepted(): boolean {
    return this.offers.some(o => o.status === 1); // 1 = Accepted
  }

  goBack() {
    this.router.navigate(['/customer']);
  }

  acceptOffer(offer: any) {
    if (confirm('Are you sure you want to accept this offer? This will decline other offers.')) {
      this.offerService.acceptOffer(offer.id).subscribe({
        next: () => {
          this.acceptSuccess = true;
          // Immediate local update for instant feedback
          this.offers.forEach(o => {
            if (o.id === offer.id) {
              o.status = 1;
            } else {
              o.status = 2;
            }
          });
          
          this.errorMessage = ''; // Clear any previous errors
          
          // Re-fetch in background
          this.loadOffers(false);
          
          setTimeout(() => {
            this.router.navigate(['/checkout'], { queryParams: { offerId: offer.id } });
          }, 1500);
        },
        error: (err) => {
          console.error(err);
          this.errorMessage = err.error?.message || 'Failed to accept offer.';
        }
      });
    }
  }
}