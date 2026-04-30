import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CustomRequestService } from '../../../../../core/services/custom-request.service';
import { NgIf, DatePipe, CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-custom-request-details',
  templateUrl: './custom-request-details.html',
  styleUrls: ['./custom-request-details.css'],
  imports: [NgIf, DatePipe, CurrencyPipe]
})
export class CustomRequestDetailsComponent implements OnInit {
  request: any = null;
  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private customRequestService: CustomRequestService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
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