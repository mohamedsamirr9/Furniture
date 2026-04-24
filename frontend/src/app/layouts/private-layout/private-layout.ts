import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Navbar } from '../../shared/components/navbar/navbar';
import { Footer } from '../../shared/components/footer/footer';

@Component({
  selector: 'app-private-layout',
  imports: [Navbar, Footer, RouterModule],
  templateUrl: './private-layout.html',
  styleUrl: './private-layout.css',
})
export class PrivateLayoutComponent {}