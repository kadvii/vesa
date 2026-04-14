import Card from '../components/Card';
import { useAuth } from '../context/AuthContext';
import Button from '../components/Button';

export default function Profile() {
  const { user, logout } = useAuth();

  return (
    <div className="space-y-4 text-beige">
      <div>
        <p className="text-sm font-semibold text-brand">الملف الشخصي</p>
        <h1 className="text-2xl font-display font-semibold text-beige">بيانات الحساب</h1>
      </div>

      <Card className="space-y-3">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-brand text-base font-bold text-base shadow-inner">
            {user?.name?.slice(0, 1)?.toUpperCase() || '?'}
          </div>
          <div>
            <p className="text-lg font-semibold text-beige">{user?.name}</p>
            <p className="text-sm text-beige/70">{user?.email}</p>
          </div>
        </div>
        <div className="grid gap-2 text-sm text-beige/80">
          <p>نوع الحساب: مستخدم تجريبي</p>
          <p>اللغة: العربية</p>
        </div>
        <Button variant="ghost" onClick={logout} className="w-fit">
          تسجيل الخروج
        </Button>
      </Card>
    </div>
  );
}
