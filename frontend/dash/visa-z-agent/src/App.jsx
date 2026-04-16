import { useMemo } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import DashboardLayout from './layouts/DashboardLayout';
import ProtectedRoute from './components/ProtectedRoute';
import AdminOnlyRoute from './components/AdminOnlyRoute';
import { AuthProvider, useAuth } from './context/AuthContext';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Prices from './pages/Prices';
import Offers from './pages/Offers';
import Documents from './pages/Documents';
import Countries from './pages/Countries';
import Bookings from './pages/Bookings';
import AdminVisaQueue from './pages/AdminVisaQueue';
import VisaBooking from './pages/VisaBooking';
import VisaStatus from './pages/VisaStatus';

function LandingRedirect() {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />;
}

const agentMenuBase = [
  { key: 'home',        label: 'لوحة التحكم',             to: '/dashboard',                icon: null },
  { key: 'visa-status', label: 'طلبات الفيزا',             to: '/dashboard/visa-status',    icon: null },
  { key: 'agents',      label: 'إدارة الوكلاء',           to: '/dashboard/prices',         icon: null },
  { key: 'users',       label: 'المستخدمين',              to: '/dashboard/documents',      icon: null },
  { key: 'settings',    label: 'الإعدادات',               to: '/dashboard/countries',      icon: null },
];

function AppRoutes() {
  const { user } = useAuth();
  const menuItems = useMemo(() => {
    const items = [...agentMenuBase];
    if (user && user.role === 'Admin') {
      items.push({
        key: 'admin-visas',
        label: 'طلبات الفيزا (أدمن)',
        to: '/dashboard/admin/visas',
        icon: null,
      });
    }
    return items;
  }, [user]);

  return (
    <Routes>
      <Route path="/" element={<LandingRedirect />} />
      <Route path="/login" element={<Login />} />
      <Route element={<ProtectedRoute />}>
        <Route path="/dashboard" element={<DashboardLayout menuItems={menuItems} />}>
          <Route index element={<Dashboard />} />
          <Route path="visa-booking" element={<VisaBooking />} />
          <Route path="visa-status"  element={<VisaStatus />} />
          <Route path="prices"       element={<Prices />} />
          <Route path="offers"       element={<Offers />} />
          <Route path="documents"    element={<Documents />} />
          <Route path="countries"    element={<Countries />} />
          <Route path="bookings"     element={<Bookings />} />
          <Route element={<AdminOnlyRoute />}>
            <Route path="admin/visas" element={<AdminVisaQueue />} />
          </Route>
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
