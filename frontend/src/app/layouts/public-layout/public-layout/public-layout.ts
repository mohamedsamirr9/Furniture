import { Component } from '@angular/core';
import { Navbar } from '../../../shared/components/navbar/navbar';
import { Footer } from '../../../shared/components/footer/footer';
import { Home } from '../../../features/public/home/pages/home/home';

@Component({
  selector: 'app-public-layout',
  imports: [Navbar, Footer, Home],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.css',
})
export class PublicLayout {}
