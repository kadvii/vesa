import { useMemo, useRef, useState } from 'react';
import Button from '../components/Button';
import Input from '../components/Input';
import Card from '../components/Card';
import { createVisa, uploadPassportDocument } from '../services/api';

const visaTypes = [
  { value: 'tourist',  label: 'سياحية'  },
  { value: 'business', label: 'أعمال'   },
  { value: 'student',  label: 'دراسية'  },
  { value: 'medical',  label: 'علاجية'  },
];

const countries = ['تركيا', 'الإمارات', 'الأردن', 'قطر', 'السعودية', 'مصر', 'ألمانيا'];

// Accepted MIME types shown in the browser file picker
const ACCEPTED_TYPES = '.jpg,.jpeg,.png,.pdf';
// Max 5 MB — mirrors the backend guard
const MAX_BYTES = 5 * 1024 * 1024;

// ── Phase indicators ────────────────────────────────────────────────────────
const PHASE = {
  IDLE:       'idle',       // form not yet submitted
  SUBMITTING: 'submitting', // step 1 — creating the visa application
  UPLOADING:  'uploading',  // step 2 — uploading the passport scan
  DONE:       'done',       // both steps succeeded
};

export default function VisaBooking() {
  const [form, setForm] = useState({
    fullName:    '',
    passport:    '',
    nationality: '',
    country:     countries[0],
    date:        '',
    type:        'tourist',
  });

  // Passport file — stored separately from the form text fields
  const [passportFile, setPassportFile]         = useState(null);
  const [filePreview,  setFilePreview]          = useState(null); // data-URL for image preview
  const fileInputRef                            = useRef(null);

  const [errors,  setErrors]  = useState({});
  const [phase,   setPhase]   = useState(PHASE.IDLE);
  const [result,  setResult]  = useState(null);   // { visaId, docId }
  const [uploadErr, setUploadErr] = useState('');

  const minDate = useMemo(() => new Date().toISOString().split('T')[0], []);

  // ── Validation ────────────────────────────────────────────────────────────
  const validate = () => {
    const next = {};
    if (!form.fullName.trim())    next.fullName    = 'الاسم الكامل مطلوب';
    if (!form.passport.trim())    next.passport    = 'رقم الجواز مطلوب';
    if (!form.nationality.trim()) next.nationality = 'الجنسية مطلوبة';
    if (!form.country)            next.country     = 'اختر دولة الوجهة';
    if (!form.date)               next.date        = 'اختر تاريخ السفر';

    // File is required
    if (!passportFile) {
      next.file = 'يرجى إرفاق صورة جواز السفر';
    } else {
      const ext = passportFile.name.split('.').pop().toLowerCase();
      if (!['jpg','jpeg','png','pdf'].includes(ext))
        next.file = 'نوع الملف غير مقبول — استخدم JPG أو PNG أو PDF';
      else if (passportFile.size > MAX_BYTES)
        next.file = 'حجم الملف تجاوز 5 ميغابايت';
    }

    setErrors(next);
    return Object.keys(next).length === 0;
  };

  // ── File picker handler ───────────────────────────────────────────────────
  const handleFileChange = (e) => {
    const file = e.target.files?.[0] ?? null;
    setPassportFile(file);
    setErrors((prev) => ({ ...prev, file: undefined }));
    setUploadErr('');

    if (!file) { setFilePreview(null); return; }

    // Show image thumbnail for JPG/PNG; show PDF icon otherwise
    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = (ev) => setFilePreview(ev.target.result);
      reader.readAsDataURL(file);
    } else {
      setFilePreview('pdf');
    }
  };

  // ── Two-phase submit ──────────────────────────────────────────────────────
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    // ── Phase 1: create the visa application (JSON) ──────────────────────
    setPhase(PHASE.SUBMITTING);
    let visa;
    try {
      visa = await createVisa(form);
    } catch (err) {
      setErrors({ submit: err.message || 'تعذر إنشاء الطلب' });
      setPhase(PHASE.IDLE);
      return;
    }

    // ── Phase 2: upload the passport scan (multipart) ────────────────────
    setPhase(PHASE.UPLOADING);
    let doc;
    try {
      doc = await uploadPassportDocument(passportFile, visa.id);
    } catch (err) {
      // Visa was created — don't re-submit. Just warn and let the user retry the upload.
      setUploadErr(
        `تم إنشاء الطلب (${visa.reference}) لكن فشل رفع الجواز: ${err.message}. ` +
        'يمكنك رفع الجواز لاحقاً من صفحة متابعة طلباتك.'
      );
      setResult({ visaId: visa.id, reference: visa.reference, docId: null });
      setPhase(PHASE.DONE);
      return;
    }

    setResult({ visaId: visa.id, reference: visa.reference, docId: doc.id });
    setPhase(PHASE.DONE);
  };

  // ── Derived state ─────────────────────────────────────────────────────────
  const isLoading = phase === PHASE.SUBMITTING || phase === PHASE.UPLOADING;

  // ── Success screen ────────────────────────────────────────────────────────
  if (phase === PHASE.DONE) {
    return (
      <div className="space-y-6 text-beige">
        <div>
          <p className="text-sm font-semibold text-brand">حجز الفيزا الإلكترونية</p>
          <h1 className="text-2xl font-display font-semibold text-beige">
            تم تقديم الطلب بنجاح ✅
          </h1>
        </div>

        <Card className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2">
            <div>
              <p className="text-xs text-beige/60 mb-1">رقم المرجع</p>
              <p className="text-lg font-bold text-brand">{result.reference}</p>
            </div>
            <div>
              <p className="text-xs text-beige/60 mb-1">معرّف الطلب</p>
              <p className="text-xs font-mono text-beige/70 break-all">{result.visaId}</p>
            </div>
            <div>
              <p className="text-xs text-beige/60 mb-1">جواز السفر</p>
              <p className={`text-sm font-semibold ${result.docId ? 'text-success' : 'text-warning'}`}>
                {result.docId ? '✅ تم الرفع' : '⚠️ لم يُرفع بعد'}
              </p>
            </div>
          </div>

          {uploadErr && (
            <div className="rounded-xl border border-warning/40 bg-warning/10 px-4 py-3 text-sm text-warning">
              {uploadErr}
            </div>
          )}

          <Button
            variant="ghost"
            onClick={() => {
              setPhase(PHASE.IDLE);
              setResult(null);
              setPassportFile(null);
              setFilePreview(null);
              setUploadErr('');
              setForm({ fullName:'', passport:'', nationality:'', country: countries[0], date:'', type:'tourist' });
            }}
          >
            تقديم طلب جديد
          </Button>
        </Card>
      </div>
    );
  }

  // ── Main form ─────────────────────────────────────────────────────────────
  return (
    <div className="space-y-6 text-beige">
      {/* Header */}
      <div className="flex items-start justify-between gap-3 flex-wrap">
        <div>
          <p className="text-sm font-semibold text-brand">حجز الفيزا الإلكترونية</p>
          <h1 className="text-2xl font-display font-semibold text-beige">نموذج تقديم سريع</h1>
          <p className="text-sm text-beige/70">أدخل البيانات بدقة لضمان سرعة المعالجة.</p>
        </div>

        {/* Phase progress badge */}
        {phase !== PHASE.IDLE && (
          <span className="badge bg-brand/20 text-brand border border-brand/40 animate-pulse text-xs px-3 py-1 rounded-full">
            {phase === PHASE.SUBMITTING ? '⏳ جارٍ إنشاء الطلب…' : '📤 جارٍ رفع الجواز…'}
          </span>
        )}
      </div>

      <Card className="space-y-4">
        <form onSubmit={handleSubmit} className="grid gap-4 md:grid-cols-2">

          {/* ── Text fields ─────────────────────────────────── */}
          <Input
            label="الاسم الكامل"
            value={form.fullName}
            onChange={(v) => setForm((p) => ({ ...p, fullName: v }))}
            error={errors.fullName}
            required
          />
          <Input
            label="رقم الجواز"
            value={form.passport}
            onChange={(v) => setForm((p) => ({ ...p, passport: v }))}
            error={errors.passport}
            required
          />
          <Input
            label="الجنسية"
            value={form.nationality}
            onChange={(v) => setForm((p) => ({ ...p, nationality: v }))}
            error={errors.nationality}
            required
          />

          {/* Country select */}
          <label className="flex flex-col gap-2 text-sm font-semibold text-beige/90">
            دولة الوجهة
            <select
              className="w-full rounded-xl border border-beige/30 bg-surface px-4 py-3 text-base font-normal text-beige outline-none ring-brand/30 transition focus:border-brand focus:ring-4"
              value={form.country}
              onChange={(e) => setForm((p) => ({ ...p, country: e.target.value }))}
            >
              {countries.map((c) => <option key={c}>{c}</option>)}
            </select>
            {errors.country && (
              <span className="text-sm font-medium text-danger">{errors.country}</span>
            )}
          </label>

          <Input
            label="تاريخ السفر"
            type="date"
            min={minDate}
            value={form.date}
            onChange={(v) => setForm((p) => ({ ...p, date: v }))}
            error={errors.date}
            required
          />

          {/* Visa type select */}
          <label className="flex flex-col gap-2 text-sm font-semibold text-beige/90">
            نوع الفيزا
            <select
              className="w-full rounded-xl border border-beige/30 bg-surface px-4 py-3 text-base font-normal text-beige outline-none ring-brand/30 transition focus:border-brand focus:ring-4"
              value={form.type}
              onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))}
            >
              {visaTypes.map((t) => (
                <option key={t.value} value={t.value}>{t.label}</option>
              ))}
            </select>
          </label>

          {/* ── Passport file upload ─────────────────────────── */}
          <div className="md:col-span-2">
            <p className="text-sm font-semibold text-beige/90 mb-2">
              صورة جواز السفر <span className="text-danger">*</span>
            </p>

            {/* Drop-zone / click area */}
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              className={[
                'w-full rounded-xl border-2 border-dashed px-4 py-6 text-center transition',
                errors.file
                  ? 'border-danger/60 bg-danger/10'
                  : passportFile
                    ? 'border-success/50 bg-success/10'
                    : 'border-beige/30 bg-surface hover:border-brand/60',
              ].join(' ')}
            >
              {/* Preview */}
              {filePreview && filePreview !== 'pdf' ? (
                <img
                  src={filePreview}
                  alt="معاينة الجواز"
                  className="mx-auto h-28 w-auto rounded-lg object-cover mb-3 ring-2 ring-brand/30"
                />
              ) : filePreview === 'pdf' ? (
                <div className="mx-auto mb-3 flex h-16 w-16 items-center justify-center rounded-xl bg-danger/20 text-3xl">
                  📄
                </div>
              ) : (
                <div className="mx-auto mb-3 flex h-16 w-16 items-center justify-center rounded-xl bg-beige/10 text-3xl">
                  🛂
                </div>
              )}

              <p className="text-sm font-semibold text-beige">
                {passportFile ? passportFile.name : 'انقر لاختيار صورة جواز السفر'}
              </p>
              <p className="mt-1 text-xs text-beige/50">
                الأنواع المقبولة: JPG · PNG · PDF — الحد الأقصى: 5 ميغابايت
              </p>
            </button>

            {/* Hidden real file input */}
            <input
              ref={fileInputRef}
              type="file"
              accept={ACCEPTED_TYPES}
              className="hidden"
              onChange={handleFileChange}
            />

            {errors.file && (
              <p className="mt-2 text-sm font-medium text-danger">{errors.file}</p>
            )}
          </div>

          {/* ── Submit error ─────────────────────────────────── */}
          {errors.submit && (
            <div className="md:col-span-2 rounded-xl border border-danger/40 bg-danger/15 px-4 py-3 text-sm font-semibold text-danger">
              {errors.submit}
            </div>
          )}

          {/* ── Footer ──────────────────────────────────────── */}
          <div className="md:col-span-2 flex items-center justify-between flex-wrap gap-3">
            <p className="text-sm text-beige/70">يتم مراجعة الطلب خلال 24–48 ساعة.</p>
            <Button type="submit" loading={isLoading} disabled={isLoading}>
              {phase === PHASE.UPLOADING ? 'جارٍ رفع الجواز…' : 'إرسال الطلب'}
            </Button>
          </div>

        </form>
      </Card>
    </div>
  );
}
