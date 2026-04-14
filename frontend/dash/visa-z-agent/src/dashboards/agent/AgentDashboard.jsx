import Card from '../../components/Card';
import Button from '../../components/Button';

const countries = ['تركيا', 'الإمارات', 'الأردن', 'قطر', 'السعودية'];

const bookings = [
  { id: 'V-401', user: 'أحمد شاكر', country: 'تركيا', date: '2026-04-09', status: 'قيد المراجعة' },
  { id: 'V-402', user: 'ليث باسم', country: 'الأردن', date: '2026-04-12', status: 'مقبول' },
];

export default function AgentDashboard() {
  return (
    <div className="space-y-4 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">لوحة الوكيل</p>
          <h1 className="text-2xl font-display font-semibold text-beige">إدارة الخدمات والأسعار</h1>
          <p className="text-sm text-beige/70">حدّث الأسعار، أضف عروضاً، واطلع على حجوزات العملاء.</p>
        </div>
        <Button>إضافة عرض جديد</Button>
      </div>

      <div className="grid gap-3 md:grid-cols-3">
        <Card className="bg-base/60">
          <p className="text-sm text-beige/70">متوسط سعر الفيزا</p>
          <p className="text-3xl font-bold text-beige">$120</p>
        </Card>
        <Card className="bg-base/60">
          <p className="text-sm text-beige/70">طلبات هذا الأسبوع</p>
          <p className="text-3xl font-bold text-beige">26</p>
        </Card>
        <Card className="bg-base/60">
          <p className="text-sm text-beige/70">دول مفعلة</p>
          <p className="text-3xl font-bold text-beige">12</p>
        </Card>
      </div>

      <Card className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-beige">أسعار الفيزا حسب البلد</h3>
          <Button variant="ghost">تحديث الأسعار</Button>
        </div>
        <div className="grid gap-2 md:grid-cols-2">
          {countries.map((c) => (
            <div key={c} className="flex items-center justify-between rounded-xl border border-beige/15 bg-base/60 px-3 py-2">
              <span>{c}</span>
              <input
                defaultValue="120"
                className="w-20 rounded-lg border border-beige/25 bg-surface px-2 py-1 text-center text-sm text-beige outline-none"
              />
            </div>
          ))}
        </div>
      </Card>

      <Card className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-beige">حجوزات العملاء</h3>
          <Button variant="ghost">تحديث</Button>
        </div>
        <div className="space-y-2">
          {bookings.map((b) => (
            <div key={b.id} className="grid grid-cols-4 items-center rounded-xl border border-beige/15 bg-base/60 px-3 py-2 text-sm">
              <span>{b.id}</span>
              <span>{b.user}</span>
              <span>{b.country}</span>
              <span className="text-brand">{b.status}</span>
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
}
