import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-footer',
  imports: [RouterModule, TranslateModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {

}
