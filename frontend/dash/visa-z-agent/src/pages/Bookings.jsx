import Card from '../components/Card';

const bookings = [
  { id: 'B-1001', name: 'أحمد شاكر', country: 'تركيا', date: '2026-04-09', status: 'قيد المراجعة' },
  { id: 'B-1002', name: 'ليث باسم', country: 'الأردن', date: '2026-04-12', status: 'مقبول' },
  { id: 'B-1003', name: 'هدى عامر', country: 'الإمارات', date: '2026-04-15', status: 'مرفوض' },
];

export default function Bookings() {
  return (
    <div className="space-y-4 text-beige">
      <div>
        <p className="text-sm font-semibold text-brand">الحجوزات</p>
        <h1 className="text-2xl font-display font-semibold text-beige">عرض حجوزات العملاء</h1>
        <p className="text-sm text-beige/70">عرض فقط (للتجريب، بدون تعديل).</p>
      </div>

      <Card className="space-y-2">
        <div className="grid grid-cols-4 gap-2 rounded-xl border border-beige/15 bg-base/60 px-3 py-2 text-sm font-semibold">
          <span>الاسم</span>
          <span>الدولة</span>
          <span>التاريخ</span>
          <span>الحالة</span>
        </div>
        {bookings.map((b) => (
          <div key={b.id} className="grid grid-cols-4 gap-2 rounded-xl border border-beige/15 bg-surface/80 px-3 py-2 text-sm">
            <span>{b.name}</span>
            <span>{b.country}</span>
            <span>{b.date}</span>
            <span className="text-brand">{b.status}</span>
          </div>
        ))}
      </Card>
    </div>
  );
}
