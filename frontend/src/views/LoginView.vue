<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const email = ref('')
const password = ref('')
const error = ref('')
const cargando = ref(false)

const auth = useAuthStore()
const router = useRouter()

async function enviar() {
  error.value = ''
  cargando.value = true
  try {
    await auth.login(email.value, password.value)
    router.push('/solicitudes')
  } catch (e: any) {
    error.value = e.response?.data?.detail ?? 'No se pudo iniciar sesión.'
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="max-w-sm mx-auto mt-20 bg-white p-8 rounded-lg shadow-sm border border-gray-200">
    <h1 class="text-xl font-semibold mb-6 text-gray-800">Iniciar sesión</h1>

    <form @submit.prevent="enviar" class="space-y-4">
      <div>
        <label class="block text-sm text-gray-600 mb-1">Email</label>
        <input
          data-testid="login-email"
          v-model="email"
          type="email"
          required
          class="w-full border border-gray-300 rounded px-3 py-2 text-sm"
        />
      </div>

      <div>
        <label class="block text-sm text-gray-600 mb-1">Contraseña</label>
        <input
          data-testid="login-password"
          v-model="password"
          type="password"
          required
          class="w-full border border-gray-300 rounded px-3 py-2 text-sm"
        />
      </div>

      <p v-if="error" data-testid="login-error" class="text-red-600 text-sm">
        {{ error }}
      </p>

      <button
        data-testid="login-submit"
        type="submit"
        :disabled="cargando"
        class="w-full bg-blue-600 text-white rounded py-2 text-sm font-medium disabled:opacity-50"
      >
        {{ cargando ? 'Ingresando...' : 'Ingresar' }}
      </button>
    </form>
  </div>
</template>