import { useMemo, useState } from 'react';
import Button from './Button';
import Input from './Input';

const bankOptions = [
  { code: 'CBI', name: 'البنك المركزي العراقي', url: 'https://cbi.iq' },
  { code: 'RAF', name: 'مصرف الرافدين', url: 'http://rafidain-bank.gov.iq' },
  { code: 'RSH', name: 'مصرف الرشيد', url: 'https://rashidbank.gov.iq' },
  { code: 'TBI', name: 'المصرف العراقي للتجارة', url: 'https://tbi.com.iq' },
  { code: 'BOB', name: 'مصرف بغداد', url: 'https://bankofbaghdad.com' },
  { code: 'KIB', name: 'مصرف كردستان الدولي', url: 'https://www.kib.iq' },
  { code: 'CIH', name: 'مصرف جيهان', url: 'https://cihanbank.com' },
];

export default function BookingBox({ onSubmit, loading = false, serverError }) {
  const [form, setForm] = useState({
    bankCode: bankOptions[0].code,
    amount: '',
    date: '',
  });
  const [errors, setErrors] = useState({});

  const minDate = useMemo(() => new Date().toISOString().split('T')[0], []);

  const selectedBank = bankOptions.find((b) => b.code === form.bankCode) || bankOptions[0];

  const validate = () => {
    const nextErrors = {};
    if (!form.bankCode) nextErrors.bankCode = 'اختر المصرف';
    if (!form.amount || Number(form.amount) <= 0) nextErrors.amount = 'أدخل مبلغاً صحيحاً';
    if (!form.date) nextErrors.date = 'اختر تاريخ الاستلام';
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleChange = (field) => (value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    await onSubmit?.({
      bank: `${selectedBank.code} | ${selectedBank.name}`,
      amount: form.amount,
      date: form.date,
      bankUrl: selectedBank.url,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="glass rounded-2xl p-6 text-right text-beige">
      <div className="mb-4 flex flex-col gap-1">
        <p className="text-sm font-semibold text-brand">حجز مخصصات الدولار</p>
        <h2 className="text-2xl font-display font-semibold text-beige">طلب حجز الدولار</h2>
        <span className="badge w-fit">برنامج المصارف العراقية</span>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <label className="flex flex-col gap-2 text-sm font-semibold text-beige/90">
          المصرف
          <select
            className="w-full rounded-xl border border-beige/30 bg-surface px-4 py-3 text-base font-normal text-beige outline-none ring-brand/30 transition focus:border-brand focus:ring-4"
            value={form.bankCode}
            onChange={(e) => handleChange('bankCode')(e.target.value)}
          >
            {bankOptions.map((bank) => (
              <option key={bank.code} value={bank.code}>
                {bank.code} | {bank.name}
              </option>
            ))}
          </select>
          {errors.bankCode && <span className="text-sm font-medium text-danger">{errors.bankCode}</span>}
          <a
            href={selectedBank.url}
            target="_blank"
            rel="noreferrer noopener"
            className="text-sm font-semibold text-brand hover:text-brand-dark"
          >
            زيارة موقع المصرف
          </a>
        </label>

        <Input
          label="المبلغ (دولار)"
          type="number"
          min="50"
          step="50"
          placeholder="مثال: 1000"
          value={form.amount}
          onChange={handleChange('amount')}
          error={errors.amount}
          required
        />

        <Input
          label="تاريخ الاستلام"
          type="date"
          min={minDate}
          value={form.date}
          onChange={handleChange('date')}
          error={errors.date}
          required
        />

        <div className="flex flex-col gap-2 rounded-xl border border-dashed border-beige/20 bg-base/60 px-4 py-3 text-sm text-beige/80">
          <span className="font-semibold text-beige">ملاحظات سريعة</span>
          <span>- إبراز جواز السفر عند الاستلام</span>
          <span>- يحدد مكان التسليم بعد الموافقة</span>
          <span>- المراجعة تستغرق عادة 48 ساعة</span>
        </div>
      </div>

      {serverError && (
        <div className="mt-4 rounded-lg bg-danger/15 px-4 py-3 text-sm font-semibold text-danger">
          {serverError}
        </div>
      )}

      <div className="mt-6 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div className="text-sm text-beige/80">
          حالة التقديم: <span className="font-semibold text-brand">مفتوح</span>
        </div>
        <Button type="submit" loading={loading} className="w-full md:w-auto">
          إرسال الطلب
        </Button>
      </div>
    </form>
  );
}
