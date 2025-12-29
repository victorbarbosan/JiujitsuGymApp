import { defineConfig } from 'vite';

export default defineConfig({
    build: {
        outDir: '../wwwroot/js',
        emptyOutDir: true,
        rollupOptions: {
            input: './src/main.ts',
            output: {
                entryFileNames: 'app.js',
                format: 'es'
            }
        }
    }
});
