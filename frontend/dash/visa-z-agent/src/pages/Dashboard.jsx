import { useEffect, useState } from 'react';
import Card from '../components/Card';
import { getVisaStats } from '../services/api';
import { useAuth } from '../context/AuthContext';

const ROLE_CAN_SEE_STATS = ['Admin', 'Employee'];

const ROLE_MAP = { User: 'مستخدم', Admin: 'مدير', Agent: 'وكيل', Employee: 'موظف' };

export default function Dashboard() {
  const { user } = useAuth();
  const canSeeStats = user && ROLE_CAN_SEE_STATS.includes(user.role);

  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(canSeeStats);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!canSeeStats) return;
    setLoading(true);
    getVisaStats()
      .then((data) => setStats(data))
      .catch((e) => setError(e.message || 'تعذر تحميل الإحصائيات'))
      .finally(() => setLoading(false));
  }, [canSeeStats]);

  return (
    <div className="space-y-6 text-beige">
      {/* Header */}
      <div>
        <p className="text-sm font-semibold text-brand">الرئيسية</p>
        <h1 className="text-2xl font-display font-semibold text-beige">
          {canSeeStats ? 'لوحة التحكم — Visa Z' : `مرحباً بك، ${user?.name || 'وكيلنا العزيز'} 👋`}
        </h1>
        <p className="text-sm text-beige/70">
          {canSeeStats
            ? 'إحصائيات فورية لطلبات التأشيرة.'
            : 'يمكنك تقديم طلبات الفيزا ومتابعة حالتها من القائمة.'}
        </p>
      </div>

      {/* Stats cards — Admin / Employee only */}
      {canSeeStats && (
        <>
          {error && (
            <div className="rounded-xl border border-danger/40 bg-danger/15 px-4 py-3 text-sm font-semibold text-danger">
              {error}
            </div>
          )}
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {[
              { label: 'إجمالي الطلبات', key: 'total',    color: '#60a5fa' },
              { label: 'قيد المراجعة',   key: 'pending',  color: '#facc15' },
              { label: 'مقبولة',          key: 'approved', color: '#4ade80' },
              { label: 'مرفوضة',          key: 'rejected', color: '#f87171' },
            ].map(({ label, key, color }) => (
              <Card key={key} className="bg-base/60 space-y-2">
                <p className="text-sm text-beige/70">{label}</p>
                {loading ? (
                  <div className="h-9 w-16 animate-pulse rounded-lg bg-beige/10" />
                ) : (
                  <p className="text-4xl font-bold" style={{ color }}>
                    {stats?.[key] ?? '—'}
                  </p>
                )}
              </Card>
            ))}
          </div>
        </>
      )}

      {/* Agent / non-admin welcome */}
      {!canSeeStats && (
        <div className="grid gap-4 sm:grid-cols-2">
          <Card className="bg-base/60 space-y-1">
            <p className="text-sm text-beige/70">الدور</p>
            <p className="text-xl font-bold text-brand">{ROLE_MAP[user?.role] || user?.role || '—'}</p>
          </Card>
          <Card className="bg-base/60 space-y-1">
            <p className="text-sm text-beige/70">البريد الإلكتروني</p>
            <p className="text-sm font-semibold text-beige break-all">{user?.email || '—'}</p>
          </Card>
        </div>
      )}
    </div>
  );
}
