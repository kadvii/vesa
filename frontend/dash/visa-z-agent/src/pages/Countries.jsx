import { useState } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import ComingSoon from '../components/ComingSoon';

const initialCountries = ['تركيا', 'الإمارات', 'الأردن', 'قطر'];

export default function Countries() {
  const [countries, setCountries] = useState(initialCountries);
  const [input, setInput] = useState('');

  const addCountry = (e) => {
    e.preventDefault();
    if (!input.trim()) return;
    if (!countries.includes(input.trim())) {
      setCountries((prev) => [...prev, input.trim()]);
    }
    setInput('');
  };

  const removeCountry = (c) => {
    setCountries((prev) => prev.filter((item) => item !== c));
  };

  return (
    <ComingSoon label="الدول المتاحة">
    <div className="space-y-4 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">الدول المتاحة</p>
          <h1 className="text-2xl font-display font-semibold text-beige">إدارة الوجهات</h1>
        </div>
        <Button onClick={addCountry}>حفظ</Button>
      </div>

      <Card className="space-y-3">
        <form className="flex flex-col gap-3 md:flex-row" onSubmit={addCountry}>
          <input
            className="flex-1 rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="أضف دولة جديدة"
            value={input}
            onChange={(e) => setInput(e.target.value)}
          />
          <Button type="submit" className="md:w-auto w-full">
            إضافة دولة
          </Button>
        </form>
      </Card>

      <Card className="grid grid-cols-2 gap-2 md:grid-cols-3">
        {countries.map((c) => (
          <div
            key={c}
            className="flex items-center justify-between rounded-xl border border-beige/15 bg-base/60 px-3 py-2 text-sm"
          >
            <span>{c}</span>
            <Button variant="ghost" className="text-danger px-2 py-1 text-xs" onClick={() => removeCountry(c)}>
              حذف
            </Button>
          </div>
        ))}
      </Card>
    </div>
    </ComingSoon>
  );
}
