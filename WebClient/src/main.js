import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import PrimeVue from 'primevue/config';
import Aura from '@primevue/themes/aura';
import locale from 'primelocale/de.json'
import {definePreset} from "@primevue/themes";

const AfraAppPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: "#f2f7fb",
      100: "#c2dbed",
      200: "#91bedf",
      300: "#61a1d1",
      400: "#3085c2",
      500: "#0068b4",
      600: "#005899",
      700: "#00497e",
      800: "#003963",
      900: "#002a48",
      950: "#001a2d"
    },
  }
})

const app = createApp(App)

app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: AfraAppPreset,
    options: {
      darkModeSelector: 'none',
      cssLayer: true
    },
    locale
  }
})
app.mount('#app')
