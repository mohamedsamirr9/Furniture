import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CustomRequestService } from '../../../../../core/services/custom-request.service';
import { OfferService } from '../../../../../core/services/offer.service';

@Component({
  selector: 'app-offers',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './offers.html',
  styleUrl: './offers.css',
})
export class Offers implements OnInit {
  requests: any[] = [];
  myOffers: any[] = [];
  
  isLoading = false;
  errorMessage = '';

  selectedRequestDetail: any | null = null;
  activeRequestId: number | null = null;
  offerForm: FormGroup;
  isSubmitting = false;
  submitSuccess = false;
  submitError = '';

  constructor(
    private customRequestService: CustomRequestService,
    private offerService: OfferService,
    private fb: FormBuilder
  ) {
    this.offerForm = this.fb.group({
      price: [null, [Validators.required, Validators.min(1)]],
      deliveryDays: [null, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.customRequestService.getAllRequests().subscribe({
      next: (res) => {
        // Backend returns paginated data: { data: [...], count: ... } or just array depending on the logic. 
        // Based on typical .NET pagination: let's handle both.
        this.requests = res.data || res;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Failed to load requests.';
        this.isLoading = false;
      }
    });

    this.offerService.getMyOffers().subscribe({
      next: (res) => {
        this.myOffers = res;
      },
      error: (err) => console.error('Failed to load my offers', err)
    });
  }

  openDetails(request: any) {
    this.selectedRequestDetail = request;
  }

  closeDetails() {
    this.selectedRequestDetail = null;
  }

  openOfferForm(requestId: number) {
    this.activeRequestId = requestId;
    this.offerForm.reset();
    this.submitSuccess = false;
    this.submitError = '';
  }

  cancelOffer() {
    this.activeRequestId = null;
    this.offerForm.reset();
  }

  submitOffer(requestId: number) {
    if (this.offerForm.invalid) {
      this.offerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.submitError = '';
    this.submitSuccess = false;

    const payload = {
      customRequestId: requestId,
      price: this.offerForm.value.price,
      deliveryDays: this.offerForm.value.deliveryDays
    };

    this.offerService.createOffer(payload).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.submitSuccess = true;
        this.loadData(); // reload to refresh 'myOffers'
        setTimeout(() => {
          this.activeRequestId = null;
          this.submitSuccess = false;
        }, 2000);
      },
      error: (err) => {
        console.error(err);
        this.submitError = err.error?.message || 'Failed to submit offer. Please try again.';
        this.isSubmitting = false;
      }
    });
  }
}