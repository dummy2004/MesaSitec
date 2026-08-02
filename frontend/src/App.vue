<script setup lang="ts">
import { RouterView } from 'vue-router'
import AppNav from '@/components/AppNav.vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <AppNav v-if="auth.estaAutenticado" />
    <main class="max-w-6xl mx-auto px-4 py-6">
      <RouterView />
    </main>
    <div
      v-if="false"
      data-testid="toast-mensaje"
    ></div>
  </div>
</template>