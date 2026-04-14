import { useCallback, useEffect, useState } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import CheckoutModal from '../components/CheckoutModal';
import { getVisas, initiateCheckout } from '../services/api';
import { useAuth } from '../context/AuthContext';

// ── Status display config ─────────────────────────────────────────────────────
const STATUS_META = {
  Pending:        { label: 'قيد المراجعة',     color: '#facc15', dot: '#facc15', icon: '⏳' },
  UnderReview:    { label: 'تحت المراجعة',      color: '#60a5fa', dot: '#60a5fa', icon: '🔍' },
  Approved:       { label: 'مقبول',             color: '#4ade80', dot: '#4ade80', icon: '✅' },
  Rejected:       { label: 'مرفوض',             color: '#f87171', dot: '#f87171', icon: '❌' },
  PendingPayment: { label: 'بانتظار الدفع',     color: '#fb923c', dot: '#fb923c', icon: '💳' },
  Paid:           { label: 'مدفوع ✓',           color: '#a78bfa', dot: '#a78bfa', icon: '💜' },
};

const DEFAULT_META = { label: '—', color: '#8899aa', dot: '#8899aa', icon: '❓' };

// Visa fee — in a real system this comes from the application record
const VISA_FEE_USD = 75;

export default function VisaStatus() {
  const { user, loading: authLoading, isAuthenticated } = useAuth();
  const [list,     setList]     = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [error,    setError]    = useState('');

  // Checkout state
  const [checkoutTarget, setCheckoutTarget] = useState(null);  // { applicationId, reference }
  const [session,        setSession]        = useState(null);  // CheckoutSessionDto
  const [initiating,     setInitiating]     = useState(false);
  const [initError,      setInitError]      = useState('');

  // ── Load visa list ──────────────────────────────────────────────────────────
  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await getVisas();
      setList(Array.isArray(res) ? res : []);
    } catch (err) {
      setError(err.message || 'تعذر تحميل الطلبات');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (authLoading) return;
    if (!isAuthenticated) { setList([]); setLoading(false); return; }
    load();
  }, [authLoading, isAuthenticated, user?.email, load]);

  // ── Initiate checkout session ───────────────────────────────────────────────
  const handlePayNow = async (item) => {
    setCheckoutTarget(item);
    setInitiating(true);
    setInitError('');
    setSession(null);
    try {
      const s = await initiateCheckout({
        applicationId: item.id,
        amount:        VISA_FEE_USD,
        currency:      'USD',
        method:        1,   // CreditCard default; user picks inside modal
      });
      setSession(s);
    } catch (err) {
      setInitError(err.message || 'تعذر فتح بوابة الدفع');
    } finally {
      setInitiating(false);
    }
  };

  // ── After successful payment ────────────────────────────────────────────────
  const handlePaymentSuccess = () => {
    setSession(null);
    setCheckoutTarget(null);
    // Refresh list so the card shows "مدفوع"
    load();
  };

  const handleModalClose = () => {
    setSession(null);
    setCheckoutTarget(null);
    setInitError('');
  };

  // ── Render ──────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-4 text-beige" dir="rtl">
      {/* Header */}
      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">حجوزات الفيزا</p>
          <h1 className="text-2xl font-display font-semibold text-beige">
            متابعة طلبات الفيزا الإلكترونية
          </h1>
          <p className="text-sm text-beige/70">
            تحديثات فورية — قيد المراجعة · مقبول · بانتظار الدفع · مدفوع · مرفوض.
          </p>
        </div>
        <Button variant="ghost" onClick={load} loading={loading}>
          تحديث
        </Button>
      </div>

      {error && (
        <div className="rounded-lg bg-danger/15 px-4 py-3 text-sm font-semibold text-danger">
          {error}
        </div>
      )}

      {/* Init error (Pay Now failed) */}
      {initError && (
        <div className="rounded-lg bg-danger/15 px-4 py-3 text-sm font-semibold text-danger">
          {initError}
        </div>
      )}

      {loading ? (
        <Card className="flex items-center justify-center text-sm text-beige/70">
          جارٍ تحميل الطلبات...
        </Card>
      ) : list.length ? (
        <div className="grid gap-4 md:grid-cols-2">
          {list.map((item) => {
            const meta = STATUS_META[item.status] ?? DEFAULT_META;
            const isPendingPayment = item.status === 'PendingPayment';
            const isPaying = initiating && checkoutTarget?.id === item.id;

            return (
              <div
                key={item.id}
                style={{
                  background: 'linear-gradient(145deg,rgba(26,31,53,0.9),rgba(15,18,32,0.95))',
                  border: `1px solid ${isPendingPayment ? 'rgba(251,146,60,0.35)' : 'rgba(255,255,255,0.08)'}`,
                  borderRadius: '16px',
                  padding: '20px',
                  position: 'relative',
                  boxShadow: isPendingPayment ? '0 0 18px rgba(251,146,60,0.12)' : 'none',
                  transition: 'box-shadow 0.3s',
                }}
              >
                {/* Status badge */}
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '12px' }}>
                  <span style={{ fontSize: '20px' }}>{meta.icon}</span>
                  <span style={{
                    background: `${meta.color}18`,
                    border: `1px solid ${meta.color}44`,
                    color: meta.color,
                    borderRadius: '20px',
                    padding: '3px 12px',
                    fontSize: '12px',
                    fontWeight: 700,
                  }}>
                    <span style={{
                      display: 'inline-block', width: '7px', height: '7px',
                      borderRadius: '50%', background: meta.dot,
                      marginLeft: '6px', verticalAlign: 'middle',
                    }} />
                    {meta.label}
                  </span>
                </div>

                {/* Application details */}
                <div style={{ marginBottom: '14px' }}>
                  <p style={{ fontSize: '16px', fontWeight: 700, color: '#e8dcc8', margin: '0 0 4px' }}>
                    {item.country || item.destination || item.visaTypeLabel || 'طلب تأشيرة'}
                  </p>
                  <p style={{ fontSize: '12px', color: '#6b7a8d', margin: 0 }}>
                    {item.visaTypeLabel || item.visaType || '—'}
                    {item.date ? ` · ${item.date}` : ''}
                  </p>
                </div>

                {/* Footer: reference + Pay Now button */}
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '11px', fontFamily: 'monospace', color: '#6b7a8d' }}>
                    {item.reference}
                  </span>

                  {isPendingPayment && (
                    <button
                      onClick={() => handlePayNow(item)}
                      disabled={isPaying}
                      style={{
                        padding: '8px 18px',
                        background: isPaying
                          ? 'rgba(251,146,60,0.15)'
                          : 'linear-gradient(135deg,#fb923c,#f97316)',
                        border: 'none',
                        borderRadius: '10px',
                        color: isPaying ? '#fb923c' : '#fff',
                        fontWeight: 700,
                        fontSize: '13px',
                        cursor: isPaying ? 'wait' : 'pointer',
                        fontFamily: "'Cairo',sans-serif",
                        transition: 'all 0.2s',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '6px',
                      }}
                    >
                      {isPaying ? (
                        <>⏳ جارٍ الفتح…</>
                      ) : (
                        <>💳 ادفع الآن</>
                      )}
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <Card className="text-sm text-beige/70">
          لا توجد طلبات بعد. ابدأ بالحجز الآن.
        </Card>
      )}

      {/* Checkout Modal — rendered when session is ready */}
      {session && (
        <CheckoutModal
          session={session}
          onSuccess={handlePaymentSuccess}
          onClose={handleModalClose}
        />
      )}
    </div>
  );
}
