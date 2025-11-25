import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import PrimeVue from 'primevue/config';
import ToastService from 'primevue/toastservice';
import Aura from '@primevue/themes/aura';
import { de as locale } from 'primelocale/de.json';
import { definePreset } from '@primevue/themes';
import { createPinia } from 'pinia';
import Tooltip from 'primevue/tooltip';
import ConfirmationService from 'primevue/confirmationservice';
import DialogService from 'primevue/dialogservice';

const AfraAppPreset = definePreset(Aura, {
    components: {
        breadcrumb: {
            colorScheme: {
                dark: {
                    root: {
                        background: 'transparent',
                    },
                },
            },
        },
        button: {
            colorScheme: {
                dark: {
                    text: {
                        primary: {
                            hoverBackground:
                                'color-mix(in srgb, {primary.700}, transparent 80%)',
                            color: '{primary.200}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        danger: {
                            hoverBackground: 'color-mix(in srgb, {red.700}, transparent 80%)',
                            color: '{red.200}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        warn: {
                            hoverBackground:
                                'color-mix(in srgb, {orange.700}, transparent 80%)',
                            color: '{orange.200}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        success: {
                            hoverBackground: 'color-mix(in srgb, {green.700}, transparent 80%)',
                            color: '{green.300}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        info: {
                            hoverBackground: 'color-mix(in srgb, {blue.700}, transparent 80%)',
                            color: '{blue.200}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                    },
                    root: {
                        primary: {
                            hoverBackground: '{blue.700}',
                            activeBackground: '{blue.600}',
                            background: '{blue.800}',
                            color: '{surface.300}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        danger: {
                            hoverBackground: '{red.800}',
                            activeBackground: '{red.700}',
                            background: '{red.900}',
                            color: '{surface.300}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        warn: {
                            hoverBackground: '{orange.700}',
                            activeBackground: '{orange.600}',
                            background: '{orange.800}',
                            color: '{surface.300}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        success: {
                            hoverBackground: '{green.700}',
                            activeBackground: '{green.600}',
                            background: '{green.800}',
                            color: '{surface.300}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                        info: {
                            hoverBackground: '{blue.700}',
                            activeBackground: '{blue.600}',
                            background: '{blue.800}',
                            color: '{surface.300}',
                            hoverColor: '{surface.200}',
                            activeColor: '{surface.100}',
                        },
                    },
                },
                light: {
                    text: {
                        success: {
                            hoverBackground: '{green.50}',
                            activeBackground: '{green.100}',
                            color: '{green.700}',
                        },
                    },
                },
            },
        },
        message: {
            colorScheme: {
                dark: {
                    info: {
                        color: '{blue.200}',
                        simple: {
                            color: '{blue.200}',
                        },
                    },
                },
            },
        },
    },
    semantic: {
        primary: {
            50: '#f2f7fb',
            100: '#c2dbed',
            200: '#91bedf',
            300: '#61a1d1',
            400: '#3085c2',
            500: '#0068b4',
            600: '#005899',
            700: '#00497e',
            800: '#003963',
            900: '#002a48',
            950: '#001a2d',
        },
        colorScheme: {
            light: {
                primary: {
                    color: '{blue.600}',
                    inverseColor: '#ffffff',
                    hoverColor: '{blue.700}',
                    activeColor: '{blue.800}',
                },
            },
            dark: {
                primary: {
                    color: '{blue.400}',
                    contrastColor: '{surface.200}',
                    inverseColor: '{surface.950}',
                    hoverColor: '{sky.300}',
                    activeColor: '{sky.200}',
                },
            },
        },
    },
});

const pinia = createPinia();
const app = createApp(App);

app.use(pinia);
app.use(router);
app.use(PrimeVue, {
    theme: {
        preset: AfraAppPreset,
        options: {
            darkModeSelector: 'system',
            cssLayer: {
                name: 'primevue',
                order: 'theme, base, primevue',
            },
        },
    },
    locale,
});
app.use(ToastService);
app.use(ConfirmationService);
app.use(DialogService);

app.directive('tooltip', Tooltip);

app.mount('#app');
