import { MoreVertical, LogOut, X } from 'lucide-react';
import { useState } from 'react';
import { keycloak, getUserInfo } from '@/lib/keycloak';

export default function Header() {
  const [showLogoutModal, setShowLogoutModal] = useState(false);
  const userInfo = getUserInfo();

  const handleLogout = () => {
    keycloak.logout();
  };

  return (
    <>
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-end">
          <div className="flex items-center gap-3">
            <div className="text-right">
              <p className="text-sm font-semibold text-gray-900">{userInfo.name || 'User'}</p>
              <p className="text-xs text-gray-500">
                {userInfo.roles?.[0] || 'Shop Admin'}
              </p>
            </div>
            <div className="relative">
              <img
                src={`https://ui-avatars.com/api/?name=${userInfo.name || 'User'}&background=6366f1&color=fff`}
                alt="User avatar"
                className="w-10 h-10 rounded-full"
              />
              <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 border-2 border-white rounded-full"></span>
            </div>
            <button 
              onClick={() => setShowLogoutModal(true)}
              className="p-1 text-gray-600 hover:bg-gray-100 rounded transition-colors"
            >
              <MoreVertical className="w-5 h-5" />
            </button>
          </div>
        </div>
      </header>

      {showLogoutModal && (
        <div 
          className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 animate-fade-in"
          onClick={() => setShowLogoutModal(false)}
        >
          <div 
            className="bg-white rounded-xl shadow-2xl w-full max-w-md mx-4 animate-scale-in"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between p-6 border-b border-gray-200">
              <h3 className="text-lg font-semibold text-gray-900">Confirmar Logout</h3>
              <button
                onClick={() => setShowLogoutModal(false)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="p-6">
              <div className="flex items-center gap-4 mb-4">
                <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center">
                  <LogOut className="w-6 h-6 text-red-600" />
                </div>
                <div>
                  <p className="text-gray-900 font-medium">Deseja sair da sua conta?</p>
                  <p className="text-sm text-gray-500">Você será redirecionado para a tela de login.</p>
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3 p-6 bg-gray-50 rounded-b-xl">
              <button
                onClick={() => setShowLogoutModal(false)}
                className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-100 transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={handleLogout}
                className="flex-1 px-4 py-2 bg-red-600 text-white font-medium rounded-lg hover:bg-red-700 transition-colors"
              >
                Sair
              </button>
            </div>
          </div>
        </div>
      )}

      <style>{`
        @keyframes fade-in {
          from {
            opacity: 0;
          }
          to {
            opacity: 1;
          }
        }

        @keyframes scale-in {
          from {
            opacity: 0;
            transform: scale(0.95);
          }
          to {
            opacity: 1;
            transform: scale(1);
          }
        }

        .animate-fade-in {
          animation: fade-in 0.2s ease-out;
        }

        .animate-scale-in {
          animation: scale-in 0.2s ease-out;
        }
      `}</style>
    </>
  );
}