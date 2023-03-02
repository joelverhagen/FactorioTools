import { createApp, nextTick } from 'vue'
import { createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import './style.scss'
import App from './App.vue'
import OilField from './views/OilField.vue'
import NotFound from './views/NotFound.vue'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import { initializeOilFieldStore } from './stores/OilFieldStore'

const pinia = createPinia()
  .use(piniaPluginPersistedstate);

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: __BASE_PATH__,
      redirect: `${__BASE_PATH__}oil-field`
    },
    {
      path: `${__BASE_PATH__}oil-field`,
      component: OilField,
      meta: { title: 'Oil field generator' },
      beforeEnter(to) {
        initializeOilFieldStore(to.query)
      }
    },
    {
      path: `${__BASE_PATH__}:catchAll(.*)`,
      component: NotFound
    }
  ],
});

router
  .afterEach((to) => {
    nextTick(() => {
      document.title = to.meta.title ? `Factorio Tools - ${to.meta.title}` : 'Factorio Tools';
    });
  })

createApp(App)
  .use(pinia)
  .use(router)
  .mount('#app')
