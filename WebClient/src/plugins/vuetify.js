/**
 * plugins/vuetify.js
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Styles
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'
import { de} from 'vuetify/locale'
import { md3, md2 } from 'vuetify/blueprints'

// Composables
import { createVuetify } from 'vuetify'

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  blueprint: md3,
  theme: {
    defaultTheme: 'light',
  },
  locale: {
    locale: 'de',
    fallback: 'en',
    messages:{
      de
    }
  }
})
