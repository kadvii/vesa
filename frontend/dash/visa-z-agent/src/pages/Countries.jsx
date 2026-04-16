import { useState, useEffect } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import { getCountries, createCountry, updateCountry, deleteCountry } from '../services/api';

export default function Countries() {
  const [countries, setCountries] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Create Form State
  const [form, setForm] = useState({ isoCode: '', name: '', description: '', isActive: true });
  const [submitting, setSubmitting] = useState(false);

  // Edit Modal State
  const [editingCountry, setEditingCountry] = useState(null);

  const fetchCountries = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getCountries();
      setCountries(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCountries();
  }, []);

  const handleAddCountry = async (e) => {
    e.preventDefault();
    if (!form.isoCode.trim() || !form.name.trim()) return;

    try {
      setSubmitting(true);
      await createCountry(form.isoCode, form.name, form.description, form.isActive);
      setForm({ isoCode: '', name: '', description: '', isActive: true });
      await fetchCountries();
    } catch (err) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdateCountry = async (e) => {
    e.preventDefault();
    if (!editingCountry) return;

    try {
      setSubmitting(true);
      await updateCountry(editingCountry.isoCode, editingCountry.name, editingCountry.description, editingCountry.isActive);
      setEditingCountry(null);
      await fetchCountries();
    } catch (err) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleToggleStatus = async (country) => {
    try {
      // Optmistic UI Update could go here, but let's await the API to be safe
      await updateCountry(country.isoCode, country.name, country.description, !country.isActive);
      await fetchCountries();
    } catch (err) {
      alert(err.message);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('هل أنت متأكد من حذف هذه الدولة تماماً من النظام؟')) return;
    try {
      await deleteCountry(id);
      await fetchCountries();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="space-y-6 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">إدارة النظام</p>
          <h1 className="text-2xl font-display font-semibold text-beige">الدول والوجهات</h1>
        </div>
        <Button onClick={fetchCountries} variant="ghost" disabled={loading}>
          تحديث البيانات 🔄
        </Button>
      </div>

      {error && (
        <div className="rounded-xl bg-red-500/10 border border-red-500/20 p-4 text-red-400 text-sm">
          {error}
        </div>
      )}

      {/* Add New Country Form */}
      <Card className="space-y-4">
        <h2 className="text-lg font-semibold text-white">إضافة وجهة جديدة</h2>
        <form className="grid gap-3 md:grid-cols-4 items-end" onSubmit={handleAddCountry}>
          <div className="space-y-1">
            <label className="text-xs text-beige/70">رمز الدولة (IsoCode)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30 uppercase"
              placeholder="مثال: AE"
              maxLength={10}
              value={form.isoCode}
              onChange={(e) => setForm((p) => ({ ...p, isoCode: e.target.value.toUpperCase() }))}
              required
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs text-beige/70">اسم الدولة (عربي)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30"
              placeholder="مثال: الإمارات"
              value={form.name}
              onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
              required
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs text-beige/70">الاسم التجاري / إنجليزي (اختياري)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30"
              placeholder="مثال: UAE"
              value={form.description}
              onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
            />
          </div>
          <Button type="submit" loading={submitting}>
            إضافة الدولة
          </Button>
        </form>
      </Card>

      {/* Data Table */}
      <Card className="overflow-hidden p-0 sm:p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-right text-sm">
            <thead className="bg-surface/50 text-beige/70">
              <tr>
                <th className="px-6 py-4 font-semibold w-24">الرمز (العلم)</th>
                <th className="px-6 py-4 font-semibold w-48">الاسم (عربي)</th>
                <th className="px-6 py-4 font-semibold">تفاصيل</th>
                <th className="px-6 py-4 font-semibold text-center w-32">الحالة</th>
                <th className="px-6 py-4 font-semibold text-center w-40">العمليات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-beige/10">
              {loading && countries.length === 0 ? (
                <tr>
                  <td colSpan="5" className="px-6 py-8 text-center text-beige/50">
                    جاري التحميل...
                  </td>
                </tr>
              ) : countries.length === 0 ? (
                <tr>
                  <td colSpan="5" className="px-6 py-8 text-center text-beige/50">
                    لا توجد دول مسجلة حالياً. قم بإضافة الوجهات المتاحة.
                  </td>
                </tr>
              ) : (
                countries.map((c) => (
                  <tr key={c.id} className="hover:bg-surface/30 transition-colors">
                    <td className="px-6 py-4 font-mono font-bold text-brand flex items-center gap-2">
                       {/* Extremely simple flag emoji generator using ISO alpha-2 trick */}
                      {c.isoCode.length === 2 && (
                        <span className="text-lg">
                          {String.fromCodePoint(...[...c.isoCode.toUpperCase()].map(char => char.charCodeAt(0) + 127397))}
                        </span>
                      )}
                      <span>{c.isoCode}</span>
                    </td>
                    <td className="px-6 py-4 font-semibold text-white">{c.name}</td>
                    <td className="px-6 py-4 text-beige/70">{c.description || '—'}</td>
                    <td className="px-6 py-4 text-center">
                      <button
                        onClick={() => handleToggleStatus(c)}
                        className={`px-3 py-1 text-xs rounded-full border transition-all ${
                          c.isActive 
                            ? 'border-emerald-500/40 bg-emerald-500/10 text-emerald-400 hover:bg-emerald-500/20' 
                            : 'border-red-500/40 bg-red-500/10 text-red-400 hover:bg-red-500/20'
                        }`}
                      >
                        {c.isActive ? 'مفعل (يعمل)' : 'معطل (موقوف)'}
                      </button>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex justify-center gap-2">
                        <Button 
                          variant="ghost" 
                          className="px-3 py-1 text-xs border-brand text-brand hover:bg-brand/10"
                          onClick={() => setEditingCountry(c)}
                        >
                          تعديل
                        </Button>
                        <Button 
                          variant="ghost" 
                          className="px-3 py-1 text-xs border-red-500 text-red-500 hover:bg-red-500/10"
                          onClick={() => handleDelete(c.id)}
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

      {/* Edit Modal (Glassmorphism overlay) */}
      {editingCountry && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
          <Card className="w-full max-w-md space-y-6 bg-base border border-brand/30 shadow-[0_0_50px_rgba(212,175,55,0.1)]">
            <h3 className="text-xl font-bold text-white">تعديل دولة: {editingCountry.isoCode}</h3>
            
            <form onSubmit={handleUpdateCountry} className="space-y-4">
              <div className="space-y-1">
                <label className="text-xs text-beige/70">اسم الدولة (عربي)</label>
                <input
                  className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-white outline-none focus:border-brand"
                  required
                  value={editingCountry.name}
                  onChange={(e) => setEditingCountry(c => ({ ...c, name: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs text-beige/70">الاسم التجاري / إنجليزي</label>
                <input
                  className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-white outline-none focus:border-brand"
                  value={editingCountry.description || ''}
                  onChange={(e) => setEditingCountry(c => ({ ...c, description: e.target.value }))}
                />
              </div>
              
              <div className="flex items-center gap-3 pt-2 pb-2">
                <input 
                  type="checkbox" 
                  id="isActiveToggle"
                  checked={editingCountry.isActive}
                  onChange={(e) => setEditingCountry(c => ({ ...c, isActive: e.target.checked }))}
                  className="w-4 h-4 accent-brand cursor-pointer"
                />
                <label htmlFor="isActiveToggle" className="text-sm text-beige cursor-pointer select-none">
                  دولة متاحة لاستقبال الطلبات (مفعلة)
                </label>
              </div>

              <div className="flex gap-3 pt-4 border-t border-beige/10">
                <Button type="button" variant="ghost" className="flex-1" onClick={() => setEditingCountry(null)}>
                  إلغاء التعديل
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
