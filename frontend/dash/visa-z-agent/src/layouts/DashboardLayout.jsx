import { useEffect, useState } from 'react';
import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar';
import Sidebar from '../components/Sidebar';

export default function DashboardLayout({ menuItems = [] }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    document.documentElement.dir = 'rtl';
    document.documentElement.lang = 'ar';
  }, []);

  return (
    <div className="min-h-screen bg-base text-beige">
      <div className="mx-auto flex max-w-7xl flex-col lg:flex-row">
        {/* Mobile overlay */}
        {sidebarOpen && (
          <div
            className="fixed inset-0 z-30 bg-black/40 backdrop-blur-sm lg:hidden"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        <div
          className={`fixed inset-y-0 right-0 z-40 w-72 transform transition-transform duration-200 lg:relative lg:translate-x-0 ${
            sidebarOpen ? 'translate-x-0' : 'translate-x-full lg:translate-x-0'
          }`}
        >
          <Sidebar onClose={() => setSidebarOpen(false)} menuItems={menuItems} />
        </div>

        <div className="flex min-h-screen flex-1 flex-col">
          <Navbar onOpenSidebar={() => setSidebarOpen(true)} />
          <main className="flex-1 px-4 pb-10 pt-6 lg:px-8">
            <div className="mx-auto max-w-6xl">
              <div className="mb-4 rounded-xl border border-beige/20 bg-base/60 p-3 text-sm">
                وضع تصحيح: إذا رأيت هذا النص فالمسارات تعمل و Outlet يعرض المحتوى.
              </div>
              <Outlet />
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
