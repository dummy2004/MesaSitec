import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/solicitudes' },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { publica: true },
    },
    {
      path: '/solicitudes',
      name: 'solicitudes-listado',
      component: () => import('@/views/SolicitudesListView.vue'),
    },
    {
      path: '/solicitudes/nueva',
      name: 'solicitudes-nueva',
      component: () => import('@/views/SolicitudFormView.vue'),
    },
    {
      path: '/solicitudes/:id',
      name: 'solicitudes-detalle',
      component: () => import('@/views/SolicitudDetalleView.vue'),
    },
    {
      path: '/solicitudes/:id/editar',
      name: 'solicitudes-editar',
      component: () => import('@/views/SolicitudFormView.vue'),
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (!to.meta.publica && !auth.estaAutenticado) {
    return { name: 'login' }
  }
  if (to.name === 'login' && auth.estaAutenticado) {
    return { name: 'solicitudes-listado' }
  }
})

export default router