import { useState, useEffect } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import { getPrices, createPrice, updatePrice, deletePrice } from '../services/api';

export default function Prices() {
  const [prices, setPrices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Create Form State
  const [form, setForm] = useState({ isoCode: '', amountUsd: '', description: '' });
  const [submitting, setSubmitting] = useState(false);

  // Edit Modal State
  const [editingPrice, setEditingPrice] = useState(null);

  const fetchPrices = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getPrices();
      setPrices(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPrices();
  }, []);

  const handleAddPrice = async (e) => {
    e.preventDefault();
    if (!form.isoCode.trim() || !form.amountUsd) return;

    try {
      setSubmitting(true);
      await createPrice(form.isoCode, Number(form.amountUsd), form.description);
      setForm({ isoCode: '', amountUsd: '', description: '' });
      await fetchPrices();
    } catch (err) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdatePrice = async (e) => {
    e.preventDefault();
    if (!editingPrice) return;

    try {
      setSubmitting(true);
      await updatePrice(editingPrice.isoCode, Number(editingPrice.amountUsd), editingPrice.description);
      setEditingPrice(null);
      await fetchPrices();
    } catch (err) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('هل أنت متأكد من حذف هذا السعر؟')) return;
    try {
      await deletePrice(id);
      await fetchPrices();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="space-y-6 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">إدارة النظام</p>
          <h1 className="text-2xl font-display font-semibold text-beige">أسعار التأشيرات</h1>
        </div>
        <Button onClick={fetchPrices} variant="ghost" disabled={loading}>
          تحديث البيانات 🔄
        </Button>
      </div>

      {error && (
        <div className="rounded-xl bg-red-500/10 border border-red-500/20 p-4 text-red-400 text-sm">
          {error}
        </div>
      )}

      {/* Add New Price Form */}
      <Card className="space-y-4">
        <h2 className="text-lg font-semibold text-white">إضافة تسعيرة جديدة</h2>
        <form className="grid gap-3 md:grid-cols-4 items-end" onSubmit={handleAddPrice}>
          <div className="space-y-1">
            <label className="text-xs text-beige/70">رمز الدولة (IsoCode)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30 uppercase"
              placeholder="مثال: TR"
              maxLength={10}
              value={form.isoCode}
              onChange={(e) => setForm((p) => ({ ...p, isoCode: e.target.value.toUpperCase() }))}
              required
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs text-beige/70">السعر ($)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30"
              placeholder="0.00"
              type="number"
              step="0.01"
              value={form.amountUsd}
              onChange={(e) => setForm((p) => ({ ...p, amountUsd: e.target.value }))}
              required
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs text-beige/70">ملاحظات والتفاصيل</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30"
              placeholder="مثال: تأشيرة سياحية تركية"
              value={form.description}
              onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
            />
          </div>
          <Button type="submit" loading={submitting}>
            إضافة السعر
          </Button>
        </form>
      </Card>

      {/* Data Table */}
      <Card className="overflow-hidden p-0 sm:p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-right text-sm">
            <thead className="bg-surface/50 text-beige/70">
              <tr>
                <th className="px-6 py-4 font-semibold w-24">الرمز</th>
                <th className="px-6 py-4 font-semibold">التفاصيل</th>
                <th className="px-6 py-4 font-semibold w-32">السعر (USD)</th>
                <th className="px-6 py-4 font-semibold w-48">تاريخ التحديث</th>
                <th className="px-6 py-4 font-semibold text-center w-32">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-beige/10">
              {loading && prices.length === 0 ? (
                <tr>
                  <td colSpan="5" className="px-6 py-8 text-center text-beige/50">
                    جاري التحميل...
                  </td>
                </tr>
              ) : prices.length === 0 ? (
                <tr>
                  <td colSpan="5" className="px-6 py-8 text-center text-beige/50">
                    لا توجد بيانات مسجلة حالياً.
                  </td>
                </tr>
              ) : (
                prices.map((p) => (
                  <tr key={p.id} className="hover:bg-surface/30 transition-colors">
                    <td className="px-6 py-4 font-mono font-bold text-brand">{p.isoCode}</td>
                    <td className="px-6 py-4">{p.description || <span className="text-beige/30">بدون تفاصيل</span>}</td>
                    <td className="px-6 py-4 font-semibold text-emerald-400">${Number(p.amountUsd).toFixed(2)}</td>
                    <td className="px-6 py-4 text-xs text-beige/60">
                      {new Date(p.updatedAt).toLocaleDateString('ar-EG', { dateStyle: 'medium' })}
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex justify-center gap-2">
                        <Button 
                          variant="ghost" 
                          className="px-3 py-1 text-xs border-brand text-brand hover:bg-brand/10"
                          onClick={() => setEditingPrice(p)}
                        >
                          تعديل
                        </Button>
                        <Button 
                          variant="ghost" 
                          className="px-3 py-1 text-xs border-red-500 text-red-500 hover:bg-red-500/10"
                          onClick={() => handleDelete(p.id)}
                        >
                          حذف
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Edit Modal (Simple overlay) */}
      {editingPrice && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
          <Card className="w-full max-w-md space-y-6 bg-base">
            <h3 className="text-xl font-bold text-white">تعديل تسعيرة: {editingPrice.isoCode}</h3>
            
            <form onSubmit={handleUpdatePrice} className="space-y-4">
              <div className="space-y-1">
                <label className="text-xs text-beige/70">السعر الجديد (USD)</label>
                <input
                  className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-white outline-none focus:border-brand"
                  type="number"
                  step="0.01"
                  required
                  value={editingPrice.amountUsd}
                  onChange={(e) => setEditingPrice(p => ({ ...p, amountUsd: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs text-beige/70">ملاحظات والتفاصيل</label>
                <input
                  className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-white outline-none focus:border-brand"
                  value={editingPrice.description || ''}
                  onChange={(e) => setEditingPrice(p => ({ ...p, description: e.target.value }))}
                />
              </div>
              
              <div className="flex gap-3 pt-2">
                <Button type="button" variant="ghost" className="flex-1" onClick={() => setEditingPrice(null)}>
                  إلغاء
                </Button>
                <Button type="submit" className="flex-1" loading={submitting}>
                  حفظ التعديلات
                </Button>
              </div>
            </form>
          </Card>
        </div>
      )}
    </div>
  );
}
