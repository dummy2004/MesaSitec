<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSolicitudesStore } from '@/stores/solicitudes'

const route = useRoute()
const router = useRouter()
const store = useSolicitudesStore()

const esEdicion = computed(() => !!route.params.id)
const id = route.params.id as string | undefined

const titulo = ref('')
const descripcion = ref('')
const categoriaId = ref('')
const prioridad = ref('')

const errorTitulo = ref('')
const errorDescripcion = ref('')
const errorCategoria = ref('')
const errorGeneral = ref('')
const enviando = ref(false)

function validar(): boolean {
  errorTitulo.value = ''
  errorDescripcion.value = ''
  errorCategoria.value = ''
  let ok = true

  if (titulo.value.length < 5 || titulo.value.length > 120) {
    errorTitulo.value = 'El título debe tener entre 5 y 120 caracteres.'
    ok = false
  }
  if (descripcion.value.length < 10 || descripcion.value.length > 4000) {
    errorDescripcion.value = 'La descripción debe tener entre 10 y 4000 caracteres.'
    ok = false
  }
  if (!categoriaId.value) {
    errorCategoria.value = 'Selecciona una categoría.'
    ok = false
  }
  return ok
}

async function enviar() {
  errorGeneral.value = ''
  if (!validar()) return

  enviando.value = true
  try {
    const payload = {
      titulo: titulo.value,
      descripcion: descripcion.value,
      categoriaId: categoriaId.value,
      prioridad: prioridad.value,
    }

    if (esEdicion.value && id) {
      await store.editar(id, payload)
      router.push(`/solicitudes/${id}`)
    } else {
      const creada = await store.crear(payload)
      router.push(`/solicitudes/${creada.id}`)
    }
  } catch (e: any) {
    errorGeneral.value = e.response?.data?.detail ?? 'No se pudo guardar la solicitud.'
  } finally {
    enviando.value = false
  }
}

function cancelar() {
  router.back()
}

onMounted(async () => {
  await store.cargarCategorias()
  if (esEdicion.value && id) {
    await store.obtenerDetalle(id)
    if (store.detalle) {
      titulo.value = store.detalle.titulo
      descripcion.value = store.detalle.descripcion
      categoriaId.value = store.detalle.categoria.id
      prioridad.value = store.detalle.prioridad
    }
  } else {
    prioridad.value = 'Media'
  }
})
</script>

<template>
  <div class="bg-white p-6 rounded border border-gray-200 max-w-xl">
    <h1 class="text-lg font-semibold mb-4">
      {{ esEdicion ? 'Editar solicitud' : 'Nueva solicitud' }}
    </h1>

    <form @submit.prevent="enviar" class="space-y-4">
      <div>
        <label class="block text-sm text-gray-600 mb-1">Título</label>
        <input data-testid="form-titulo" v-model="titulo" class="w-full border rounded px-3 py-2 text-sm" />
        <p v-if="errorTitulo" data-testid="error-titulo" class="text-red-600 text-xs mt-1">{{ errorTitulo }}</p>
      </div>

      <div>
        <label class="block text-sm text-gray-600 mb-1">Descripción</label>
        <textarea data-testid="form-descripcion" v-model="descripcion" rows="4" class="w-full border rounded px-3 py-2 text-sm"></textarea>
        <p v-if="errorDescripcion" data-testid="error-descripcion" class="text-red-600 text-xs mt-1">{{ errorDescripcion }}</p>
      </div>

      <div>
        <label class="block text-sm text-gray-600 mb-1">Categoría</label>
        <select data-testid="form-categoria" v-model="categoriaId" class="w-full border rounded px-3 py-2 text-sm">
          <option value="">Selecciona una categoría</option>
          <option v-for="c in store.categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
        </select>
        <p v-if="errorCategoria" data-testid="error-categoria" class="text-red-600 text-xs mt-1">{{ errorCategoria }}</p>
      </div>

      <div>
        <label class="block text-sm text-gray-600 mb-1">Prioridad</label>
        <select data-testid="form-prioridad" v-model="prioridad" class="w-full border rounded px-3 py-2 text-sm">
          <option value="Baja">Baja</option>
          <option value="Media">Media</option>
          <option value="Alta">Alta</option>
          <option value="Critica">Crítica</option>
        </select>
      </div>

      <p v-if="errorGeneral" class="text-red-600 text-sm">{{ errorGeneral }}</p>

      <div class="flex gap-2">
        <button data-testid="form-submit" type="submit" :disabled="enviando" class="bg-blue-600 text-white rounded px-4 py-2 text-sm disabled:opacity-50">
          {{ enviando ? 'Guardando...' : 'Guardar' }}
        </button>
        <button data-testid="form-cancelar" type="button" class="border rounded px-4 py-2 text-sm" @click="cancelar">
          Cancelar
        </button>
      </div>
    </form>
  </div>
</template>