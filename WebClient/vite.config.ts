import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueDevTools from 'vite-plugin-vue-devtools';
import tailwindcss from '@tailwindcss/vite';
import { VitePWA, VitePWAOptions } from 'vite-plugin-pwa';
import ui, { NuxtUIOptions } from '@nuxt/ui/vite';

const nuxtUiOptions: NuxtUIOptions = {
    ui: {
        colors: {
            primary: 'blue',
            secondary: 'neutral',
            neutral: 'neutral',
        },
    },
};

const pwaOptions: Partial<VitePWAOptions> = {
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
                sizes: '2160x1620',
            },
            {
                src: 'screenshots/screenshot_narrow.png',
                type: 'image/png',
                form_factor: 'narrow',
                sizes: '1242x2688',
            },
        ],
    },
    workbox: {
        navigateFallbackDenylist: [/^\/api/],
    },
};

// https://vite.dev/config/
export default defineConfig({
    base: '/',
    plugins: [vue(), vueDevTools(), tailwindcss(), ui(nuxtUiOptions), VitePWA(pwaOptions)],
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
