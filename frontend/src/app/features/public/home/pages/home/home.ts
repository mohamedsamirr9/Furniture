import { Component } from '@angular/core';
import { Aboutus } from '../../components/aboutus/aboutus';
import { Contactus } from '../../components/contactus/contactus';
import { Bestseller } from '../../components/bestseller/bestseller';
import { Specialoffers } from '../../components/specialoffers/specialoffers';
import { Hero } from '../../components/hero/hero';
import { Categories } from '../../components/categories/categories';

@Component({
  selector: 'app-home',
  imports: [Aboutus, Contactus, Bestseller, Specialoffers, Hero, Categories],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {}
