import { useState } from 'react';
import Card from '../components/Card';
import Button from '../components/Button';
import ComingSoon from '../components/ComingSoon';

const initialOffers = [
  { id: 'off-1', title: 'Ø®ØµÙ… 15% Ù„ØªØ±ÙƒÙŠØ§', desc: 'ÙŠØ³ØªÙ…Ø± Ø­ØªÙ‰ Ù†Ù‡Ø§ÙŠØ© Ø§Ù„Ø´Ù‡Ø±' },
  { id: 'off-2', title: 'ÙÙŠØ²Ø§ Ø§Ù„Ø¥Ù…Ø§Ø±Ø§Øª 130$', desc: 'Ø¹Ø±Ø¶ Ù…Ø­Ø¯ÙˆØ¯ 7 Ø£ÙŠØ§Ù…' },
];

export default function Offers() {
  const [offers, setOffers] = useState(initialOffers);
  const [form, setForm] = useState({ title: '', desc: '' });

  const addOffer = (e) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    setOffers((prev) => [{ id: `off-${prev.length + 1}`, ...form }, ...prev]);
    setForm({ title: '', desc: '' });
  };

  const removeOffer = (id) => {
    setOffers((prev) => prev.filter((o) => o.id !== id));
  };

  return (
    <ComingSoon label="Ø§Ù„Ø¹Ø±ÙˆØ¶ ÙˆØ§Ù„Ø®ØµÙˆÙ…Ø§Øª">
    <div className="space-y-4 text-beige">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-semibold text-brand">Ø§Ù„Ø¹Ø±ÙˆØ¶</p>
          <h1 className="text-2xl font-display font-semibold text-beige">Ø¥Ø¯Ø§Ø±Ø© Ø§Ù„Ø®ØµÙˆÙ…Ø§Øª ÙˆØ§Ù„Ø¹Ø±ÙˆØ¶</h1>
        </div>
        <Button onClick={addOffer}>Ø­ÙØ¸</Button>
      </div>

      <Card className="space-y-3">
        <form className="grid gap-3 md:grid-cols-3" onSubmit={addOffer}>
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="Ø¹Ù†ÙˆØ§Ù† Ø§Ù„Ø¹Ø±Ø¶"
            value={form.title}
            onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
          />
          <input
            className="rounded-xl border border-beige/30 bg-surface px-3 py-2 text-sm text-beige outline-none focus:border-brand"
            placeholder="ÙˆØµÙ Ù…Ø®ØªØµØ±"
            value={form.desc}
            onChange={(e) => setForm((p) => ({ ...p, desc: e.target.value }))}
          />
          <Button type="submit">Ø¥Ø¶Ø§ÙØ© Ø¹Ø±Ø¶</Button>
        </form>
      </Card>

      <div className="grid gap-3 md:grid-cols-2">
        {offers.map((o) => (
          <Card key={o.id} className="space-y-2 bg-base/60">
            <div className="flex items-start justify-between gap-2">
              <div>
                <p className="text-lg font-semibold text-beige">{o.title}</p>
                <p className="text-sm text-beige/70">{o.desc}</p>
              </div>
              <Button variant="ghost" className="text-danger" onClick={() => removeOffer(o.id)}>
                Ø­Ø°Ù
              </Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
    </ComingSoon>
  );
}
