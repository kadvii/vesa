import { useState } from 'react';
import BookingBox from '../components/BookingBox';
import StatusCard from '../components/StatusCard';
import { useAuth } from '../context/AuthContext';
import { createReservation } from '../services/api';

export default function Booking() {
  const { user } = useAuth();
  const [loading, setLoading] = useState(false);
  const [serverError, setServerError] = useState('');
  const [lastReservation, setLastReservation] = useState(null);

  const handleSubmit = async (data) => {
    setLoading(true);
    setServerError('');
    try {
      const res = await createReservation({ ...data, userEmail: user?.email });
      setLastReservation(res);
    } catch (err) {
      setServerError(err.message || 'ØªØ¹Ø°Ø± Ø¥Ù†Ø´Ø§Ø¡ Ø§Ù„Ø­Ø¬Ø²');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="grid gap-6 lg:grid-cols-3 lg:items-start text-beige">
      <div className="lg:col-span-2">
        <BookingBox onSubmit={handleSubmit} loading={loading} serverError={serverError} />
      </div>

      <div className="rounded-2xl border border-beige/15 bg-surface/90 p-6 shadow-card">
        <div className="mb-4 flex items-center justify-between gap-3">
          <div>
            <p className="text-sm font-semibold text-brand">Ø£Ø­Ø¯Ø« Ø·Ù„Ø¨</p>
            <h3 className="text-xl font-display font-semibold text-beige">Ù…Ù„Ø®Øµ Ø§Ù„ØªØ£ÙƒÙŠØ¯</h3>
          </div>
          <span className="badge">ØªØ­Ø¯ÙŠØ« ÙÙˆØ±ÙŠ</span>
        </div>

        {lastReservation ? (
          <StatusCard reservation={lastReservation} />
        ) : (
          <div className="rounded-xl border border-dashed border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
            Ø¨Ø¹Ø¯ Ø¥Ø±Ø³Ø§Ù„ Ø£ÙˆÙ„ Ø­Ø¬Ø² Ø³ÙŠØ¸Ù‡Ø± Ø§Ù„Ù…Ù„Ø®Øµ Ù‡Ù†Ø§ Ù…Ø¹ Ø­Ø§Ù„Ø© Ø§Ù„Ø·Ù„Ø¨.
          </div>
        )}
      </div>
    </div>
  );
}
