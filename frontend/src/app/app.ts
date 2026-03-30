import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './shared/components/navbar/navbar';
import { Footer } from './shared/components/footer/footer';
import { Contactus } from './features/public/home/components/contactus/contactus';
import { Aboutus } from './features/public/home/components/aboutus/aboutus';
import { PublicLayout } from './layouts/public-layout/public-layout/public-layout';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, PublicLayout],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('Furniture');
}
