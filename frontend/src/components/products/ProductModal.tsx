import { Dialog } from '@headlessui/react';
import { X } from 'lucide-react';
import { useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { productService } from '@/services/productService';
import type { Product } from '@/types/product';
import type { Category } from '@/types/category';

const productSchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório'),
  description: z.string().min(1, 'Descrição é obrigatória'),
  price: z.number().min(0.01, 'Preço deve ser maior que zero'),
  categoryId: z.string().min(1, 'Categoria é obrigatória'),
  stockQuantity: z.number().int().min(0, 'Estoque não pode ser negativo'),
  imageUrl: z.string().url('URL inválida').optional().or(z.literal('')),
});

type ProductForm = z.infer<typeof productSchema>;

interface ProductModalProps {
  product: Product | null;
  categories: Category[];
  onClose: () => void;
}

export default function ProductModal({ product, categories, onClose }: ProductModalProps) {
  const queryClient = useQueryClient();
  const isEditing = !!product;

  const { register, handleSubmit, control, formState: { errors } } = useForm<ProductForm>({
    resolver: zodResolver(productSchema),
    defaultValues: product
      ? {
          name: product.name,
          description: product.description,
          price: product.price,
          categoryId: product.categoryId,
          stockQuantity: product.stockQuantity,
          imageUrl: product.imageUrl || '',
        }
      : {
          name: '',
          description: '',
          price: 0,
          categoryId: '',
          stockQuantity: 0,
          imageUrl: '',
        },
  });

  // OBSERVAÇÃO: useWatch é seguro e evita warnings do React Hook Form
  const currentStockValue = useWatch({
    control,
    name: 'stockQuantity',
    defaultValue: product?.stockQuantity ?? 0,
  });

  const mutation = useMutation({
    mutationFn: async (data: ProductForm) => {
      if (isEditing && product) {
        await productService.update(product.id, data);

        if (data.stockQuantity !== product.stockQuantity) {
          await productService.updateStock(product.id, data.stockQuantity);
        }
      } else {
        await productService.create(data);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      onClose();
    },
    onError: (error: unknown) => {
      console.error('Erro ao salvar produto:', error);
      alert(error instanceof Error ? error.message : 'Erro ao salvar produto. Tente novamente.');
    },
  });

  const onSubmit = (data: ProductForm) => {
    mutation.mutate(data);
  };

  return (
    <Dialog open={true} onClose={onClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />

      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Dialog.Panel className="mx-auto max-w-2xl w-full bg-white rounded-xl shadow-xl max-h-[90vh] overflow-y-auto">
          <div className="flex items-center justify-between p-6 border-b border-gray-200 sticky top-0 bg-white">
            <Dialog.Title className="text-xl font-semibold text-gray-900">
              {isEditing ? 'Editar Produto' : 'Novo Produto'}
            </Dialog.Title>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors"
              type="button"
            >
              <X className="w-6 h-6" />
            </button>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
            {/* Nome */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nome *</label>
              <input
                {...register('name')}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                placeholder="Ex: Notebook Dell Inspiron"
              />
              {errors.name && (
                <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
              )}
            </div>

            {/* Descrição */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Descrição *</label>
              <textarea
                {...register('description')}
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                placeholder="Descreva o produto..."
              />
              {errors.description && (
                <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>
              )}
            </div>

            {/* Preço e Estoque */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Preço (R$) *</label>
                <input
                  type="number"
                  step="0.01"
                  {...register('price', { valueAsNumber: true })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  placeholder="0.00"
                />
                {errors.price && (
                  <p className="mt-1 text-sm text-red-600">{errors.price.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Estoque *</label>
                <input
                  type="number"
                  min="0"
                  {...register('stockQuantity', {
                    valueAsNumber: true,
                    setValueAs: (v) => v === '' ? 0 : Math.max(0, parseInt(v)),
                  })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  placeholder="0"
                />
                {errors.stockQuantity && (
                  <p className="mt-1 text-sm text-red-600">{errors.stockQuantity.message}</p>
                )}
                {isEditing && product && currentStockValue !== product.stockQuantity && (
                  <p className="mt-1 text-xs text-blue-600">
                    Estoque será alterado de {product.stockQuantity} para {currentStockValue}
                  </p>
                )}
              </div>
            </div>

            {/* Categoria */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Categoria *</label>
              <select
                {...register('categoryId')}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              >
                <option value="">Selecione uma categoria</option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </select>
              {errors.categoryId && (
                <p className="mt-1 text-sm text-red-600">{errors.categoryId.message}</p>
              )}
            </div>

            {/* Imagem */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">URL da Imagem</label>
              <input
                {...register('imageUrl')}
                type="url"
                placeholder="https://exemplo.com/imagem.jpg"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              />
              {errors.imageUrl && (
                <p className="mt-1 text-sm text-red-600">{errors.imageUrl.message}</p>
              )}
              <p className="mt-1 text-xs text-gray-500">
                Opcional: URL de uma imagem do produto
              </p>
            </div>

            {/* Botões */}
            <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={mutation.isPending}
                className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {mutation.isPending ? 'Salvando...' : isEditing ? 'Salvar Alterações' : 'Criar Produto'}
              </button>
            </div>
          </form>
        </Dialog.Panel>
      </div>
    </Dialog>
  );
}
