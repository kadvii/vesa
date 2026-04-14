/**
 * ComingSoon — renders children behind a frosted-glass "قريباً" overlay.
 *
 * Usage:
 *   <ComingSoon label="إدارة الأسعار">
 *     <Prices />
 *   </ComingSoon>
 *
 * When `disabled` is true the overlay is removed (for when the backend is ready).
 */
export default function ComingSoon({ children, label = 'هذه الصفحة', disabled = false }) {
  return (
    <div style={{ position: 'relative' }}>
      {children}

      {!disabled && (
        <div
          style={{
            position: 'absolute',
            inset: 0,
            zIndex: 50,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '16px',
            backdropFilter: 'blur(6px)',
            WebkitBackdropFilter: 'blur(6px)',
            backgroundColor: 'rgba(10, 14, 26, 0.72)',
            borderRadius: '16px',
          }}
        >
          {/* Pulsing badge */}
          <div
            style={{
              width: '64px',
              height: '64px',
              borderRadius: '50%',
              background: 'linear-gradient(135deg, #d4af37 0%, #f0d060 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: '28px',
              animation: 'cs-pulse 2s infinite',
              boxShadow: '0 0 0 0 rgba(212,175,55,0.5)',
            }}
          >
            🕐
          </div>

          <div style={{ textAlign: 'center', direction: 'rtl' }}>
            <h2
              style={{
                fontSize: '32px',
                fontWeight: '800',
                color: '#d4af37',
                margin: 0,
                letterSpacing: '1px',
                textShadow: '0 2px 12px rgba(212,175,55,0.4)',
              }}
            >
              قريباً
            </h2>
            <p style={{ color: '#c8b89a', fontSize: '14px', marginTop: '6px' }}>
              {label} — جارٍ التطوير، تابعنا قريباً!
            </p>
          </div>

          <style>{`
            @keyframes cs-pulse {
              0%   { box-shadow: 0 0 0 0   rgba(212,175,55,0.6); }
              70%  { box-shadow: 0 0 0 20px rgba(212,175,55,0);   }
              100% { box-shadow: 0 0 0 0   rgba(212,175,55,0);    }
            }
          `}</style>
        </div>
      )}
    </div>
  );
}
