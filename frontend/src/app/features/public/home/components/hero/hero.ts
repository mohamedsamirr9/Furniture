import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [RouterModule, TranslateModule],
  templateUrl: './hero.html',
  styleUrl: './hero.css',
})
export class Hero {

}
