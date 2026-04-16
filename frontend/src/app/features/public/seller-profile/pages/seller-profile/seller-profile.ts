import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';

interface PortfolioItem {
  id: number;
  category: string;
  title: string;
  description: string;
  imageUrl: string;
}

interface Seller {
  id: number;
  name: string;
  location: string;
  joinDate: string;
  rating: number;
  reviewsCount: number;
  completedOrders: number;
  bio: string;
  avatarUrl: string;
  specialties: string[];
  portfolio: PortfolioItem[];
}

@Component({
  selector: 'app-seller-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './seller-profile.html',
  styleUrl: './seller-profile.css'
})
export class SellerProfileComponent implements OnInit {
  seller: Seller = {
    id: 1,
    name: 'Marcus Woodwell',
    location: 'Brooklyn, NY',
    joinDate: 'June 2023',
    rating: 4.9,
    reviewsCount: 87,
    completedOrders: 142,
    bio: 'Master craftsman with over 15 years of experience in bespoke furniture design. Specializing in solid wood pieces that blend Scandinavian minimalism with timeless craftsmanship. Every piece is handmade in my Brooklyn workshop using sustainably sourced materials.',
    avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop',
    specialties: ['Dining Tables', 'Bookshelves', 'Coffee Tables', 'Custom Cabinetry'],
    portfolio: [
      {
        id: 1,
        category: 'TABLES',
        title: 'Solid Oak Dining Table',
        description: '8-seater dining table crafted from sustainably sourced European oak with a natural oil finish.',
        imageUrl: 'https://images.unsplash.com/photo-1617806118233-18e1de247200?w=600&auto=format&fit=crop'
      },
      {
        id: 2,
        category: 'STORAGE',
        title: 'Floating Wall Shelves',
        description: 'Minimalist floating shelves with hidden brackets, available in oak, walnut, and ash.',
        imageUrl: 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&auto=format&fit=crop'
      },
      {
        id: 3,
        category: 'CHAIRS',
        title: 'Sage Velvet Armchair',
        description: 'Mid-century inspired armchair upholstered in premium sage green velvet with solid walnut legs.',
        imageUrl: 'https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=600&auto=format&fit=crop'
      },
      {
        id: 4,
        category: 'TABLES',
        title: 'Live Edge Coffee Table',
        description: 'Unique live edge walnut coffee table with hand-forged steel base. Each piece is one-of-a-kind.',
        imageUrl: 'https://images.unsplash.com/photo-1567016432779-094069958ea5?w=600&auto=format&fit=crop'
      },
      {
        id: 5,
        category: 'STORAGE',
        title: 'Custom Built-In Wardrobe',
        description: 'Floor-to-ceiling wardrobe system with sliding doors, fully customizable to your space.',
        imageUrl: 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&auto=format&fit=crop'
      },
      {
        id: 6,
        category: 'TABLES',
        title: 'Scandinavian Nightstand',
        description: 'Simple, elegant nightstand with single drawer and open shelf, handcrafted from solid beech.',
        imageUrl: 'https://images.unsplash.com/photo-1505693314120-0d443867891c?w=600&auto=format&fit=crop'
      },
    ]
  };

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
   
  }
}