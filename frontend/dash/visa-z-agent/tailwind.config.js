/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx,ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        display: ['"Cairo"', 'system-ui', 'sans-serif'],
        body: ['"Cairo"', 'system-ui', 'sans-serif'],
      },
      colors: {
        brand: {
          DEFAULT: '#556e53', // Sage Green
          dark: '#445843',
        },
        surface: '#29435c', // Medium Navy
        base: '#152a38', // Dark Navy
        beige: '#d1d4c9', // Light Beige
        success: '#86efac',
        warning: '#facc15',
        danger: '#f87171',
        slateglass: 'rgba(21, 42, 56, 0.75)',
      },
      boxShadow: {
        card: '0 18px 60px -28px rgba(0,0,0,0.45)',
      },
    },
  },
  plugins: [],
};
