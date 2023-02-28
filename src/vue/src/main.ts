import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import './style.scss'
import App from './App.vue'
import OilField from './views/OilField.vue'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

createApp(App)
  .use(createPinia().use(piniaPluginPersistedstate))
  .use(createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/', redirect: '/oil-field' },
      { path: '/oil-field', component: OilField }
    ],
  }))
  .mount('#app')
