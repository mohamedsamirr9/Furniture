import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../../../../core/services/auth.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  providers: [DatePipe],
  templateUrl: './users.html',
  styleUrl: './users.css',
})
export class Users implements OnInit {
  users: any[] = [];
  isLoading = true;

  constructor(private authService: AuthService, private datePipe: DatePipe) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.authService.getAllUsers().subscribe({
      next: (res: any[]) => {
        this.users = res.map((user: any) => ({
          name: user.name,
          email: user.email,
          phone: user.phone || '—',
          address: user.address || '—',
          role: user.role,
          joined: this.datePipe.transform(user.joinDate, 'yyyy-MM-dd') || '—',
          accountStatus: user.accountStatus
        }));
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Error fetching users', err);
        this.isLoading = false;
      }
    });
  }
}
