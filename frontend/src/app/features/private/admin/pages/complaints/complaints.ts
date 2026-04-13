import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-complaints',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './complaints.html',
  styleUrl: './complaints.css',
})
export class Complaints {
  complaints = [
    { id: 'C-001', user: 'Lisa P.', description: 'Damaged item received', date: '2026-03-25', status: 'Open' },
    { id: 'C-002', user: 'Mike D.', description: 'Wrong color delivered', date: '2026-03-22', status: 'In Progress' },
    { id: 'C-003', user: 'Anna S.', description: 'Late delivery', date: '2026-03-18', status: 'Resolved' },
  ];
}