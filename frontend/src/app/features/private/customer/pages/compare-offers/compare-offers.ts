import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OfferService } from '../../../../../core/services/offer.service';
import { ChatSignalRService } from '../../../../../core/services/chat-signalr.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LocalizedPricePipe } from '../../../../../core/pipes/localized-price.pipe';

@Component({
  selector: 'app-compare-offers',
  standalone: true,
  imports: [CommonModule, TranslateModule, LocalizedPricePipe],
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
    private offerService: OfferService,
    private chatService: ChatSignalRService,
    private translate: TranslateService
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
        this.errorMessage = 'ALERTS.ERROR';
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
    const confirmMsg = this.translate.instant('COMPARE_OFFERS.CONFIRM_ACCEPT');
    if (confirm(confirmMsg)) {
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

          // Start or get conversation with seller and open chat
          if (offer.sellerId) {
            this.chatService.startConversation({ otherUserId: offer.sellerId, firstMessage: `I accepted your offer #${offer.id}` })
              .subscribe({
                next: (conversation) => {
                  // Dispatch event to open chat
                  const event = new CustomEvent('openConversation', { detail: conversation, bubbles: true });
                  window.dispatchEvent(event);
                },
                error: (err) => console.error('Error starting conversation:', err)
              });
          }

          setTimeout(() => {
            this.router.navigate(['/checkout'], { queryParams: { offerId: offer.id } });
          }, 1500);
        },
        error: (err) => {
          console.error(err);
          this.errorMessage = err.error?.message || 'ALERTS.OFFER_ACCEPT_ERROR';
        }
      });
    }
  }
}