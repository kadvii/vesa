const banks = [
  { code: 'CBI', name: 'Ø§Ù„Ø¨Ù†Ùƒ Ø§Ù„Ù…Ø±ÙƒØ²ÙŠ Ø§Ù„Ø¹Ø±Ø§Ù‚ÙŠ', url: 'https://cbi.iq', city: 'Ø¨ØºØ¯Ø§Ø¯', type: 'Ø­ÙƒÙˆÙ…ÙŠ' },
  { code: 'RAF', name: 'Ù…ØµØ±Ù Ø§Ù„Ø±Ø§ÙØ¯ÙŠÙ†', url: 'http://rafidain-bank.gov.iq', city: 'Ø¨ØºØ¯Ø§Ø¯', type: 'Ø­ÙƒÙˆÙ…ÙŠ' },
  { code: 'RSH', name: 'Ù…ØµØ±Ù Ø§Ù„Ø±Ø´ÙŠØ¯', url: 'https://rashidbank.gov.iq', city: 'Ø¨ØºØ¯Ø§Ø¯', type: 'Ø­ÙƒÙˆÙ…ÙŠ' },
  { code: 'TBI', name: 'Ø§Ù„Ù…ØµØ±Ù Ø§Ù„Ø¹Ø±Ø§Ù‚ÙŠ Ù„Ù„ØªØ¬Ø§Ø±Ø©', url: 'https://tbi.com.iq', city: 'Ø¨ØºØ¯Ø§Ø¯', type: 'ØªØ¬Ø§Ø±ÙŠ' },
  { code: 'BOB', name: 'Ù…ØµØ±Ù Ø¨ØºØ¯Ø§Ø¯', url: 'https://bankofbaghdad.com', city: 'Ø¨ØºØ¯Ø§Ø¯', type: 'ØªØ¬Ø§Ø±ÙŠ' },
  { code: 'KIB', name: 'Ù…ØµØ±Ù ÙƒØ±Ø¯Ø³ØªØ§Ù† Ø§Ù„Ø¯ÙˆÙ„ÙŠ', url: 'https://www.kib.iq', city: 'Ø£Ø±Ø¨ÙŠÙ„', type: 'Ø¥Ù‚Ù„ÙŠÙ…ÙŠ' },
  { code: 'CIH', name: 'Ù…ØµØ±Ù Ø¬ÙŠÙ‡Ø§Ù†', url: 'https://cihanbank.com', city: 'Ø£Ø±Ø¨ÙŠÙ„', type: 'Ø¥Ù‚Ù„ÙŠÙ…ÙŠ' },
];

export default function Banks() {
  return (
    <section className="space-y-4">
      <div className="flex flex-col gap-2">
        <p className="text-sm font-semibold text-brand">Ø§Ù„Ø¨Ù†ÙˆÙƒ Ø§Ù„Ù…Ø¹ØªÙ…Ø¯Ø©</p>
        <h1 className="text-2xl font-display font-semibold text-slate-900">Ù‚Ø§Ø¦Ù…Ø© Ø§Ù„Ù…ØµØ§Ø±Ù ÙˆØ±ÙˆØ§Ø¨Ø·Ù‡Ø§</h1>
        <p className="text-sm text-slate-600">
          Ø§Ù„Ù‚Ø§Ø¦Ù…Ø© Ù…Ø­Ø¯Ø«Ø© Ù„Ø£ØºØ±Ø§Ø¶ Ø§Ù„Ø¹Ø±Ø¶ Ø§Ù„ØªØ¬Ø±ÙŠØ¨ÙŠ. ÙŠÙ…ÙƒÙ†Ùƒ Ø²ÙŠØ§Ø±Ø© Ø§Ù„Ù…ÙˆÙ‚Ø¹ Ø§Ù„Ø±Ø³Ù…ÙŠ Ù„ÙƒÙ„ Ù…ØµØ±Ù Ù„Ù„ØªØ­Ù‚Ù‚ Ù…Ù† Ù…ØªØ·Ù„Ø¨Ø§Øª Ø§Ù„Ø­Ø¬Ø².
        </p>
      </div>

      <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3 text-beige">
        {banks.map((bank) => (
          <a
            key={bank.code}
            href={bank.url}
            target="_blank"
            rel="noreferrer noopener"
            className="group rounded-2xl border border-beige/15 bg-surface/90 p-4 shadow-card transition hover:-translate-y-0.5 hover:border-brand"
          >
            <div className="flex items-start justify-between gap-2">
              <div>
                <p className="text-xs font-semibold text-beige/70">{bank.code}</p>
                <h3 className="text-lg font-semibold text-beige">{bank.name}</h3>
                <p className="text-xs text-beige/60">
                  {bank.city} Â· {bank.type}
                </p>
              </div>
              <span className="rounded-full bg-base/60 px-3 py-1 text-xs font-semibold text-beige group-hover:bg-brand/20 group-hover:text-brand">
                Ø²ÙŠØ§Ø±Ø© Ø§Ù„Ù…ÙˆÙ‚Ø¹
              </span>
            </div>
          </a>
        ))}
      </div>
    </section>
  );
}
