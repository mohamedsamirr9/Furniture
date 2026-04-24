export interface ProductQueryParams {
  categoryId?: number | null;
  search?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  sort?: string | null;
  page: number;
  pageSize: number;
}
