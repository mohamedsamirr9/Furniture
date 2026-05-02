import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CustomRequestService } from '../../../../../core/services/custom-request.service';
import { TranslateModule } from '@ngx-translate/core';
import { LocalizedPricePipe } from '../../../../../core/pipes/localized-price.pipe';

@Component({
  selector: 'app-custom-request',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, LocalizedPricePipe],
  templateUrl: './custom-request.html',
  styleUrl: './custom-request.css',

})
export class CustomRequestComponent implements OnInit {
  requestForm: FormGroup;
  myRequests: any[] = [];
  
  selectedFile: File | null = null;
  imagePreviewUrl: string | null = null;
  
  isLoading = false;
  submitError = '';
  submitSuccess = false;

  constructor(
    private router: Router,
    private fb: FormBuilder,
    private customRequestService: CustomRequestService
  ) {
    this.requestForm = this.fb.group({
      description: ['', [Validators.required, Validators.minLength(10)]],
      budget: [null, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit() {
    this.loadMyRequests();
  }

  loadMyRequests() {
    this.customRequestService.getMyRequests().subscribe({
      next: (reqs) => this.myRequests = reqs,
      error: (err) => console.error('Failed to load requests', err)
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit() {
    if (this.requestForm.invalid) {
      this.requestForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.submitError = '';
    this.submitSuccess = false;

    const finalizeSubmission = (imageUrl: string | null) => {
      const payload = {
        Description: this.requestForm.value.description,
        Budget: this.requestForm.value.budget,
        ImageUrl: imageUrl
      };

      this.customRequestService.createCustomRequest(payload).subscribe({
        next: (res) => {
          this.isLoading = false;
          this.submitSuccess = true;
          this.requestForm.reset();
          this.selectedFile = null;
          this.imagePreviewUrl = null;
          this.loadMyRequests();
          setTimeout(() => {
            this.router.navigate(['/customer/success']);
          }, 1500);
        },
        error: (err) => {
          console.error(err);
          this.submitError = 'ALERTS.SUBMIT_ERROR';
          this.isLoading = false;
        }
      });
    };

    if (this.selectedFile) {
      this.customRequestService.uploadImage(this.selectedFile).subscribe({
        next: (res) => {
          finalizeSubmission(res.secure_url);
        },
        error: (err) => {
          console.error(err);
          this.submitError = 'ALERTS.UPLOAD_ERROR';
          this.isLoading = false;
        }
      });
    } else {
      finalizeSubmission(null);
    }
  }

  viewOffer(requestId: number) {
    this.router.navigate(['/customer/compare-offers', requestId]);
  }
}