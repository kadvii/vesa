import { useState } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import ComingSoon from '../components/ComingSoon';

const initialPrices = [
  { country: 'تركيا', price: 100 },
  { country: 'الإمارات', price: 150 },
];

export default function Prices() {
  const [prices, setPrices] = useState(initialPrices);
  const [form, setForm] = useState({ country: '', price: '' });

  const addPrice = (e) => {
    e.preventDefault();
    if (!form.country.trim() || !form.price) return;
    setPrices((prev) => [...prev, { country: form.country, price: Number(form.price) }]);
    setForm({ country: '', price: '' });
  };

  const updatePrice = (country, value) => {
    setPrices((prev) => prev.map((p) => (p.country === country ? { ...p, price: Number(value) } : p)));
  };

  return (
    <ComingSoon label="إدارة الأسعار">
    <div className="space-y-4 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">إدارة الأسعار</p>
          <h1 className="text-2xl font-display font-semibold text-beige">تحديد أسعار الفيزا</h1>
        </div>
        <Button onClick={addPrice}>حفظ</Button>
      </div>

      <Card className="space-y-3">
        <form className="grid gap-3 md:grid-cols-3" onSubmit={addPrice}>
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="الدولة"
            value={form.country}
            onChange={(e) => setForm((p) => ({ ...p, country: e.target.value }))}
          />
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="السعر بالدولار"
            type="number"
            value={form.price}
            onChange={(e) => setForm((p) => ({ ...p, price: e.target.value }))}
          />
          <Button type="submit">إضافة سعر</Button>
        </form>
      </Card>

      <Card className="space-y-2">
        <h3 className="text-lg font-semibold text-beige">الأسعار الحالية</h3>
        <div className="space-y-2">
          {prices.map((p) => (
            <div
              key={p.country}
              className="grid grid-cols-3 items-center rounded-xl border border-beige/15 bg-base/60 px-3 py-2 text-sm"
            >
              <span>{p.country}</span>
              <input
                className="rounded-lg border border-beige/25 bg-surface px-2 py-1 text-center text-sm text-beige outline-none"
                type="number"
                value={p.price}
                onChange={(e) => updatePrice(p.country, e.target.value)}
              />
              <span className="text-beige/70">USD</span>
            </div>
          ))}
        </div>
      </Card>
    </div>
    </ComingSoon>
  );
}
