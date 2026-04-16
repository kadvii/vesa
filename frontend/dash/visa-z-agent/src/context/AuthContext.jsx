import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import {
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  USER_STORAGE_KEY,
  api,
  clearAuthStorage,
} from '../services/api';

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [authError, setAuthError] = useState('');

  useEffect(() => {
    const saved = localStorage.getItem(USER_STORAGE_KEY);
    if (saved) {
      try {
        setUser(JSON.parse(saved));
      } catch (err) {
        console.error('Failed to parse saved user', err);
        clearAuthStorage();
      }
    }
    setLoading(false);
  }, []);

  const login = async ({ email, password }) => {
    setAuthError('');
    try {
      const result = await api.auth.login({ email, password });
      localStorage.setItem(ACCESS_TOKEN_KEY, result.accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, result.refreshToken);
      localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(result.user));
      setUser(result.user);
      return result.user;
    } catch (err) {
      const message = err?.message || 'فشل تسجيل الدخول. تحقق من البيانات.';
      setAuthError(message);
      throw new Error(message);
    }
  };

  const logout = async () => {
    try {
      await api.auth.logout();
    } catch (err) {
      // Server-side revocation failed — still clear local state so the user is logged out.
      console.warn('Logout API call failed (token may already be invalid):', err?.message);
    } finally {
      setUser(null);
      clearAuthStorage();
    }
  };

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: Boolean(user),
      loading,
      authError,
      login,
      logout,
    }),
    [user, loading, authError],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => useContext(AuthContext);
