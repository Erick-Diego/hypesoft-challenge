import api from './api';
import type { Category, CreateCategoryDto, UpdateCategoryDto } from '@/types/category';
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

export const categoryService = {
  getAll: async (): Promise<Category[]> => {
    try {
      const response = await api.get<Category[]>('/categories');
      return response.data;
    } catch (error) {
      console.error('Error fetching categories:', error);
      return [];
    }
  },

  getById: async (id: string): Promise<Category | null> => {
    try {
      const response = await api.get<Category>(`/categories/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching category:', error);
      return null;
    }
  },

  create: async (data: CreateCategoryDto): Promise<Category> => {
    try {
      const response = await api.post<Category>('/categories', data);
      return response.data;
    } catch (error) {
      handleError(error, 'Error creating category');
      throw error;
    }
  },

  update: async (id: string, data: UpdateCategoryDto): Promise<Category> => {
    try {
      const response = await api.put<Category>(`/categories/${id}`, data);
      return response.data;
    } catch (error) {
      handleError(error, 'Error updating category');
      throw error;
    }
  },

  delete: async (id: string): Promise<void> => {
    try {
      await api.delete(`/categories/${id}`);
    } catch (error) {
      handleError(error, 'Error deleting category');
      throw error;
    }
  },
};