import { useEffect, useState, useCallback } from 'react';
import { api } from '../services/api';

/* ─── Status badge colours ──────────────────────────────────────────────── */
const STATUS_CFG = {
  Pending:  { bg: '#f59e0b22', border: '#f59e0b', color: '#fbbf24', label: 'قيد الانتظار' },
  Approved: { bg: '#10b98122', border: '#10b981', color: '#34d399', label: 'مقبول'        },
  Rejected: { bg: '#ef444422', border: '#ef4444', color: '#f87171', label: 'مرفوض'       },
};

const VISA_TYPE_AR = {
  Tourist: 'سياحية', Business: 'أعمال', Student: 'دراسية',
  Work: 'عمل', Transit: 'عبور', Medical: 'علاجية',
};

/* ─── Modal ─────────────────────────────────────────────────────────────── */
function ConfirmModal({ item, action, onConfirm, onCancel }) {
  const [notes, setNotes] = useState('');
  const isApprove = action === 'Approved';

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        <div style={styles.modalHeader}>
          <span style={{ fontSize: 28 }}>{isApprove ? '✅' : '❌'}</span>
          <h2 style={styles.modalTitle}>
            {isApprove ? 'تأكيد القبول' : 'تأكيد الرفض'}
          </h2>
        </div>

        <p style={styles.modalSub}>
          الطلب رقم: <strong style={{ color: '#e2e8f0' }}>{item.reference}</strong>
          {' — '}{item.applicantName}
        </p>

        <label style={styles.label}>ملاحظات المراجع (اختياري)</label>
        <textarea
          id="review-notes"
          style={styles.textarea}
          rows={3}
          placeholder={isApprove ? 'أدخل سبب القبول أو ملاحظات…' : 'أدخل سبب الرفض…'}
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />

        <div style={styles.modalActions}>
          <button id="modal-cancel" style={styles.btnSecondary} onClick={onCancel}>
            إلغاء
          </button>
          <button
            id="modal-confirm"
            style={isApprove ? styles.btnApprove : styles.btnReject}
            onClick={() => onConfirm(notes || null)}
          >
            {isApprove ? 'قبول الطلب' : 'رفض الطلب'}
          </button>
        </div>
      </div>
    </div>
  );
}

/* ─── Toast ─────────────────────────────────────────────────────────────── */
function Toast({ msg, type }) {
  if (!msg) return null;
  const bg = type === 'error' ? '#7f1d1d' : '#064e3b';
  const border = type === 'error' ? '#ef4444' : '#10b981';
  return (
    <div style={{ ...styles.toast, background: bg, borderColor: border }}>
      <span>{type === 'error' ? '⛔' : '✅'}</span> {msg}
    </div>
  );
}

/* ─── Status Badge ──────────────────────────────────────────────────────── */
function Badge({ status }) {
  const cfg = STATUS_CFG[status] || STATUS_CFG.Pending;
  return (
    <span style={{
      padding: '3px 12px', borderRadius: 20,
      fontSize: 12, fontWeight: 700,
      background: cfg.bg, border: `1px solid ${cfg.border}`, color: cfg.color,
    }}>
      {cfg.label}
    </span>
  );
}

