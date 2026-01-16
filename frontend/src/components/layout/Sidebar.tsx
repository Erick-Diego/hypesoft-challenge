import { NavLink } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Package, 
  FolderOpen,
  Sparkles
} from 'lucide-react';

const menuItems = [
  { 
    title: 'GENERAL', 
    items: [
      { name: 'Dashboard', icon: LayoutDashboard, path: '/dashboard' },
    ]
  },
  { 
    title: 'SHOP', 
    items: [
      { name: 'Products', icon: Package, path: '/products' },
      { name: 'Categories', icon: FolderOpen, path: '/categories' },
    ]
  },
];

export default function Sidebar() {
  return (
    <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center">
            <Sparkles className="w-5 h-5 text-white" />
          </div>
          <span className="text-xl font-semibold">Hypesoft</span>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto p-4">
        {menuItems.map((section) => (
          <div key={section.title} className="mb-6">
            <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
              {section.title}
            </h3>
            <ul className="space-y-1">
              {section.items.map((item) => (
                <li key={item.name}>
                  <NavLink
                    to={item.path}
                    className={({ isActive }) =>
                      `flex items-center gap-3 px-3 py-2 rounded-lg transition-colors ${
                        isActive
                          ? 'bg-indigo-50 text-indigo-600'
                          : 'text-gray-700 hover:bg-gray-100'
                      }`
                    }
                  >
                    <item.icon className="w-5 h-5" />
                    <span className="flex-1 text-sm font-medium">{item.name}</span>
                  </NavLink>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </nav>

    </aside>
  );
}