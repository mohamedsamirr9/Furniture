import { environment } from '../../../environments/environment';

/** Maps API-relative paths like `/images/foo.png` to full URL using API host. */
export function resolvePublicAssetUrl(path: string | null | undefined): string {
  if (!path) return '';
  if (path.startsWith('http')) return path;
  const base = environment.apiUrl.replace(/\/api\/?$/, '');
  return `${base}${path.startsWith('/') ? '' : '/'}${path}`;
}