/* ─── Main Component ────────────────────────────────────────────────────── */
export default function AdminVisaQueue() {
  const [items,    setItems]    = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [error,    setError]    = useState('');
  const [toast,    setToast]    = useState({ msg: '', type: 'success' });
  const [page,     setPage]     = useState(1);
  const [total,    setTotal]    = useState(0);
  const [filter,   setFilter]   = useState('all');   // all | Pending | Approved | Rejected
  const [search,   setSearch]   = useState('');
  const [modal,    setModal]    = useState(null);    // { item, action }
  const [acting,   setActing]   = useState(false);

  const PAGE_SIZE = 20;

  const showToast = (msg, type = 'success') => {
    setToast({ msg, type });
    setTimeout(() => setToast({ msg: '', type: 'success' }), 3500);
  };

  const fetchVisas = useCallback(async (pg = page) => {
    setLoading(true);
    setError('');
    try {
      const res = await api.admin.getAllVisas(pg, PAGE_SIZE);
      setItems(res.items);
      setTotal(res.totalCount);
    } catch (e) {
      setError(e.message || 'فشل تحميل الطلبات');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => { fetchVisas(page); }, [page]);

  /* ── filter + search client-side ── */
  const displayed = items.filter((v) => {
    const matchFilter = filter === 'all' || v.status === filter;
    const q = search.trim().toLowerCase();
    const matchSearch = !q ||
      (v.applicantName || '').toLowerCase().includes(q) ||
      (v.reference    || '').toLowerCase().includes(q) ||
      (v.destination  || '').toLowerCase().includes(q);
    return matchFilter && matchSearch;
  });

  /* ── approve / reject ── */
  const handleAction = async (notes) => {
    if (!modal) return;
    setActing(true);
    try {
      await api.admin.patchVisaStatus(modal.item.id, modal.action, notes);
      showToast(
        modal.action === 'Approved'
          ? `تم قبول الطلب ${modal.item.reference} بنجاح`
          : `تم رفض الطلب ${modal.item.reference}`,
        'success',
      );
      setModal(null);
      fetchVisas(page);
    } catch (e) {
      showToast(e.message || 'فشلت العملية', 'error');
      setActing(false);
    }
  };

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  /* ─────────────────────── RENDER ─────────────────────── */
  return (
    <div style={styles.root} dir="rtl">
      {/* ── Header ── */}
      <div style={styles.header}>
        <div>
          <h1 style={styles.title}>لوحة تحكم الادمن — طلبات التأشيرة</h1>
          <p style={styles.sub}>مراجعة وقبول أو رفض طلبات المتقدمين</p>
        </div>
        <div style={styles.statsRow}>
          <StatChip label="إجمالي" count={total} color="#6366f1" />
          <StatChip label="معلق"   count={items.filter(i => i.status === 'Pending').length}  color="#f59e0b" />
          <StatChip label="مقبول"  count={items.filter(i => i.status === 'Approved').length} color="#10b981" />
          <StatChip label="مرفوض"  count={items.filter(i => i.status === 'Rejected').length} color="#ef4444" />
        </div>
      </div>

      {/* ── Toolbar ── */}
      <div style={styles.toolbar}>
        <input
          id="admin-search"
          style={styles.searchInput}
          placeholder="🔍  بحث بالاسم أو الرقم المرجعي…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <div style={styles.filters}>
          {['all', 'Pending', 'Approved', 'Rejected'].map((f) => (
            <button
              key={f}
              id={`filter-${f}`}
              style={{ ...styles.filterBtn, ...(filter === f ? styles.filterActive : {}) }}
              onClick={() => setFilter(f)}
            >
              {f === 'all' ? 'الكل' : STATUS_CFG[f]?.label || f}
            </button>
          ))}
        </div>
        <button id="refresh-btn" style={styles.refreshBtn} onClick={() => fetchVisas(page)} title="تحديث">
          🔄
        </button>
      </div>

      {/* ── Body ── */}
      {loading ? (
        <div style={styles.center}><div style={styles.spinner} /></div>
      ) : error ? (
        <div style={styles.errorBox}>
          <p>⛔ {error}</p>
          <button id="retry-btn" style={styles.btnApprove} onClick={() => fetchVisas(page)}>إعادة المحاولة</button>
        </div>
      ) : displayed.length === 0 ? (
        <div style={styles.empty}>لا توجد طلبات مطابقة</div>
      ) : (
        <>
          <div style={styles.tableWrapper}>
            <table style={styles.table}>
              <thead>
                <tr>
                  {['#', 'المرجع', 'المتقدم', 'نوع التأشيرة', 'الوجهة', 'تاريخ التقديم', 'الحالة', 'الإجراء'].map((h) => (
                    <th key={h} style={styles.th}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {displayed.map((v, idx) => (
                  <tr key={v.id} style={styles.tr} onMouseEnter={e => e.currentTarget.style.background = '#1e293b'} onMouseLeave={e => e.currentTarget.style.background = 'transparent'}>
                    <td style={styles.td}>{(page - 1) * PAGE_SIZE + idx + 1}</td>
                    <td style={{ ...styles.td, fontFamily: 'monospace', color: '#94a3b8', fontSize: 13 }}>{v.reference}</td>
                    <td style={{ ...styles.td, fontWeight: 600, color: '#e2e8f0' }}>{v.applicantName || '—'}</td>
                    <td style={styles.td}>{VISA_TYPE_AR[v.visaType] || v.visaType || '—'}</td>
                    <td style={styles.td}>{v.destination || '—'}</td>
                    <td style={{ ...styles.td, color: '#94a3b8', fontSize: 13 }}>
                      {v.submissionDate ? new Date(v.submissionDate).toLocaleDateString('ar-SA') : '—'}
                    </td>
                    <td style={styles.td}><Badge status={v.status} /></td>
                    <td style={styles.td}>
                      {v.status === 'Pending' ? (
                        <div style={{ display: 'flex', gap: 8 }}>
                          <button
                            id={`approve-${v.id}`}
                            style={styles.btnApprove}
                            disabled={acting}
                            onClick={() => setModal({ item: v, action: 'Approved' })}
                          >
                            ✓ قبول
                          </button>
                          <button
                            id={`reject-${v.id}`}
                            style={styles.btnReject}
                            disabled={acting}
                            onClick={() => setModal({ item: v, action: 'Rejected' })}
                          >
                            ✗ رفض
                          </button>
                        </div>
                      ) : (
                        <span style={{ color: '#64748b', fontSize: 13 }}>تمت المراجعة</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* ── Pagination ── */}
          {totalPages > 1 && (
            <div style={styles.pagination}>
              <button id="prev-page" style={styles.pageBtn} disabled={page <= 1} onClick={() => setPage(p => p - 1)}>
                → السابق
              </button>
              <span style={{ color: '#94a3b8', fontSize: 14 }}>صفحة {page} من {totalPages}</span>
              <button id="next-page" style={styles.pageBtn} disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>
                التالي ←
              </button>
            </div>
          )}
        </>
      )}

      {/* ── Modal ── */}
      {modal && (
        <ConfirmModal
          item={modal.item}
          action={modal.action}
          onConfirm={handleAction}
          onCancel={() => { if (!acting) setModal(null); }}
        />
      )}

      {/* ── Toast ── */}
      <Toast msg={toast.msg} type={toast.type} />
    </div>
  );
}

/* ─── StatChip ──────────────────────────────────────────────────────────── */
function StatChip({ label, count, color }) {
  return (
    <div style={{ textAlign: 'center', background: '#1e293b', border: `1px solid ${color}33`, borderRadius: 12, padding: '8px 20px', minWidth: 70 }}>
      <div style={{ fontSize: 22, fontWeight: 800, color }}>{count}</div>
      <div style={{ fontSize: 11, color: '#64748b', marginTop: 2 }}>{label}</div>
    </div>
  );
}

/* ─── Styles ────────────────────────────────────────────────────────────── */
const styles = {
  root: {
    minHeight: '100vh',
    background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)',
    padding: '32px 24px',
    fontFamily: "'Segoe UI', Tahoma, Arial, sans-serif",
    position: 'relative',
  },
  header: {
    display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start',
    flexWrap: 'wrap', gap: 16, marginBottom: 28,
  },
  title: { fontSize: 26, fontWeight: 800, color: '#f1f5f9', margin: 0 },
  sub:   { fontSize: 14, color: '#64748b', marginTop: 4 },
  statsRow: { display: 'flex', gap: 12, flexWrap: 'wrap' },
  toolbar: {
    display: 'flex', gap: 12, alignItems: 'center',
    flexWrap: 'wrap', marginBottom: 20,
  },
  searchInput: {
    flex: 1, minWidth: 220, padding: '10px 16px',
    background: '#1e293b', border: '1px solid #334155',
    borderRadius: 10, color: '#e2e8f0', fontSize: 14,
    outline: 'none',
  },
  filters: { display: 'flex', gap: 6 },
  filterBtn: {
    padding: '8px 14px', borderRadius: 8, border: '1px solid #334155',
    background: 'transparent', color: '#94a3b8', cursor: 'pointer',
    fontSize: 13, fontFamily: 'inherit', transition: 'all 0.2s',
  },
  filterActive: {
    background: '#6366f1', borderColor: '#6366f1', color: '#fff',
  },
  refreshBtn: {
    padding: '8px 14px', borderRadius: 8, border: '1px solid #334155',
    background: 'transparent', color: '#94a3b8', cursor: 'pointer', fontSize: 18,
  },
  tableWrapper: { overflowX: 'auto', borderRadius: 14, border: '1px solid #1e293b' },
  table: { width: '100%', borderCollapse: 'collapse', minWidth: 900 },
  th: {
    padding: '14px 16px', background: '#1e293b',
    color: '#64748b', fontSize: 12, fontWeight: 700,
    textAlign: 'right', whiteSpace: 'nowrap',
    borderBottom: '1px solid #334155',
  },
  tr: { transition: 'background 0.15s', cursor: 'default' },
  td: {
    padding: '13px 16px', color: '#cbd5e1', fontSize: 14,
    borderBottom: '1px solid #1e293b', whiteSpace: 'nowrap',
  },
  btnApprove: {
    padding: '7px 14px', borderRadius: 8, border: 'none',
    background: 'linear-gradient(135deg,#059669,#10b981)',
    color: '#fff', cursor: 'pointer', fontSize: 13,
    fontWeight: 700, fontFamily: 'inherit', transition: 'opacity 0.2s',
  },
  btnReject: {
    padding: '7px 14px', borderRadius: 8, border: 'none',
    background: 'linear-gradient(135deg,#dc2626,#ef4444)',
    color: '#fff', cursor: 'pointer', fontSize: 13,
    fontWeight: 700, fontFamily: 'inherit', transition: 'opacity 0.2s',
  },
  btnSecondary: {
    padding: '10px 20px', borderRadius: 10, border: '1px solid #334155',
    background: 'transparent', color: '#94a3b8', cursor: 'pointer',
    fontSize: 14, fontFamily: 'inherit',
  },
  pagination: {
    display: 'flex', justifyContent: 'center', alignItems: 'center',
    gap: 16, marginTop: 24,
  },
  pageBtn: {
    padding: '8px 18px', borderRadius: 8, border: '1px solid #334155',
    background: '#1e293b', color: '#94a3b8', cursor: 'pointer',
    fontFamily: 'inherit', fontSize: 14,
  },
  center: { display: 'flex', justifyContent: 'center', padding: '80px 0' },
  spinner: {
    width: 48, height: 48, borderRadius: '50%',
    border: '4px solid #1e293b', borderTop: '4px solid #6366f1',
    animation: 'spin 0.8s linear infinite',
  },
  errorBox: {
    textAlign: 'center', padding: 48, color: '#f87171', fontSize: 16,
  },
  empty: {
    textAlign: 'center', padding: 60, color: '#475569', fontSize: 16,
  },
  overlay: {
    position: 'fixed', inset: 0, background: '#00000088',
    display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 9999,
    backdropFilter: 'blur(4px)',
  },
  modal: {
    background: '#1e293b', border: '1px solid #334155',
    borderRadius: 18, padding: '32px 36px', width: 420, maxWidth: '90vw',
    boxShadow: '0 25px 60px #00000066',
  },
  modalHeader: { display: 'flex', alignItems: 'center', gap: 12, marginBottom: 12 },
  modalTitle: { fontSize: 20, fontWeight: 800, color: '#f1f5f9', margin: 0 },
  modalSub: { color: '#94a3b8', fontSize: 14, marginBottom: 20 },
  label: { display: 'block', color: '#64748b', fontSize: 13, marginBottom: 8 },
  textarea: {
    width: '100%', padding: '10px 14px', background: '#0f172a',
    border: '1px solid #334155', borderRadius: 10, color: '#e2e8f0',
    fontSize: 14, fontFamily: 'inherit', resize: 'vertical', boxSizing: 'border-box',
    outline: 'none', marginBottom: 24,
  },
  modalActions: { display: 'flex', justifyContent: 'flex-end', gap: 10 },
  toast: {
    position: 'fixed', bottom: 28, left: '50%', transform: 'translateX(-50%)',
    padding: '14px 28px', borderRadius: 12, border: '1px solid',
    color: '#f1f5f9', fontSize: 15, fontWeight: 600,
    display: 'flex', alignItems: 'center', gap: 10,
    boxShadow: '0 8px 24px #00000055', zIndex: 99999,
    animation: 'fadeIn 0.3s ease',
  },
};

/* ─── CSS animation injection ────────────────────────────────────────────── */
if (typeof document !== 'undefined' && !document.getElementById('admin-queue-anim')) {
  const s = document.createElement('style');
  s.id = 'admin-queue-anim';
  s.textContent = `
    @keyframes spin   { to { transform: rotate(360deg); } }
    @keyframes fadeIn { from { opacity:0; transform:translateX(-50%) translateY(10px); }
                        to   { opacity:1; transform:translateX(-50%) translateY(0);     } }
  `;
  document.head.appendChild(s);
}
