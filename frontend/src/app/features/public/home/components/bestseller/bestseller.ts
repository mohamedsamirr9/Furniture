import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-bestseller',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './bestseller.html',
  styleUrl: './bestseller.css',
})
export class Bestseller {

}
