import { useEffect, useState } from 'react';
import Card from '../components/Card';
import { api } from '../services/api';

const STATUS_CLASS = {
  Pending:  'text-yellow-400',
  Approved: 'text-green-400',
  Rejected: 'text-red-400',
};

export default function Bookings() {
  const [visas,   setVisas]   = useState([]);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState('');

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError('');

    api.admin
      .getAllVisas(1, 50)
      .then(({ items }) => {
        if (!cancelled) setVisas(items);
      })
      .catch((err) => {
        if (!cancelled) setError(err?.message || 'تعذّر تحميل البيانات.');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  return (
    <div className="space-y-4 text-beige">
      <div>
        <p className="text-sm font-semibold text-brand">الحجوزات</p>
        <h1 className="text-2xl font-display font-semibold text-beige">عرض حجوزات العملاء</h1>
        <p className="text-sm text-beige/70">جميع طلبات الفيزا المسجّلة في النظام.</p>
      </div>

      <Card className="space-y-2">
        {/* Header row */}
        <div className="grid grid-cols-4 gap-2 rounded-xl border border-beige/15 bg-base/60 px-3 py-2 text-sm font-semibold">
          <span>الاسم</span>
          <span>الدولة</span>
          <span>التاريخ</span>
          <span>الحالة</span>
        </div>

        {/* States */}
        {loading && (
          <p className="py-6 text-center text-sm text-beige/60">جارٍ التحميل…</p>
        )}

        {!loading && error && (
          <p className="py-6 text-center text-sm text-red-400">{error}</p>
        )}

        {!loading && !error && visas.length === 0 && (
          <p className="py-6 text-center text-sm text-beige/50">لا توجد حجوزات حتى الآن.</p>
        )}

        {!loading && !error && visas.map((v) => (
          <div
            key={v.id}
            className="grid grid-cols-4 gap-2 rounded-xl border border-beige/15 bg-surface/80 px-3 py-2 text-sm"
          >
            <span>{v.applicantName || '—'}</span>
            <span>{v.destination   || '—'}</span>
            <span>{v.submissionDate ? String(v.submissionDate).slice(0, 10) : '—'}</span>
            <span className={STATUS_CLASS[v.status] || 'text-brand'}>{v.status}</span>
          </div>
        ))}
      </Card>
    </div>
  );
}
