export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  categoryId: string;
  stockQuantity: number;
  imageUrl?: string;
  sku: string;
  isLowStock: boolean;
  isOutOfStock: boolean;
  totalStockValue: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProductDto {
  name: string;
  description: string;
  price: number;
  categoryId: string;
  stockQuantity: number;
  imageUrl?: string;
}

export interface UpdateProductDto {
  name: string;
  description: string;
  price: number;
  categoryId: string;
  imageUrl?: string;
}

// ← ADICIONE ESTE TIPO
export interface PaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}