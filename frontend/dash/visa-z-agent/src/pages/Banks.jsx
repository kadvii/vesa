const banks = [
  { code: 'CBI', name: 'البنك المركزي العراقي', url: 'https://cbi.iq', city: 'بغداد', type: 'حكومي' },
  { code: 'RAF', name: 'مصرف الرافدين', url: 'http://rafidain-bank.gov.iq', city: 'بغداد', type: 'حكومي' },
  { code: 'RSH', name: 'مصرف الرشيد', url: 'https://rashidbank.gov.iq', city: 'بغداد', type: 'حكومي' },
  { code: 'TBI', name: 'المصرف العراقي للتجارة', url: 'https://tbi.com.iq', city: 'بغداد', type: 'تجاري' },
  { code: 'BOB', name: 'مصرف بغداد', url: 'https://bankofbaghdad.com', city: 'بغداد', type: 'تجاري' },
  { code: 'KIB', name: 'مصرف كردستان الدولي', url: 'https://www.kib.iq', city: 'أربيل', type: 'إقليمي' },
  { code: 'CIH', name: 'مصرف جيهان', url: 'https://cihanbank.com', city: 'أربيل', type: 'إقليمي' },
];

export default function Banks() {
  return (
    <section className="space-y-4">
      <div className="flex flex-col gap-2">
        <p className="text-sm font-semibold text-brand">البنوك المعتمدة</p>
        <h1 className="text-2xl font-display font-semibold text-slate-900">قائمة المصارف وروابطها</h1>
        <p className="text-sm text-slate-600">
          القائمة محدثة لأغراض العرض التجريبي. يمكنك زيارة الموقع الرسمي لكل مصرف للتحقق من متطلبات الحجز.
        </p>
      </div>

      <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3 text-beige">
        {banks.map((bank) => (
          <a
            key={bank.code}
            href={bank.url}
            target="_blank"
            rel="noreferrer noopener"
            className="group rounded-2xl border border-beige/15 bg-surface/90 p-4 shadow-card transition hover:-translate-y-0.5 hover:border-brand"
          >
            <div className="flex items-start justify-between gap-2">
              <div>
                <p className="text-xs font-semibold text-beige/70">{bank.code}</p>
                <h3 className="text-lg font-semibold text-beige">{bank.name}</h3>
                <p className="text-xs text-beige/60">
                  {bank.city} · {bank.type}
                </p>
              </div>
              <span className="rounded-full bg-base/60 px-3 py-1 text-xs font-semibold text-beige group-hover:bg-brand/20 group-hover:text-brand">
                زيارة الموقع
              </span>
            </div>
          </a>
        ))}
      </div>
    </section>
  );
}
