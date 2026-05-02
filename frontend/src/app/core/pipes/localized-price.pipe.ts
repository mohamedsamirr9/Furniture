import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

/** Same shape as Angular DecimalPipe `digitsInfo`: minIntegerDigits.minFraction-maxFraction (e.g. 1.2-2). */
function parseDigitsInfo(digitsInfo: string): {
  minimumIntegerDigits: number;
  minimumFractionDigits: number;
  maximumFractionDigits: number;
} {
  const m = digitsInfo.match(/^(\d+)\.(\d+)-(\d+)$/);
  if (!m) {
    return { minimumIntegerDigits: 1, minimumFractionDigits: 0, maximumFractionDigits: 2 };
  }
  const minimumIntegerDigits = Math.max(1, parseInt(m[1], 10));
  const minimumFractionDigits = parseInt(m[2], 10);
  const maximumFractionDigits = Math.max(minimumFractionDigits, parseInt(m[3], 10));
  return { minimumIntegerDigits, minimumFractionDigits, maximumFractionDigits };
}

/**
 * Formats amounts with en-US grouping/fractions (stable digits), then appends
 * COMMON.PRICE_CURRENCY from ngx-translate (e.g. EGP / جنيه).
 * Does not inject DecimalPipe so it works in standalone routes without extra providers.
 */
@Pipe({
  name: 'localizedPrice',
  standalone: true,
  pure: false,
})
export class LocalizedPricePipe implements PipeTransform {
  private readonly translate = inject(TranslateService);

  transform(value: number | string | null | undefined, digitsInfo = '1.2-2'): string {
    if (value === null || value === undefined || value === '') return '';
    const n = typeof value === 'string' ? Number(value) : value;
    if (Number.isNaN(n)) return '';
    const opts = parseDigitsInfo(digitsInfo);
    const formatted = new Intl.NumberFormat('en-US', {
      ...opts,
      useGrouping: true,
    }).format(n);
    const suffix = this.translate.instant('COMMON.PRICE_CURRENCY').trim();
    return suffix ? `${formatted} ${suffix}` : formatted;
  }
}
