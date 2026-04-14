import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const baseLink =
  'group flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-semibold transition-all duration-150';

const linkClasses = ({ isActive }) =>
  [
    baseLink,
    isActive
      ? 'bg-brand/20 text-beige shadow-inner shadow-black/20'
      : 'text-beige/80 hover:text-beige hover:bg-surface/60',
  ].join(' ');

export default function Sidebar({ onClose, menuItems }) {
  const { logout } = useAuth();
  const navigate = useNavigate();
  const items = menuItems && menuItems.length ? menuItems : [];

  const handleLogout = () => {
    logout();
    navigate('/login');
    onClose?.();
  };

  return (
    <aside className="relative flex h-full flex-col gap-3 border-l border-beige/15 bg-surface/95 p-4 backdrop-blur-lg">
      <div className="flex items-center justify-between gap-2 px-1">
        <div className="flex items-center gap-2">
          <div className="flex h-10 w-10 items-center justify-center rounded-2xl bg-brand/20 text-xl font-bold text-brand shadow-inner">
            Z
          </div>
          <div className="leading-tight">
            <p className="text-xs font-semibold text-beige/70">منصة الحكومة</p>
            <p className="text-sm font-bold text-beige">Visa Z</p>
          </div>
        </div>
      </div>

      <nav className="mt-2 flex flex-col gap-1">
        {items.map(({ key, label, to, icon: Icon }) => {
          const IconRender = Icon || HomeIcon;
          return (
            <NavLink key={key} to={to} className={linkClasses} onClick={onClose}>
              <IconRender className="h-5 w-5 text-beige/60 group-hover:text-brand" />
              <span>{label}</span>
            </NavLink>
          );
        })}
      </nav>

      <div className="mt-auto space-y-2 border-t border-beige/15 pt-4">
        <button
          type="button"
          onClick={handleLogout}
          className={`${baseLink} text-danger hover:bg-danger/20 hover:text-danger`}
        >
          <LogoutIcon className="h-5 w-5" />
          <span>تسجيل الخروج</span>
        </button>
        <div className="rounded-xl border border-beige/15 bg-base/60 p-3 text-xs text-beige/80">
          <p className="font-semibold text-beige">تلميح سريع</p>
          <p>تابع حالة الفيزا والدفع من لوحة التحكم مباشرة.</p>
        </div>
      </div>
    </aside>
  );
}

function HomeIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" {...props}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-4.5v-5.5h-5V21H5a1 1 0 0 1-1-1z" />
    </svg>
  );
}
function PassportIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" {...props}>
      <rect x="5" y="3" width="14" height="18" rx="2" />
      <circle cx="12" cy="9" r="3" />
      <path strokeLinecap="round" d="M7 16h10M9.5 12.5h5" />
    </svg>
  );
}
function ListIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" {...props}>
      <path strokeLinecap="round" d="M8 7h12M8 12h12M8 17h12" />
      <circle cx="4" cy="7" r="1.2" />
      <circle cx="4" cy="12" r="1.2" />
      <circle cx="4" cy="17" r="1.2" />
    </svg>
  );
}
function DollarIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" {...props}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v18m-3.5-4.5c0 1.38 1.57 2.5 3.5 2.5s3.5-1.12 3.5-2.5S14.93 14 12 14s-3.5-1.12-3.5-2.5S9.07 9 12 9s3.5-1.12 3.5-2.5S14.43 4 12 4s-3.5 1.12-3.5 2.5" />
    </svg>
  );
}
function BankIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" {...props}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M4 9h16M5 9V7l7-4 7 4v2" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M5 9v9h14V9M3 18h18" />
    </svg>
  );
}
function UserIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" {...props}>
      <circle cx="12" cy="8" r="3.2" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M5 20c.7-3.1 3.4-5 7-5s6.3 1.9 7 5" />
    </svg>
  );
}
function LogoutIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" {...props}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M15.5 6.5 21 12l-5.5 5.5" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M21 12H9.5" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M9.5 4h-4A1.5 1.5 0 0 0 4 5.5v13A1.5 1.5 0 0 0 5.5 20h4" />
    </svg>
  );
}
