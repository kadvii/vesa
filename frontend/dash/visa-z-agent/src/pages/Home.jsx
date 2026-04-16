import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Card from '../components/Card';
import Button from '../components/Button';
import { useAuth } from '../context/AuthContext';
import { getVisaStats, api } from '../services/api';

const STATUS_BADGE = {
  Pending:  'bg-yellow-500/20 text-yellow-300 border border-yellow-500/40',
  Approved: 'bg-green-500/20  text-green-300  border border-green-500/40',
  Rejected: 'bg-red-500/20    text-red-300    border border-red-500/40',
};

const STATUS_LABEL = {
  Pending:  'قيد المراجعة',
  Approved: 'مقبول',
  Rejected:  'مرفوض',
};

export default function Home() {
  const { user } = useAuth();

  const [stats,   setStats]   = useState(null);
  const [recent,  setRecent]  = useState([]);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState('');

  useEffect(() => {
    let cancelled = false;

    Promise.all([
      getVisaStats(),
      api.admin.getAllVisas(1, 5),
    ])
      .then(([statsData, visasData]) => {
        if (cancelled) return;
        setStats(statsData);
        setRecent(visasData.items.slice(0, 5));
      })
      .catch((err) => {
        if (!cancelled) setError(err?.message || 'تعذّر تحميل البيانات.');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  const statCards = stats
    ? [
        { label: 'إجمالي طلبات الفيزا', value: stats.total    ?? 0, hint: 'خلال آخر 30 يوم' },
        { label: 'قيد المراجعة',          value: stats.pending  ?? 0, hint: 'ينتظر الاعتماد'   },
        { label: 'مقبولة',                value: stats.approved ?? 0, hint: 'جاهزة للتسليم'    },
        { label: 'مرفوضة',               value: stats.rejected ?? 0, hint: 'تحتاج إعادة تقديم' },
      ]
    : [];

  return (
    <div className="space-y-6 text-beige">
      {/* Hero */}
      <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">مرحباً {user?.name || 'بك'} في Visa Z</p>
          <h2 className="text-2xl font-display font-semibold text-beige">منصة الفيزا الإلكترونية الحكومية</h2>
          <p className="text-sm text-beige/70">
            احجز فيزا إلكترونية، تابع حالتها، وأدر حجوزات الدولار في لوحة واحدة بتجربة عربية احترافية.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link to="/dashboard/visa-booking">
            <Button className="w-full md:w-auto">بدء حجز الفيزا</Button>
          </Link>
          <Link to="/dashboard/visa-status">
            <Button variant="ghost" className="w-full md:w-auto">
              متابعة الحالة
            </Button>
          </Link>
        </div>
      </div>

      {/* Error banner */}
      {error && (
        <p className="rounded-lg bg-red-500/10 px-4 py-3 text-sm text-red-400">{error}</p>
      )}

      {/* Stat Cards */}
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {loading
          ? Array.from({ length: 4 }).map((_, i) => (
              <Card key={i} className="flex flex-col gap-1 border-beige/10 bg-surface/90 animate-pulse">
                <div className="h-3 w-24 rounded bg-beige/10" />
                <div className="h-8 w-12 rounded bg-beige/10" />
              </Card>
            ))
          : statCards.map((item) => (
              <Card
                key={item.label}
                className="flex flex-col gap-1 border-beige/10 bg-surface/90 hover:-translate-y-0.5 hover:shadow-card transition"
              >
                <p className="text-xs font-semibold text-beige/70">{item.label}</p>
                <p className="text-3xl font-bold text-beige">{item.value}</p>
                <span className="text-xs text-beige/60">{item.hint}</span>
              </Card>
            ))}
      </div>

      {/* Recent Applications + Quick Actions */}
      <div className="grid gap-4 lg:grid-cols-[1.4fr_0.8fr]">
        <Card className="space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold text-beige">آخر الطلبات</h3>
            <Link to="/dashboard/visa-status" className="text-sm font-semibold text-brand hover:text-brand-dark">
              عرض الكل
            </Link>
          </div>

          <div className="space-y-3">
            {loading && (
              <p className="py-4 text-center text-sm text-beige/50">جارِ التحميل…</p>
            )}
            {!loading && !error && recent.length === 0 && (
              <p className="py-4 text-center text-sm text-beige/50">لا توجد طلبات حديثة.</p>
            )}
            {!loading && recent.map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between rounded-xl border border-beige/15 bg-base/60 px-4 py-3 text-sm"
              >
                <div className="flex flex-col">
                  <span className="font-semibold text-beige">{item.destination || item.visaTypeLabel || '—'}</span>
                  <span className="text-beige/60 text-xs">
                    {item.submissionDate ? String(item.submissionDate).slice(0, 10) : '—'}
                  </span>
                </div>
                <span className={`badge ${STATUS_BADGE[item.status] ?? ''}`}>
                  {STATUS_LABEL[item.status] ?? item.status}
                </span>
              </div>
            ))}
          </div>
        </Card>

        <Card className="space-y-3">
          <h3 className="text-lg font-semibold text-beige">إجراءات سريعة</h3>
          <QuickAction
            title="حجز فيزا جديدة"
            desc="املأ النموذج وابدأ المعالجة."
            to="/dashboard/visa-booking"
          />
          <QuickAction
            title="متابعة حالة الفيزا"
            desc="اطلع على حالة جميع الطلبات."
            to="/dashboard/visa-status"
          />
          <QuickAction
            title="حجز الدولار"
            desc="احجز مخصصات الدولار بسرعة."
            to="/dashboard/booking"
          />
        </Card>
      </div>
    </div>
  );
}

function QuickAction({ title, desc, to }) {
  return (
    <Link
      to={to}
      className="block rounded-xl border border-beige/15 bg-base/50 px-4 py-3 transition hover:-translate-y-0.5 hover:border-brand hover:shadow-card"
    >
      <p className="text-sm font-semibold text-beige">{title}</p>
      <p className="text-xs text-beige/70">{desc}</p>
    </Link>
  );
}
