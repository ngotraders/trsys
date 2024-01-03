import react from "@vitejs/plugin-react";
import basicSsl from '@vitejs/plugin-basic-ssl';
import { defineConfig } from "vite";

export default defineConfig({
  server: {
    proxy: {
      '/api': {
        secure: false,
        target: 'https://localhost:8443',
      },
      '/auth': {
        secure: false,
        target: 'https://localhost:8443',
        rewrite: (path) => path.replace(/^\/auth/, ''),
      }
    }
  },
  plugins: [react(), basicSsl()],
});
