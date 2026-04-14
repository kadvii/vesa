import { useState } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import ComingSoon from '../components/ComingSoon';

const initialDocs = [
  { country: 'تركيا', docs: ['جواز سفر', 'صورة شخصية'] },
  { country: 'الإمارات', docs: ['جواز سفر', 'حجز فندق'] },
];

export default function Documents() {
  const [list, setList] = useState(initialDocs);
  const [form, setForm] = useState({ country: '', doc: '' });

  const addDoc = (e) => {
    e.preventDefault();
    if (!form.country.trim() || !form.doc.trim()) return;
    setList((prev) => {
      const existing = prev.find((c) => c.country === form.country);
      if (existing) {
        return prev.map((c) =>
          c.country === form.country ? { ...c, docs: [...c.docs, form.doc] } : c,
        );
      }
      return [...prev, { country: form.country, docs: [form.doc] }];
    });
    setForm({ country: '', doc: '' });
  };

  return (
    <ComingSoon label="الوثائق المطلوبة">
    <div className="space-y-4 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">الوثائق المطلوبة</p>
          <h1 className="text-2xl font-display font-semibold text-beige">تعريف الوثائق لكل دولة</h1>
        </div>
        <Button onClick={addDoc}>حفظ</Button>
      </div>

      <Card className="space-y-3">
        <form className="grid gap-3 md:grid-cols-3" onSubmit={addDoc}>
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="الدولة"
            value={form.country}
            onChange={(e) => setForm((p) => ({ ...p, country: e.target.value }))}
          />
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="اسم الوثيقة"
            value={form.doc}
            onChange={(e) => setForm((p) => ({ ...p, doc: e.target.value }))}
          />
          <Button type="submit">إضافة وثيقة</Button>
        </form>
      </Card>

      <div className="grid gap-3 md:grid-cols-2">
        {list.map((c) => (
          <Card key={c.country} className="bg-base/60 space-y-2">
            <div className="flex items-center justify-between">
              <p className="text-lg font-semibold text-beige">{c.country}</p>
              <span className="badge bg-brand/20 text-brand border border-brand/40">عدد {c.docs.length}</span>
            </div>
            <ul className="space-y-1 text-sm text-beige/80">
              {c.docs.map((d, i) => (
                <li key={i}>- {d}</li>
              ))}
            </ul>
          </Card>
        ))}
      </div>
    </div>
    </ComingSoon>
  );
}
