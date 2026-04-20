import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-image-preview-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-preview-modal.html',
  styleUrl: './image-preview-modal.css',
})
export class ImagePreviewModalComponent {
  @Input() imageUrl: string | null = null;
  @Output() close = new EventEmitter<void>();

  onClose() {
    this.close.emit();
  }
}
