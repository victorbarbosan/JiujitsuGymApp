import { defineConfig } from 'vite';
import path from 'path';

export default defineConfig({

    build: {
        outDir: '../wwwroot/dist',
        emptyOutDir: true,

        manifest: true,

        rollupOptions: {
            input: './src/main.ts',
            output: {
                entryFileNames: 'js/app.js',
                chunkFileNames: 'js/[name].js',
                assetFileNames: 'assets/[name].[ext]'
            }
        }
    },
    server: {
        cors: true,
        strictPort: true,
        port: 5173,
        hmr: {
            protocol: 'ws'
        }
    }
});