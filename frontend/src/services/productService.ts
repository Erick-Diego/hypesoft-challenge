import api from './api';
import type { Product, CreateProductDto, UpdateProductDto, PaginatedResult } from '@/types/product';
import { AxiosError } from 'axios';

const handleError = (error: unknown, context: string) => {
  console.error(`${context}:`, error);

  if (error instanceof AxiosError) {
    const message = error.response?.data?.message || error.response?.data?.title || 'Erro ao processar requisição';
    throw new Error(message);
  } else if (error instanceof Error) {
    throw new Error(error.message);
  } else {
    throw new Error('Erro inesperado. Tente novamente.');
  }
};

export const productService = {
  getPaged: async (page: number = 1, pageSize: number = 10): Promise<PaginatedResult<Product>> => {
    try {
      const response = await api.get<PaginatedResult<Product>>('/Products/paged', {
        params: { page, pageSize }
      });
      return response.data;
    } catch (error) {
      handleError(error, 'Error fetching products');
      throw error;
    }
  },

  search: async (term: string): Promise<Product[]> => {
    try {
      if (!term.trim()) return [];
      
      const response = await api.get<Product[]>('/Products/search', {
        params: { term }
      });
      return response.data;
    } catch (error) {
      console.error('Error searching products:', error);
      return [];
    }
  },

  create: async (data: CreateProductDto): Promise<Product> => {
    try {
      const response = await api.post<Product>('/Products', data);
      return response.data;
    } catch (error) {
      handleError(error, 'Error creating product');
      throw error;
    }
  },

  update: async (id: string, data: UpdateProductDto): Promise<Product> => {
    try {
      const response = await api.put<Product>(`/Products/${id}`, data);
      return response.data;
    } catch (error) {
      handleError(error, 'Error updating product');
      throw error;
    }
  },

  updateStock: async (id: string, newStockValue: number): Promise<Product> => {
    try {
      const response = await api.patch<Product>(`/Products/${id}/stock`, { quantity: newStockValue });
      return response.data;
    } catch (error) {
      handleError(error, 'Error updating stock');
      throw error;
    }
  },

  delete: async (id: string): Promise<void> => {
    try {
      await api.delete(`/Products/${id}`);
    } catch (error) {
      handleError(error, 'Error deleting product');
      throw error;
    }
  },
};