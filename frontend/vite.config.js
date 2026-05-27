import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  root: process.cwd(),
  build: {
    rollupOptions: {
      input: path.resolve(__dirname, 'index.html')
    }
  },
  server: {
    host: '0.0.0.0',
    port: 4173,
        allowedHosts: [
      'localhost',
      'frontend',           
      'todoapp-frontend',   
      '.localhost'          
    ],
     proxy: {
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, '/api')
      }
    }
  }
})
