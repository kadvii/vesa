import { useMemo } from 'react';
import { useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Button from './Button';

export default function Navbar({ onOpenSidebar }) {
  const { user, logout } = useAuth();
  const location = useLocation();

  const pageTitle = useMemo(() => {
    if (location.pathname.includes('visa-booking')) return 'حجز الفيزا الإلكترونية';
    if (location.pathname.includes('visa-status')) return 'حجوزات الفيزا';
    if (location.pathname.includes('booking')) return 'حجز الدولار';
    if (location.pathname.includes('status')) return 'حالة الطلبات';
    if (location.pathname.includes('banks')) return 'البنوك المعتمدة';
    if (location.pathname.includes('profile')) return 'الملف الشخصي';
    return 'الرئيسية';
  }, [location.pathname]);

  return (
    <header className="sticky top-0 z-20 border-b border-beige/15 bg-surface/95 backdrop-blur-lg text-beige">
      <div className="flex items-center justify-between gap-4 px-4 py-3 lg:px-8">
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={onOpenSidebar}
            className="flex h-10 w-10 items-center justify-center rounded-xl border border-beige/25 bg-base text-beige shadow-sm transition hover:text-brand lg:hidden"
            aria-label="فتح القائمة"
          >
            <span className="sr-only">فتح القائمة</span>
            <div className="space-y-1.5">
              <span className="block h-0.5 w-6 rounded-full bg-current" />
              <span className="block h-0.5 w-4 rounded-full bg-current" />
              <span className="block h-0.5 w-5 rounded-full bg-current" />
            </div>
          </button>
          <div>
            <p className="text-xs font-semibold text-beige/70">لوحة التحكم</p>
            <h1 className="text-lg font-display font-semibold text-beige">{pageTitle}</h1>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <div className="hidden items-center gap-3 rounded-2xl border border-beige/20 bg-base/80 px-3 py-2 text-xs font-semibold text-beige sm:flex">
            <span className="h-2 w-2 animate-pulse rounded-full bg-brand" />
            <span>متصل بالنظام التجريبي</span>
          </div>
          {user && (
            <div className="flex items-center gap-2 rounded-full bg-base/70 px-3 py-2 text-sm font-semibold text-beige">
              <span className="flex h-9 w-9 items-center justify-center rounded-full bg-brand text-base shadow-inner">
                {user.name?.slice(0, 1)?.toUpperCase() || '?'}
              </span>
              <div className="leading-tight">
                <span className="block text-beige">{user.name}</span>
                <span className="text-xs text-beige/70">{user.email}</span>
              </div>
            </div>
          )}
          <Button variant="ghost" onClick={logout} className="hidden lg:inline-flex">
            تسجيل الخروج
          </Button>
        </div>
      </div>
    </header>
  );
}
