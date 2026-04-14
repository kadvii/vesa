import { useEffect, useRef, useState } from 'react';
import { confirmPayment } from '../services/api';

// Payment method labels
const METHOD_OPTIONS = [
  { value: 1, label: 'بطاقة ائتمانية',  icon: '💳' },
  { value: 2, label: 'بطاقة مدينة',     icon: '🏦' },
  { value: 4, label: 'محفظة إلكترونية', icon: '📱' },
];

const PHASE = {
  FORM:       'form',       // card / wallet details form
  PROCESSING: 'processing', // spinner while calling confirm webhook
  SUCCESS:    'success',    // green confirmation screen
  FAILED:     'failed',     // red error screen
};

/**
 * CheckoutModal
 *
 * Props:
 *  @param {object}   session         — CheckoutSessionDto returned by initiateCheckout()
 *  @param {string}   session.paymentId
 *  @param {string}   session.sessionToken
 *  @param {number}   session.amount
 *  @param {string}   session.currency
 *  @param {string}   session.description
 *  @param {string}   session.expiresAt    — ISO timestamp
 *  @param {function} onSuccess(paymentDto) — called after confirmed Paid
 *  @param {function} onClose()            — called when the user closes the modal
 */
export default function CheckoutModal({ session, onSuccess, onClose }) {
  const [phase,    setPhase]    = useState(PHASE.FORM);
  const [method,   setMethod]   = useState(1);          // selected PaymentMethod enum
  const [cardNum,  setCardNum]  = useState('');
  const [expiry,   setExpiry]   = useState('');
  const [cvv,      setCvv]      = useState('');
  const [errors,   setErrors]   = useState({});
  const [errMsg,   setErrMsg]   = useState('');
  const [timeLeft, setTimeLeft] = useState(null);       // countdown seconds

  const backdropRef = useRef(null);

  // ── Countdown timer ─────────────────────────────────────────────────────────
  useEffect(() => {
    if (!session?.expiresAt) return;
    const tick = () => {
      const secs = Math.floor((new Date(session.expiresAt) - Date.now()) / 1000);
      setTimeLeft(secs > 0 ? secs : 0);
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [session?.expiresAt]);

  // ── Close on backdrop click ─────────────────────────────────────────────────
  const handleBackdropClick = (e) => {
    if (e.target === backdropRef.current) onClose();
  };

  // ── Validation ──────────────────────────────────────────────────────────────
  const validate = () => {
    const next = {};
    if (method === 1 || method === 2) {
      // Card validations
      if (!/^\d{16}$/.test(cardNum.replace(/\s/g, '')))
        next.cardNum = 'رقم البطاقة يجب أن يكون 16 رقماً';
      if (!/^\d{2}\/\d{2}$/.test(expiry))
        next.expiry = 'تاريخ الانتهاء بصيغة MM/YY';
      if (!/^\d{3,4}$/.test(cvv))
        next.cvv = 'رمز CVV غير صحيح';
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  // ── Submit ──────────────────────────────────────────────────────────────────
  const handlePay = async () => {
    if (!validate()) return;
    if (timeLeft === 0) {
      setErrMsg('انتهت صلاحية جلسة الدفع. أغلق وأعد المحاولة.');
      return;
    }

    setPhase(PHASE.PROCESSING);
    setErrMsg('');

    try {
      // In production: the payment gateway calls the webhook on its own.
      // In development/demo: the frontend calls the confirm endpoint directly
      // after the "gateway UI" (this modal) completes.
      const result = await confirmPayment({
        paymentId:        session.paymentId,
        sessionToken:     session.sessionToken,
        status:           'Paid',
        gatewayReference: `SIM-${Date.now()}`,   // simulated gateway ref
      });

      setPhase(PHASE.SUCCESS);
      onSuccess(result);
    } catch (err) {
      setErrMsg(err.message || 'فشلت عملية الدفع');
      setPhase(PHASE.FAILED);
    }
  };

  // ── Format helpers ──────────────────────────────────────────────────────────
  const fmtAmount = new Intl.NumberFormat('ar-IQ', {
    style: 'currency', currency: session?.currency ?? 'USD', maximumFractionDigits: 2,
  }).format(session?.amount ?? 0);

  const fmtTime = timeLeft != null
    ? `${String(Math.floor(timeLeft / 60)).padStart(2, '0')}:${String(timeLeft % 60).padStart(2, '0')}`
    : null;

  const formatCardNum = (v) =>
    v.replace(/\D/g, '').slice(0, 16).replace(/(\d{4})(?=\d)/g, '$1 ');

  const formatExpiry = (v) => {
    const d = v.replace(/\D/g, '').slice(0, 4);
    return d.length >= 3 ? `${d.slice(0, 2)}/${d.slice(2)}` : d;
  };

  // ── Render ──────────────────────────────────────────────────────────────────
  return (
    <div
      ref={backdropRef}
      onClick={handleBackdropClick}
      style={{
        position: 'fixed', inset: 0, zIndex: 1000,
        background: 'rgba(0,0,0,0.75)',
        backdropFilter: 'blur(4px)',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        padding: '16px',
      }}
    >
      <div
        dir="rtl"
        style={{
          background: 'linear-gradient(145deg,#1a1f35,#0f1220)',
          border: '1px solid rgba(255,255,255,0.1)',
          borderRadius: '20px',
          padding: '28px 24px',
          width: '100%',
          maxWidth: '420px',
          fontFamily: "'Cairo','Segoe UI',sans-serif",
          color: '#e8dcc8',
          position: 'relative',
          boxShadow: '0 24px 60px rgba(0,0,0,0.6)',
        }}
      >
        {/* Close button */}
        <button
          onClick={onClose}
          style={{
            position: 'absolute', top: '16px', left: '16px',
            background: 'transparent', border: 'none',
            color: '#8899aa', fontSize: '20px', cursor: 'pointer', lineHeight: 1,
          }}
        >
          ✕
        </button>

        {/* ── Header ── */}
        <div style={{ textAlign: 'center', marginBottom: '20px' }}>
          <div style={{ fontSize: '32px', marginBottom: '6px' }}>🔐</div>
          <h2 style={{ margin: 0, fontSize: '18px', fontWeight: 800, color: '#d4af37' }}>
            بوابة الدفع الآمن
          </h2>
          <p style={{ margin: '4px 0 0', fontSize: '13px', color: '#8899aa' }}>
            {session?.description}
          </p>
        </div>

        {/* ── Amount + countdown ── */}
        <div style={{
          background: 'rgba(212,175,55,0.08)',
          border: '1px solid rgba(212,175,55,0.25)',
          borderRadius: '12px',
          padding: '12px 16px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: '20px',
        }}>
          <span style={{ fontSize: '22px', fontWeight: 800, color: '#d4af37' }}>
            {fmtAmount}
          </span>
          {fmtTime && (
            <span style={{
              fontSize: '12px', color: timeLeft < 60 ? '#f87171' : '#8899aa',
              fontFamily: 'monospace',
            }}>
              ⏱ {fmtTime}
            </span>
          )}
        </div>

        {/* ── SUCCESS ── */}
        {phase === PHASE.SUCCESS && (
          <div style={{ textAlign: 'center', padding: '24px 0' }}>
            <div style={{ fontSize: '48px', marginBottom: '12px' }}>✅</div>
            <p style={{ fontSize: '18px', fontWeight: 700, color: '#4ade80', margin: 0 }}>
              تمت عملية الدفع بنجاح
            </p>
            <p style={{ fontSize: '13px', color: '#8899aa', marginTop: '6px' }}>
              تم تحديث حالة طلبك إلى "مدفوع"
            </p>
            <button
              onClick={onClose}
              style={{
                marginTop: '20px', padding: '10px 28px',
                background: 'linear-gradient(135deg,#4ade80,#22c55e)',
                border: 'none', borderRadius: '10px',
                color: '#fff', fontWeight: 700, fontSize: '14px',
                cursor: 'pointer', fontFamily: "'Cairo',sans-serif",
              }}
            >
              إغلاق
            </button>
          </div>
        )}

        {/* ── PROCESSING ── */}
        {phase === PHASE.PROCESSING && (
          <div style={{ textAlign: 'center', padding: '32px 0' }}>
            <div style={{ fontSize: '36px', animation: 'spin 1s linear infinite' }}>⏳</div>
            <p style={{ color: '#8899aa', marginTop: '12px' }}>جارٍ معالجة الدفع…</p>
            <style>{`@keyframes spin { to { transform:rotate(360deg);} }`}</style>
          </div>
        )}

        {/* ── FAILED ── */}
        {phase === PHASE.FAILED && (
          <div style={{ textAlign: 'center', padding: '20px 0' }}>
            <div style={{ fontSize: '42px', marginBottom: '12px' }}>❌</div>
            <p style={{ color: '#f87171', fontWeight: 600, margin: 0 }}>فشلت عملية الدفع</p>
            <p style={{ color: '#8899aa', fontSize: '13px', marginTop: '6px' }}>{errMsg}</p>
            <button
              onClick={() => setPhase(PHASE.FORM)}
              style={{
                marginTop: '16px', padding: '10px 28px',
                background: 'rgba(248,113,113,0.2)',
                border: '1px solid rgba(248,113,113,0.4)',
                borderRadius: '10px', color: '#f87171',
                fontWeight: 700, fontSize: '14px',
                cursor: 'pointer', fontFamily: "'Cairo',sans-serif",
              }}
            >
              إعادة المحاولة
            </button>
          </div>
        )}

        {/* ── FORM ── */}
        {phase === PHASE.FORM && (
          <>
            {/* Method selector */}
            <p style={{ fontSize: '12px', color: '#8899aa', marginBottom: '8px' }}>
              طريقة الدفع
            </p>
            <div style={{ display: 'flex', gap: '8px', marginBottom: '20px' }}>
              {METHOD_OPTIONS.map((m) => (
                <button
                  key={m.value}
                  type="button"
                  onClick={() => setMethod(m.value)}
                  style={{
                    flex: 1, padding: '10px 6px',
                    background: method === m.value
                      ? 'rgba(212,175,55,0.15)' : 'rgba(255,255,255,0.04)',
                    border: `1px solid ${method === m.value ? '#d4af37' : 'rgba(255,255,255,0.1)'}`,
                    borderRadius: '10px', color: method === m.value ? '#d4af37' : '#8899aa',
                    fontSize: '11px', fontWeight: 600, cursor: 'pointer',
                    fontFamily: "'Cairo',sans-serif", textAlign: 'center',
                    transition: 'all 0.15s',
                  }}
                >
                  <div style={{ fontSize: '18px', marginBottom: '3px' }}>{m.icon}</div>
                  {m.label}
                </button>
              ))}
            </div>

            {/* Card fields (for card methods) */}
            {(method === 1 || method === 2) && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '12px', marginBottom: '20px' }}>
                {/* Card number */}
                <div>
                  <label style={{ fontSize: '12px', color: '#8899aa', display: 'block', marginBottom: '5px' }}>
                    رقم البطاقة
                  </label>
                  <input
                    type="text"
                    inputMode="numeric"
                    placeholder="0000 0000 0000 0000"
                    maxLength={19}
                    value={cardNum}
                    onChange={(e) => setCardNum(formatCardNum(e.target.value))}
                    style={inputStyle(!!errors.cardNum)}
                  />
                  {errors.cardNum && <p style={errStyle}>{errors.cardNum}</p>}
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '10px' }}>
                  <div>
                    <label style={{ fontSize: '12px', color: '#8899aa', display: 'block', marginBottom: '5px' }}>
                      تاريخ الانتهاء
                    </label>
                    <input
                      type="text"
                      placeholder="MM/YY"
                      maxLength={5}
                      value={expiry}
                      onChange={(e) => setExpiry(formatExpiry(e.target.value))}
                      style={inputStyle(!!errors.expiry)}
                    />
                    {errors.expiry && <p style={errStyle}>{errors.expiry}</p>}
                  </div>
                  <div>
                    <label style={{ fontSize: '12px', color: '#8899aa', display: 'block', marginBottom: '5px' }}>
                      CVV
                    </label>
                    <input
                      type="password"
                      placeholder="•••"
                      maxLength={4}
                      value={cvv}
                      onChange={(e) => setCvv(e.target.value.replace(/\D/g, '').slice(0, 4))}
                      style={inputStyle(!!errors.cvv)}
                    />
                    {errors.cvv && <p style={errStyle}>{errors.cvv}</p>}
                  </div>
                </div>
              </div>
            )}

            {/* Wallet — no extra fields needed */}
            {method === 4 && (
              <div style={{
                background: 'rgba(96,165,250,0.08)',
                border: '1px solid rgba(96,165,250,0.25)',
                borderRadius: '12px', padding: '14px 16px',
                fontSize: '13px', color: '#93c5fd',
                marginBottom: '20px', textAlign: 'center',
              }}>
                📱 سيتم إرسال طلب الموافقة إلى تطبيق محفظتك الإلكترونية
              </div>
            )}

            {errMsg && (
              <p style={{ ...errStyle, marginBottom: '12px', textAlign: 'center' }}>{errMsg}</p>
            )}

            {/* Pay button */}
            <button
              onClick={handlePay}
              disabled={timeLeft === 0}
              style={{
                width: '100%', padding: '14px',
                background: timeLeft === 0
                  ? 'rgba(255,255,255,0.05)'
                  : 'linear-gradient(135deg,#d4af37,#f0d060)',
                border: 'none', borderRadius: '12px',
                color: timeLeft === 0 ? '#555' : '#0f1220',
                fontWeight: 800, fontSize: '15px',
                cursor: timeLeft === 0 ? 'not-allowed' : 'pointer',
                fontFamily: "'Cairo',sans-serif",
                transition: 'all 0.2s',
                letterSpacing: '0.5px',
              }}
            >
              {timeLeft === 0 ? 'انتهت صلاحية الجلسة' : `ادفع ${fmtAmount} الآن 🔒`}
            </button>

            <p style={{ textAlign: 'center', fontSize: '11px', color: '#555', marginTop: '12px' }}>
              🔐 جميع المعاملات مشفرة بـ TLS 1.3
            </p>
          </>
        )}
      </div>
    </div>
  );
}

// ── Style helpers ──────────────────────────────────────────────────────────────
const inputStyle = (hasError) => ({
  width: '100%',
  background: 'rgba(255,255,255,0.05)',
  border: `1px solid ${hasError ? '#f87171' : 'rgba(255,255,255,0.15)'}`,
  borderRadius: '10px',
  padding: '11px 14px',
  color: '#e8dcc8',
  fontSize: '14px',
  fontFamily: "'Cairo', monospace",
  outline: 'none',
  letterSpacing: '1px',
  boxSizing: 'border-box',
});

const errStyle = {
  color: '#f87171',
  fontSize: '11px',
  margin: '4px 0 0',
};
