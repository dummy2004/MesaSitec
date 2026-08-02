import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import http from '@/api/http'
import type { Usuario, LoginResponse } from '@/types/dtos'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('mesasitec_token'))
  const usuario = ref<Usuario | null>(
    JSON.parse(localStorage.getItem('mesasitec_usuario') || 'null')
  )

  const estaAutenticado = computed(() => !!token.value)

  async function login(email: string, password: string) {
    const { data } = await http.post<LoginResponse>('/auth/login', { email, password })
    token.value = data.accessToken
    usuario.value = data.usuario
    localStorage.setItem('mesasitec_token', data.accessToken)
    localStorage.setItem('mesasitec_usuario', JSON.stringify(data.usuario))
  }

  function logout() {
    token.value = null
    usuario.value = null
    localStorage.removeItem('mesasitec_token')
    localStorage.removeItem('mesasitec_usuario')
  }

  return { token, usuario, estaAutenticado, login, logout }
})