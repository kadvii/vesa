import { useState, useEffect } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import { getRequiredDocs, createRequiredDoc, updateRequiredDoc, deleteRequiredDoc } from '../services/api';

export default function Documents() {
  const [docs, setDocs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Create Form State
  const [form, setForm] = useState({ slug: '', name: '', description: '', isMandatory: true });
  const [submitting, setSubmitting] = useState(false);

  // Edit Modal State
  const [editingDoc, setEditingDoc] = useState(null);

  const fetchDocs = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getRequiredDocs();
      setDocs(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDocs();
  }, []);

  const handleAddDoc = async (e) => {
    e.preventDefault();
    if (!form.slug.trim() || !form.name.trim()) return;

    // Convert slug to standard format (lowercase, hyphens)
    const formattedSlug = form.slug.trim().toLowerCase().replace(/[^a-z0-9]+/g, '-');

    try {
      setSubmitting(true);
      await createRequiredDoc(formattedSlug, form.name, form.description, form.isMandatory);
      setForm({ slug: '', name: '', description: '', isMandatory: true });
      await fetchDocs();
    } catch (err) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdateDoc = async (e) => {
    e.preventDefault();
    if (!editingDoc) return;

    try {
      setSubmitting(true);
      await updateRequiredDoc(editingDoc.slug, editingDoc.name, editingDoc.description, editingDoc.isMandatory);
      setEditingDoc(null);
      await fetchDocs();
    } catch (err) {
      alert(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('هل أنت متأكد من حذف هذا المستند تماماً من النظام؟')) return;
    try {
      await deleteRequiredDoc(id);
      await fetchDocs();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="space-y-6 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">إدارة النظام</p>
          <h1 className="text-2xl font-display font-semibold text-beige">المستندات المطلوبة</h1>
        </div>
        <Button onClick={fetchDocs} variant="ghost" disabled={loading}>
          تحديث البيانات 🔄
        </Button>
      </div>

      {error && (
        <div className="rounded-xl bg-red-500/10 border border-red-500/20 p-4 text-red-400 text-sm">
          {error}
        </div>
      )}

      {/* Add New Document Form */}
      <Card className="space-y-4">
        <h2 className="text-lg font-semibold text-white">إضافة مستند جديد للطلبات</h2>
        <form className="grid gap-4 md:grid-cols-2 lg:grid-cols-5 items-start" onSubmit={handleAddDoc}>
          <div className="space-y-1 lg:col-span-1">
            <label className="text-xs text-beige/70">المعرف البرمجي (Slug)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30"
              placeholder="مثال: passport-copy"
              maxLength={50}
              value={form.slug}
              onChange={(e) => setForm((p) => ({ ...p, slug: e.target.value }))}
              required
            />
          </div>
          <div className="space-y-1 lg:col-span-1">
            <label className="text-xs text-beige/70">اسم المستند (عربي)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30"
              placeholder="مثال: صورة الجواز"
              value={form.name}
              onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
              required
            />
          </div>
          <div className="space-y-1 lg:col-span-1">
            <label className="text-xs text-beige/70">ملاحظات / وصف (اختياري)</label>
            <input
              className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand placeholder:text-beige/30"
              placeholder="يجب أن تكون صورة واضحة"
              value={form.description}
              onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
            />
          </div>
          
          <div className="space-y-1 lg:col-span-1 flex flex-col justify-center h-[60px]">
            <label className="flex items-center gap-2 cursor-pointer mt-5">
              <input 
                type="checkbox" 
                checked={form.isMandatory}
                onChange={(e) => setForm((p) => ({ ...p, isMandatory: e.target.checked }))}
                className="w-4 h-4 accent-brand cursor-pointer"
              />
              <span className="text-sm text-beige select-none text-red-300 font-semibold">مستند إلزامي أساسي</span>
            </label>
          </div>

          <div className="lg:col-span-1 flex items-end h-[60px]">
            <Button type="submit" loading={submitting} className="w-full">
              إضافة المستند
            </Button>
          </div>
        </form>
      </Card>

      {/* Data Table */}
      <Card className="overflow-hidden p-0 sm:p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-right text-sm">
            <thead className="bg-surface/50 text-beige/70">
              <tr>
                <th className="px-6 py-4 font-semibold w-40">المعرف (Slug)</th>
                <th className="px-6 py-4 font-semibold w-48">اسم المستند</th>
                <th className="px-6 py-4 font-semibold">الوصف والشروط</th>
                <th className="px-6 py-4 font-semibold text-center w-32">الأهمية</th>
                <th className="px-6 py-4 font-semibold text-center w-40">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-beige/10">
              {loading && docs.length === 0 ? (
                <tr>
                  <td colSpan="5" className="px-6 py-8 text-center text-beige/50">
                    جاري التحميل...
                  </td>
                </tr>
              ) : docs.length === 0 ? (
                <tr>
                  <td colSpan="5" className="px-6 py-8 text-center text-beige/50">
                    لا توجد مستندات مسجلة حالياً.
                  </td>
                </tr>
              ) : (
                docs.map((doc) => (
                  <tr key={doc.id} className="hover:bg-surface/30 transition-colors">
                    <td className="px-6 py-4 font-mono text-xs text-beige/60">{doc.slug}</td>
                    <td className="px-6 py-4 font-semibold text-white">{doc.name}</td>
                    <td className="px-6 py-4 text-beige/70">{doc.description || '—'}</td>
                    <td className="px-6 py-4 text-center">
                      <span className={`inline-flex items-center justify-center px-2 py-1 text-xs font-semibold rounded-md border ${
                        doc.isMandatory 
                          ? 'border-red-500/30 bg-red-500/10 text-red-400' 
                          : 'border-slate-500/30 bg-slate-500/10 text-slate-400'
                      }`}>
                        {doc.isMandatory ? 'إلزامي' : 'اختياري'}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex justify-center gap-2">
                        <Button 
                          variant="ghost" 
                          className="px-3 py-1 text-xs border-brand text-brand hover:bg-brand/10"
                          onClick={() => setEditingDoc(doc)}
                        >
                          تعديل
                        </Button>
                        <Button 
                          variant="ghost" 
                          className="px-3 py-1 text-xs border-red-500 text-red-500 hover:bg-red-500/10"
                          onClick={() => handleDelete(doc.id)}
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
      {editingDoc && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm">
          <Card className="w-full max-w-md space-y-6 bg-base border border-brand/30 shadow-[0_0_50px_rgba(212,175,55,0.1)]">
            <h3 className="text-xl font-bold text-white">تعديل المستند: {editingDoc.slug}</h3>
            
            <form onSubmit={handleUpdateDoc} className="space-y-4">
              <div className="space-y-1">
                <label className="text-xs text-beige/70">اسم المستند (عربي)</label>
                <input
                  className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-white outline-none focus:border-brand"
                  required
                  value={editingDoc.name}
                  onChange={(e) => setEditingDoc(c => ({ ...c, name: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs text-beige/70">وصف وشروط إضافية</label>
                <input
                  className="w-full rounded-xl border border-beige/30 bg-surface px-3 py-2 text-white outline-none focus:border-brand"
                  value={editingDoc.description || ''}
                  onChange={(e) => setEditingDoc(c => ({ ...c, description: e.target.value }))}
                />
              </div>
              
              <div className="flex items-center gap-3 pt-2 pb-2">
                <input 
                  type="checkbox" 
                  id="isMandatoryToggle"
                  checked={editingDoc.isMandatory}
                  onChange={(e) => setEditingDoc(c => ({ ...c, isMandatory: e.target.checked }))}
                  className="w-4 h-4 accent-brand cursor-pointer"
                />
                <label htmlFor="isMandatoryToggle" className="text-sm text-red-300 font-semibold cursor-pointer select-none">
                  مستند إلزامي وأساسي لتقديم الطلب
                </label>
              </div>

              <div className="flex gap-3 pt-4 border-t border-beige/10">
                <Button type="button" variant="ghost" className="flex-1" onClick={() => setEditingDoc(null)}>
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
