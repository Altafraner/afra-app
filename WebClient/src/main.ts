import { createApp } from 'vue';
import App from './App.vue';
import router from './router/index';
import ui from '@nuxt/ui/vue-plugin';
import { createPinia } from 'pinia';
import lucideIcons from '@iconify-json/lucide/icons.json';
import { addCollection } from '@iconify/vue';

addCollection(lucideIcons);

const pinia = createPinia();
const app = createApp(App);

app.use(pinia);
app.use(router);
app.use(ui);

app.mount('#app');
