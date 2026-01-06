import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueDevTools from 'vite-plugin-vue-devtools';
import tailwindcss from '@tailwindcss/vite';
import { VitePWA } from 'vite-plugin-pwa';

const pwaOptions = {
    base: '/',
    registerType: 'prompt',
    manifest: {
        name: 'Afra-App',
        short_name: 'Afra-App',
        theme_color: '#005899',
        lang: 'de',
        icons: [
            {
                src: 'vdaa/appicon.svg',
                type: 'image/svg+xml',
                sizes: '512x512',
                purpose: ['any', 'maskable'],
            },
        ],
        screenshots: [
            {
                src: 'screenshots/screenshot_wide.png',
                type: 'image/png',
                form_factor: 'wide',
            },
            {
                src: 'screenshots/screenshot_narrow.png',
                type: 'image/png',
            },
        ],
    },
    workbox: {
        navigateFallbackDenylist: [/^\/api/],
    },
};

// https://vite.dev/config/
export default defineConfig({
    base: 'https://afra.altafraner.de/',
    plugins: [vue(), vueDevTools(), tailwindcss(), VitePWA(pwaOptions)],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    server: {
        proxy: {
            '/api': {
                target: 'http://127.0.0.1:5043',
                changeOrigin: true,
                ws: true,
            },
        },
    },
});
