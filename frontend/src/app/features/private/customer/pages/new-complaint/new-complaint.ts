import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-new-complaint',
  imports: [CommonModule, FormsModule],
  templateUrl: './new-complaint.html',
  styleUrl: './new-complaint.css',
})
export class NewComplaint {
  formData = {
    order: '',
    description: '',
  };

  constructor(private router: Router) {}

  onSubmit() {
    this.router.navigate(['/customer/complaints']);
  }
}