import type { Config } from 'tailwindcss'

export default {
  darkMode: 'class',
  content: [
    './index.html',
    './src/**/*.{vue,ts,js}'
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          yellow: '#FACC15',
          'yellow-hover': '#EAB308',
          'yellow-muted': 'rgba(250, 204, 21, 0.12)'
        }
      },
      fontFamily: {
        ui: ['Plus Jakarta Sans', 'system-ui', '-apple-system', 'sans-serif']
      }
    }
  },
  plugins: []
} satisfies Config
