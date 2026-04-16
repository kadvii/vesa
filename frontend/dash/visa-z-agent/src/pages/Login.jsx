import { useEffect, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import Button from '../components/Button';
import Input from '../components/Input';
import Card from '../components/Card';
import { useAuth } from '../context/AuthContext';

const isEmailOrPhone = (value) => {
  const emailRegex = /\S+@\S+\.\S+/;
  const phoneRegex = /^[0-9+]{7,15}$/;
  return emailRegex.test(value) || phoneRegex.test(value);
};

export default function Login() {
  const { isAuthenticated, login, authError, user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname;

  const [form, setForm] = useState({ identifier: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    document.title = 'تسجيل الدخول | Visa Z Agent Panel';
  }, []);

  if (isAuthenticated) {
    const target = from || `/dashboard`;
    return <Navigate to={target} replace />;
  }

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isEmailOrPhone(form.identifier)) {
      setError('أدخل بريداً إلكترونياً أو رقم هاتف صحيح.');
      return;
    }
    if (!form.password || form.password.length < 6) {
      setError('كلمة المرور يجب أن تكون 6 أحرف على الأقل.');
      return;
    }
    setError('');
    setLoading(true);
    try {
      const u = await login({ email: form.identifier, password: form.password });
      const target = from || `/dashboard`;
      navigate(target, { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-base px-4 py-10 text-beige">
      <div className="grid w-full max-w-5xl gap-8 rounded-3xl border border-beige/15 bg-surface/90 p-8 shadow-card backdrop-blur-xl md:grid-cols-[1.1fr_0.9fr]">
        <div className="flex flex-col justify-between gap-6 rounded-2xl border border-beige/15 bg-base/60 p-6 shadow-inner">
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand/25 text-2xl font-bold text-brand shadow-inner">
                Z
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-beige/70">Visa Z Platform</p>
                <h1 className="text-2xl font-display font-semibold text-beige">تسجيل الدخول إلى Visa Z</h1>
              </div>
            </div>
            <p className="text-sm leading-relaxed text-beige/80">
              منصة موحدة لإدارة الفيزا الإلكترونية وحجز الدولار بخطوات بسيطة وواجهة حكومية عصرية.
            </p>
            <div className="grid gap-3 text-sm text-beige/80 sm:grid-cols-2">
              <Highlight title="هوية رقمية" text="تسجيل عبر البريد أو الهاتف." />
              <Highlight title="متابعة آنية" text="تحديث فوري لحالة الفيزا." />
              <Highlight title="تجربة آمنة" text="تصميم حكومي بلمسة فينتك." />
              <Highlight title="واجهة عربية" text="دعم كامل للـ RTL." />
            </div>
          </div>
          <div className="flex items-center gap-3 rounded-2xl bg-surface/80 px-4 py-3 text-sm text-beige shadow-inner">
            <span className="h-2 w-2 animate-pulse rounded-full bg-brand" />
            <span>النظام يعمل بوضع المحاكاة - البيانات للتجربة فقط.</span>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-2xl border border-beige/15 bg-base/60 p-6 shadow-card">
          <div className="space-y-1">
            <p className="text-sm font-semibold text-brand">تسجيل الدخول</p>
            <h2 className="text-xl font-display font-semibold text-beige">مرحبا بك في Visa Z</h2>
            <p className="text-sm text-beige/70">أدخل بيانات الاعتماد للوصول إلى لوحة التحكم.</p>
          </div>

          <Input
            label="البريد الإلكتروني أو رقم الهاتف"
            placeholder="example@bank.iq أو 07701234567"
            value={form.identifier}
            onChange={(value) => setForm((prev) => ({ ...prev, identifier: value }))}
            required
          />
          <Input
            label="كلمة المرور"
            type="password"
            placeholder="••••••••"
            value={form.password}
            onChange={(value) => setForm((prev) => ({ ...prev, password: value }))}
            required
          />

          {(error || authError) && (
            <div className="rounded-xl bg-danger/15 px-4 py-3 text-sm font-semibold text-danger">
              {error || authError}
            </div>
          )}

          <Button type="submit" loading={loading} className="w-full">
            دخول إلى النظام
          </Button>

          <div className="flex items-center justify-center text-xs text-beige/70">
            بيانات تجريبية: agent@visaz.local / password123
          </div>
        </form>
      </div>
    </div>
  );
}

function Highlight({ title, text }) {
  return (
    <div className="rounded-xl border border-white/60 bg-white/60 px-4 py-3 shadow-sm shadow-sky-50">
      <p className="text-sm font-semibold text-slate-800">{title}</p>
      <p className="text-xs text-slate-600">{text}</p>
    </div>
  );
}
