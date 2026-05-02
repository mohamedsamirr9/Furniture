import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CustomRequestService } from '../../../../../core/services/custom-request.service';
import { NgIf } from '@angular/common';
import { LocalizedPricePipe } from '../../../../../core/pipes/localized-price.pipe';

@Component({
  selector: 'app-custom-request-details',
  templateUrl: './custom-request-details.html',
  styleUrls: ['./custom-request-details.css'],
  imports: [NgIf, LocalizedPricePipe],
})
export class CustomRequestDetailsComponent implements OnInit {
  request: any = null;
  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private customRequestService: CustomRequestService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      this.loadRequest(id);
    });
  }

  private loadRequest(id: number): void {
    this.isLoading = true;
    this.request = null;

    this.customRequestService.getRequestById(id).subscribe({
      next: (data) => {
        this.request = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }
}