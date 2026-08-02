<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

function cerrarSesion() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <nav
    data-testid="app-nav"
    class="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between"
  >
    <div class="font-semibold text-gray-800">MesaSitec</div>
    <div class="flex items-center gap-4 text-sm">
      <span data-testid="nav-usuario-nombre" class="text-gray-700">
        {{ auth.usuario?.nombre }}
      </span>
      <span
        data-testid="nav-usuario-rol"
        class="px-2 py-0.5 rounded bg-gray-100 text-gray-600 text-xs"
      >
        {{ auth.usuario?.rol }}
      </span>
      <button
        data-testid="btn-logout"
        class="text-red-600 hover:text-red-800"
        @click="cerrarSesion"
      >
        Cerrar sesión
      </button>
    </div>
  </nav>
</template>