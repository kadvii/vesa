import Card from '../../components/Card';
import Button from '../../components/Button';

const countries = ['ØªØ±ÙƒÙŠØ§', 'Ø§Ù„Ø¥Ù…Ø§Ø±Ø§Øª', 'Ø§Ù„Ø£Ø±Ø¯Ù†', 'Ù‚Ø·Ø±', 'Ø§Ù„Ø³Ø¹ÙˆØ¯ÙŠØ©'];

const bookings = [
  { id: 'V-401', user: 'Ø£Ø­Ù…Ø¯ Ø´Ø§ÙƒØ±', country: 'ØªØ±ÙƒÙŠØ§', date: '2026-04-09', status: 'Ù‚ÙŠØ¯ Ø§Ù„Ù…Ø±Ø§Ø¬Ø¹Ø©' },
  { id: 'V-402', user: 'Ù„ÙŠØ« Ø¨Ø§Ø³Ù…', country: 'Ø§Ù„Ø£Ø±Ø¯Ù†', date: '2026-04-12', status: 'Ù…Ù‚Ø¨ÙˆÙ„' },
];

export default function AgentDashboard() {
  return (
    <div className="space-y-4 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">Ù„ÙˆØ­Ø© Ø§Ù„ÙˆÙƒÙŠÙ„</p>
          <h1 className="text-2xl font-display font-semibold text-beige">Ø¥Ø¯Ø§Ø±Ø© Ø§Ù„Ø®Ø¯Ù…Ø§Øª ÙˆØ§Ù„Ø£Ø³Ø¹Ø§Ø±</h1>
          <p className="text-sm text-beige/70">Ø­Ø¯Ù‘Ø« Ø§Ù„Ø£Ø³Ø¹Ø§Ø±ØŒ Ø£Ø¶Ù Ø¹Ø±ÙˆØ¶Ø§Ù‹ØŒ ÙˆØ§Ø·Ù„Ø¹ Ø¹Ù„Ù‰ Ø­Ø¬ÙˆØ²Ø§Øª Ø§Ù„Ø¹Ù…Ù„Ø§Ø¡.</p>
        </div>
        <Button>Ø¥Ø¶Ø§ÙØ© Ø¹Ø±Ø¶ Ø¬Ø¯ÙŠØ¯</Button>
      </div>

      <div className="grid gap-3 md:grid-cols-3">
        <Card className="bg-base/60">
          <p className="text-sm text-beige/70">Ù…ØªÙˆØ³Ø· Ø³Ø¹Ø± Ø§Ù„ÙÙŠØ²Ø§</p>
          <p className="text-3xl font-bold text-beige">$120</p>
        </Card>
        <Card className="bg-base/60">
          <p className="text-sm text-beige/70">Ø·Ù„Ø¨Ø§Øª Ù‡Ø°Ø§ Ø§Ù„Ø£Ø³Ø¨ÙˆØ¹</p>
          <p className="text-3xl font-bold text-beige">26</p>
        </Card>
        <Card className="bg-base/60">
          <p className="text-sm text-beige/70">Ø¯ÙˆÙ„ Ù…ÙØ¹Ù„Ø©</p>
          <p className="text-3xl font-bold text-beige">12</p>
        </Card>
      </div>

      <Card className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-beige">Ø£Ø³Ø¹Ø§Ø± Ø§Ù„ÙÙŠØ²Ø§ Ø­Ø³Ø¨ Ø§Ù„Ø¨Ù„Ø¯</h3>
          <Button variant="ghost">ØªØ­Ø¯ÙŠØ« Ø§Ù„Ø£Ø³Ø¹Ø§Ø±</Button>
        </div>
        <div className="grid gap-2 md:grid-cols-2">
          {countries.map((c) => (
            <div key={c} className="flex items-center justify-between rounded-xl border border-beige/15 bg-base/60 px-3 py-2">
              <span>{c}</span>
              <input
                defaultValue="120"
                className="w-20 rounded-lg border border-beige/25 bg-surface px-2 py-1 text-center text-sm text-beige outline-none"
              />
            </div>
          ))}
        </div>
      </Card>

      <Card className="space-y-3">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-beige">Ø­Ø¬ÙˆØ²Ø§Øª Ø§Ù„Ø¹Ù…Ù„Ø§Ø¡</h3>
          <Button variant="ghost">ØªØ­Ø¯ÙŠØ«</Button>
        </div>
        <div className="space-y-2">
          {bookings.map((b) => (
            <div key={b.id} className="grid grid-cols-4 items-center rounded-xl border border-beige/15 bg-base/60 px-3 py-2 text-sm">
              <span>{b.id}</span>
              <span>{b.user}</span>
              <span>{b.country}</span>
              <span className="text-brand">{b.status}</span>
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
}
