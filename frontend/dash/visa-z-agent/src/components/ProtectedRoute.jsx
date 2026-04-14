import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function ProtectedRoute() {
  const { isAuthenticated, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-base text-beige">
        <div className="flex items-center gap-3 rounded-2xl bg-surface/80 px-6 py-4 shadow-card">
          <span className="h-4 w-4 animate-spin rounded-full border-2 border-brand/30 border-t-brand" />
          <span className="text-sm font-semibold">جاري التحقق من الجلسة...</span>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}
