const variants = {
  primary:
    'bg-brand text-beige hover:bg-brand-dark focus-visible:outline-brand-dark shadow-md shadow-black/30',
  ghost:
    'bg-transparent text-beige border border-beige/40 hover:border-beige hover:text-white hover:bg-surface/60',
  subtle:
    'bg-surface text-beige hover:bg-surface/80 border border-beige/20',
};

export default function Button({
  children,
  variant = 'primary',
  loading = false,
  disabled = false,
  className = '',
  ...props
}) {
  const cx = (...classes) => classes.filter(Boolean).join(' ');

  return (
    <button
      className={cx(
        'inline-flex items-center justify-center gap-2 rounded-xl px-4 py-2.5 text-sm font-semibold transition-all duration-150 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 disabled:cursor-not-allowed disabled:opacity-60',
        variants[variant],
        className,
      )}
      disabled={disabled || loading}
      {...props}
    >
      {loading && (
        <span className="h-4 w-4 animate-spin rounded-full border-2 border-white/60 border-t-white" />
      )}
      {children}
    </button>
  );
}
