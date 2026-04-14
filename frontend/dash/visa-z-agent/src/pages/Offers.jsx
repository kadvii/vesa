import { useState } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import ComingSoon from '../components/ComingSoon';

const initialOffers = [
  { id: 'off-1', title: 'خصم 15% لتركيا', desc: 'يستمر حتى نهاية الشهر' },
  { id: 'off-2', title: 'فيزا الإمارات 130$', desc: 'عرض محدود 7 أيام' },
];

export default function Offers() {
  const [offers, setOffers] = useState(initialOffers);
  const [form, setForm] = useState({ title: '', desc: '' });

  const addOffer = (e) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    setOffers((prev) => [{ id: `off-${prev.length + 1}`, ...form }, ...prev]);
    setForm({ title: '', desc: '' });
  };

  const removeOffer = (id) => {
    setOffers((prev) => prev.filter((o) => o.id !== id));
  };

  return (
    <ComingSoon label="العروض والخصومات">
    <div className="space-y-4 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">العروض</p>
          <h1 className="text-2xl font-display font-semibold text-beige">إدارة الخصومات والعروض</h1>
        </div>
        <Button onClick={addOffer}>حفظ</Button>
      </div>

      <Card className="space-y-3">
        <form className="grid gap-3 md:grid-cols-3" onSubmit={addOffer}>
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="عنوان العرض"
            value={form.title}
            onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
          />
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="وصف مختصر"
            value={form.desc}
            onChange={(e) => setForm((p) => ({ ...p, desc: e.target.value }))}
          />
          <Button type="submit">إضافة عرض</Button>
        </form>
      </Card>

      <div className="grid gap-3 md:grid-cols-2">
        {offers.map((o) => (
          <Card key={o.id} className="space-y-2 bg-base/60">
            <div className="flex items-start justify-between gap-2">
              <div>
                <p className="text-lg font-semibold text-beige">{o.title}</p>
                <p className="text-sm text-beige/70">{o.desc}</p>
              </div>
              <Button variant="ghost" className="text-danger" onClick={() => removeOffer(o.id)}>
                حذف
              </Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
    </ComingSoon>
  );
}
