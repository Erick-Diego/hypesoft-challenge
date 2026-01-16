import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { productService } from '@/services/productService';
import { categoryService } from '@/services/categoryService';
import { 
  TrendingUp, 
  Package, 
  DollarSign, 
  AlertTriangle,
  Filter,
  TrendingDown,
  BarChart3,
} from 'lucide-react';
import { 
  BarChart, 
  Bar, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import type { Product } from '@/types/product';
import type { Category } from '@/types/category';

export default function Dashboard() {
  const [timeRange, setTimeRange] = useState('30days');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  const { data: allProducts = [], isLoading: loadingProducts } = useQuery<Product[]>({
    queryKey: ['dashboard-products'],
    queryFn: async () => {
      const data = await productService.getPaged(1, 1000);
      return data.items || [];
    },
  });

  const { data: categories = [] } = useQuery<Category[]>({
    queryKey: ['categories'],
    queryFn: categoryService.getAll,
  });

  const stats = {
    totalProducts: allProducts.length,
    totalStockValue: allProducts.reduce((sum, p) => sum + (p.price * p.stockQuantity), 0),
    lowStockProducts: allProducts.filter(p => p.isLowStock && !p.isOutOfStock).length,
    outOfStockProducts: allProducts.filter(p => p.isOutOfStock).length,
    inStockProducts: allProducts.filter(p => !p.isLowStock && !p.isOutOfStock).length,
    averagePrice: allProducts.length > 0 
      ? allProducts.reduce((sum, p) => sum + p.price, 0) / allProducts.length 
      : 0,
    totalUnits: allProducts.reduce((sum, p) => sum + p.stockQuantity, 0),
  };

  const productsByCategory = categories.map(cat => {
    const categoryProducts = allProducts.filter(p => p.categoryId === cat.id);
    return {
      name: cat.name,
      count: categoryProducts.length,
      value: categoryProducts.reduce((sum, p) => sum + (p.price * p.stockQuantity), 0),
      stock: categoryProducts.reduce((sum, p) => sum + p.stockQuantity, 0),
    };
  }).filter(cat => cat.count > 0);

  const stockDistribution = [
    { name: 'Em Estoque', value: stats.inStockProducts, color: '#10b981' },
    { name: 'Estoque Baixo', value: stats.lowStockProducts, color: '#f59e0b' },
    { name: 'Sem Estoque', value: stats.outOfStockProducts, color: '#ef4444' },
  ].filter(item => item.value > 0);

  const topProducts = [...allProducts]
    .sort((a, b) => (b.price * b.stockQuantity) - (a.price * a.stockQuantity))
    .slice(0, 5);

  const lowStockItems = allProducts
    .filter(p => p.isLowStock || p.isOutOfStock)
    .sort((a, b) => a.stockQuantity - b.stockQuantity)
    .slice(0, 5);

  const priceRanges = [
    { range: 'R$ 0-50', min: 0, max: 50 },
    { range: 'R$ 50-100', min: 50, max: 100 },
    { range: 'R$ 100-200', min: 100, max: 200 },
    { range: 'R$ 200-500', min: 200, max: 500 },
    { range: 'R$ 500+', min: 500, max: Infinity },
  ];

  const priceDistribution = priceRanges.map(range => ({
    name: range.range,
    count: allProducts.filter(p => p.price >= range.min && p.price < range.max).length,
  })).filter(item => item.count > 0);

  if (loadingProducts) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-sm text-gray-500 mt-1">Visão geral do sistema de gestão de produtos</p>
        </div>
        <div className="flex items-center gap-3">
          <select
            value={timeRange}
            onChange={(e) => setTimeRange(e.target.value)}
            className="px-4 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
          >
            <option value="7days">Últimos 7 dias</option>
            <option value="30days">Últimos 30 dias</option>
            <option value="90days">Últimos 90 dias</option>
            <option value="year">Último ano</option>
          </select>
          <button className="flex items-center gap-2 px-4 py-2 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
            <Filter className="w-4 h-4" />
            <span className="text-sm font-medium">Filtros</span>
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white p-6 rounded-xl border border-gray-200 hover:shadow-lg transition-shadow">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-indigo-50 rounded-lg">
                <Package className="w-5 h-5 text-indigo-600" />
              </div>
              <span className="text-sm font-medium text-gray-600">Total de Produtos</span>
            </div>
          </div>
          <h3 className="text-3xl font-bold text-gray-900 mb-2">{stats.totalProducts}</h3>
          <div className="flex items-center gap-2 text-sm">
            <span className="flex items-center text-gray-500">
              {stats.totalUnits} unidades em estoque
            </span>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200 hover:shadow-lg transition-shadow">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-green-50 rounded-lg">
                <DollarSign className="w-5 h-5 text-green-600" />
              </div>
              <span className="text-sm font-medium text-gray-600">Valor Total do Estoque</span>
            </div>
          </div>
          <h3 className="text-3xl font-bold text-gray-900 mb-2">
            R$ {stats.totalStockValue.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
          </h3>
          <div className="flex items-center gap-2 text-sm">
            <span className="text-gray-500">
              Preço médio: R$ {stats.averagePrice.toFixed(2)}
            </span>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200 hover:shadow-lg transition-shadow">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-yellow-50 rounded-lg">
                <AlertTriangle className="w-5 h-5 text-yellow-600" />
              </div>
              <span className="text-sm font-medium text-gray-600">Estoque Baixo</span>
            </div>
          </div>
          <h3 className="text-3xl font-bold text-gray-900 mb-2">{stats.lowStockProducts}</h3>
          <div className="flex items-center gap-2 text-sm">
            <span className="flex items-center text-yellow-600 font-medium">
              <AlertTriangle className="w-4 h-4 mr-1" />
              Requer atenção
            </span>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200 hover:shadow-lg transition-shadow">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-red-50 rounded-lg">
                <TrendingDown className="w-5 h-5 text-red-600" />
              </div>
              <span className="text-sm font-medium text-gray-600">Sem Estoque</span>
            </div>
          </div>
          <h3 className="text-3xl font-bold text-gray-900 mb-2">{stats.outOfStockProducts}</h3>
          <div className="flex items-center gap-2 text-sm">
            <span className="flex items-center text-red-600 font-medium">
              <TrendingDown className="w-4 h-4 mr-1" />
              Ação necessária
            </span>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-white p-6 rounded-xl border border-gray-200">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-indigo-50 rounded-lg">
                <BarChart3 className="w-5 h-5 text-indigo-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Produtos por Categoria</h3>
            </div>
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="px-3 py-1.5 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <option value="all">Todas</option>
              {categories.map(cat => (
                <option key={cat.id} value={cat.id}>{cat.name}</option>
              ))}
            </select>
          </div>

          {productsByCategory.length > 0 ? (
            <div className="h-80">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={productsByCategory}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis 
                    dataKey="name" 
                    stroke="#9ca3af" 
                    style={{ fontSize: '12px' }}
                    angle={-45}
                    textAnchor="end"
                    height={80}
                  />
                  <YAxis stroke="#9ca3af" style={{ fontSize: '12px' }} />
                  <Tooltip 
                    contentStyle={{ 
                      backgroundColor: 'white', 
                      border: '1px solid #e5e7eb',
                      borderRadius: '8px'
                    }}
                    formatter={(value: unknown, name?: string) => {
                        const key = name || '';
                        const stringValue =
                            key === 'value'
                            ? `R$ ${Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`
                            : String(value);

                        if (key === 'value') return [stringValue, 'Valor'];
                        if (key === 'count') return [stringValue, 'Produtos'];
                        if (key === 'stock') return [stringValue, 'Unidades'];
                        return [stringValue, key];
                    }}
                  />
                  <Bar dataKey="count" fill="#6366f1" radius={[8, 8, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <div className="flex items-center justify-center h-80 text-gray-500">
              Nenhum dado disponível
            </div>
          )}
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-purple-50 rounded-lg">
                <Package className="w-5 h-5 text-purple-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Status do Estoque</h3>
            </div>
          </div>

          {stockDistribution.length > 0 ? (
            <>
              <div className="h-48 flex items-center justify-center">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={stockDistribution}
                      cx="50%"
                      cy="50%"
                      innerRadius={50}
                      outerRadius={80}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {stockDistribution.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </div>

              <div className="space-y-3 mt-4">
                {stockDistribution.map((item, index) => (
                  <div key={index} className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <div 
                        className="w-3 h-3 rounded-full" 
                        style={{ backgroundColor: item.color }}
                      />
                      <span className="text-sm text-gray-600">{item.name}</span>
                    </div>
                    <span className="text-sm font-semibold text-gray-900">{item.value}</span>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <div className="flex items-center justify-center h-64 text-gray-500">
              Nenhum dado disponível
            </div>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-xl border border-gray-200">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-green-50 rounded-lg">
                <DollarSign className="w-5 h-5 text-green-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Distribuição de Preços</h3>
            </div>
          </div>

          {priceDistribution.length > 0 ? (
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={priceDistribution} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis type="number" stroke="#9ca3af" style={{ fontSize: '12px' }} />
                  <YAxis 
                    dataKey="name" 
                    type="category" 
                    stroke="#9ca3af" 
                    style={{ fontSize: '12px' }}
                    width={80}
                  />
                  <Tooltip />
                  <Bar dataKey="count" fill="#10b981" radius={[0, 8, 8, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <div className="flex items-center justify-center h-64 text-gray-500">
              Nenhum dado disponível
            </div>
          )}
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-blue-50 rounded-lg">
                <TrendingUp className="w-5 h-5 text-blue-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Valor por Categoria</h3>
            </div>
          </div>

          {productsByCategory.length > 0 ? (
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={productsByCategory}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis 
                    dataKey="name" 
                    stroke="#9ca3af" 
                    style={{ fontSize: '12px' }}
                    angle={-45}
                    textAnchor="end"
                    height={80}
                  />
                  <YAxis stroke="#9ca3af" style={{ fontSize: '12px' }} />
                  <Tooltip 
                    formatter={(value: unknown) => `R$ ${Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`}
                  />
                  <Bar dataKey="value" fill="#3b82f6" radius={[8, 8, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <div className="flex items-center justify-center h-64 text-gray-500">
              Nenhum dado disponível
            </div>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-xl border border-gray-200">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-indigo-50 rounded-lg">
                <TrendingUp className="w-5 h-5 text-indigo-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Top 5 Produtos por Valor</h3>
            </div>
          </div>

          <div className="space-y-4">
            {topProducts.length > 0 ? (
              topProducts.map((product, index) => (
                <div key={product.id} className="flex items-center gap-4">
                  <div className="flex-shrink-0 w-8 h-8 bg-indigo-50 rounded-lg flex items-center justify-center">
                    <span className="text-sm font-bold text-indigo-600">#{index + 1}</span>
                  </div>
                  <img
                    src={product.imageUrl || 'https://via.placeholder.com/48'}
                    alt={product.name}
                    className="w-12 h-12 rounded-lg object-cover"
                  />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-gray-900 truncate">{product.name}</p>
                    <p className="text-xs text-gray-500">{product.stockQuantity} unidades</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-bold text-gray-900">
                      R$ {(product.price * product.stockQuantity).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                    </p>
                    <p className="text-xs text-gray-500">R$ {product.price.toFixed(2)}/un</p>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8 text-gray-500">
                Nenhum produto disponível
              </div>
            )}
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-2">
              <div className="p-2 bg-yellow-50 rounded-lg">
                <AlertTriangle className="w-5 h-5 text-yellow-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Produtos com Estoque Baixo</h3>
            </div>
          </div>

          <div className="space-y-4">
            {lowStockItems.length > 0 ? (
              lowStockItems.map((product) => (
                <div key={product.id} className="flex items-center gap-4">
                  <img
                    src={product.imageUrl || 'https://via.placeholder.com/48'}
                    alt={product.name}
                    className="w-12 h-12 rounded-lg object-cover"
                  />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-gray-900 truncate">{product.name}</p>
                    <p className="text-xs text-gray-500">SKU: {product.sku}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-bold text-gray-900">{product.stockQuantity}</p>
                    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                      product.isOutOfStock 
                        ? 'bg-red-100 text-red-800' 
                        : 'bg-yellow-100 text-yellow-800'
                    }`}>
                      {product.isOutOfStock ? 'Sem estoque' : 'Baixo'}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8 text-gray-500">
                Nenhum produto com estoque baixo
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}