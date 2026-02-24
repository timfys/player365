import { resolve } from 'node:path';

export default {
  root: '.',
  server: { port: 5173, strictPort: true },
  build: {
    outDir: resolve('..', 'wwwroot', 'assets', 'js'),
    emptyOutDir: false,
    sourcemap: true,
    rollupOptions: {
      input: {
        main: resolve('src', 'main.js')
      },
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: 'chunks/[name].js',
        assetFileNames: 'assets/[name][extname]'
      }
    }
  }
};