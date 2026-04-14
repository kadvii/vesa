const statusStyles = {
  Pending: { bg: 'bg-warning/20', text: 'text-warning', dot: 'bg-warning', label: 'قيد المراجعة' },
  Approved: { bg: 'bg-success/25', text: 'text-success', dot: 'bg-success', label: 'مقبول' },
  Rejected: { bg: 'bg-danger/20', text: 'text-danger', dot: 'bg-danger', label: 'مرفوض' },
};

const currency = new Intl.NumberFormat('ar-IQ', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
});

/**
 * @param {{ reservation: object, variant?: 'reservation' | 'visa' }}}
 */
export default function StatusCard({ reservation, variant = 'reservation' }) {
  const style = statusStyles[reservation.status] || statusStyles.Pending;
  const isVisa = variant === 'visa';
  const hasAmount = reservation.amount != null && reservation.amount !== '' && !Number.isNaN(Number(reservation.amount));

  return (
    <div className="glass relative rounded-2xl p-5 text-right text-beige">
      <div className="flex items-start justify-between gap-3">
        <div className="flex flex-col gap-1">
          <p className="text-xs font-semibold uppercase tracking-wide text-beige/70">
            {isVisa ? 'الوجهة' : 'المصرف'}
          </p>
          <h3 className="text-lg font-display font-semibold text-beige">{reservation.bank}</h3>
          <p className="text-sm text-beige/80">
            {isVisa ? 'تاريخ السفر المقترح' : 'موعد التسليم'} {reservation.date}
          </p>
        </div>
        <div className="text-right">
          {isVisa ? (
            <>
              <p className="text-xs font-semibold uppercase tracking-wide text-beige/70">نوع الفيزا</p>
              <p className="text-lg font-semibold text-beige">
                {reservation.visaTypeLabel || reservation.visaType || '—'}
              </p>
            </>
          ) : (
            <>
              <p className="text-xs font-semibold uppercase tracking-wide text-beige/70">المبلغ</p>
              <p className="text-xl font-semibold text-beige">
                {hasAmount ? currency.format(Number(reservation.amount)) : '—'}
              </p>
            </>
          )}
        </div>
      </div>

      <div className="mt-4 flex items-center justify-between">
        <div className={`badge ${style.bg} ${style.text}`}>
          <span className={`ml-2 inline-block h-2 w-2 rounded-full ${style.dot}`} />
          {style.label || reservation.status}
        </div>
        <div className="text-sm font-mono text-beige/70">الرمز: {reservation.reference}</div>
      </div>
    </div>
  );
}
