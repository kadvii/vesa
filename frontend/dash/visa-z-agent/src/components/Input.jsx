export default function Input({
  label,
  type = 'text',
  name,
  value,
  onChange,
  placeholder,
  error,
  required = false,
  ...props
}) {
  return (
    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 text-right">
      {label && <span className="text-beige">{label}</span>}
      <input
        className="w-full rounded-xl border border-beige/30 bg-surface px-4 py-3 text-base font-normal text-beige text-right outline-none ring-brand/30 transition focus:border-brand focus:ring-4 placeholder:text-beige/60"
        type={type}
        name={name}
        value={value}
        placeholder={placeholder}
        required={required}
        onChange={(e) => onChange(e.target.value)}
        {...props}
      />
      {error && <span className="text-sm font-medium text-danger">{error}</span>}
    </label>
  );
}
