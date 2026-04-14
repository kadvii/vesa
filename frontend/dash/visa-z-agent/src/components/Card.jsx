export default function Card({ children, className = '' }) {
  return (
    <div
      className={`rounded-2xl border border-beige/15 bg-surface/90 p-6 text-beige shadow-card backdrop-blur-sm ${className}`}
    >
      {children}
    </div>
  );
}
