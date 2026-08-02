<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useSolicitudesStore } from '@/stores/solicitudes'
import type { EstadoSolicitud, PrioridadSolicitud } from '@/types/dtos'

const store = useSolicitudesStore()
const router = useRouter()

const filtroEstado = ref<EstadoSolicitud | ''>('')
const filtroPrioridad = ref<PrioridadSolicitud | ''>('')
const filtroCategoria = ref('')
const filtroVencidas = ref(false)
const filtroBusqueda = ref('')
const page = ref(1)

async function cargar() {
  await store.listar({
    estado: filtroEstado.value || undefined,
    prioridad: filtroPrioridad.value || undefined,
    categoriaId: filtroCategoria.value || undefined,
    vencidas: filtroVencidas.value || undefined,
    q: filtroBusqueda.value || undefined,
    page: page.value,
    pageSize: 20,
  })
}

function limpiarFiltros() {
  filtroEstado.value = ''
  filtroPrioridad.value = ''
  filtroCategoria.value = ''
  filtroVencidas.value = false
  filtroBusqueda.value = ''
  page.value = 1
  cargar()
}

function irADetalle(id: string) {
  router.push(`/solicitudes/${id}`)
}

function paginaAnterior() {
  if (page.value > 1) {
    page.value -= 1
    cargar()
  }
}

function paginaSiguiente() {
  if (store.listado && page.value < store.listado.totalPaginas) {
    page.value += 1
    cargar()
  }
}

watch([filtroEstado, filtroPrioridad, filtroCategoria, filtroVencidas], () => {
  page.value = 1
  cargar()
})

onMounted(async () => {
  await store.cargarCategorias()
  await cargar()
})
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-lg font-semibold text-gray-800">Solicitudes</h1>
      <button
        data-testid="btn-nueva-solicitud"
        class="bg-blue-600 text-white text-sm px-4 py-2 rounded"
        @click="router.push('/solicitudes/nueva')"
      >
        Nueva solicitud
      </button>
    </div>

    <div class="flex flex-wrap gap-3 mb-4 bg-white p-4 rounded border border-gray-200">
      <select data-testid="filtro-estado" v-model="filtroEstado" class="border rounded px-2 py-1 text-sm">
        <option value="">Todos los estados</option>
        <option value="Nueva">Nueva</option>
        <option value="Asignada">Asignada</option>
        <option value="EnProceso">En Proceso</option>
        <option value="Resuelta">Resuelta</option>
        <option value="Cerrada">Cerrada</option>
        <option value="Cancelada">Cancelada</option>
      </select>

      <select data-testid="filtro-prioridad" v-model="filtroPrioridad" class="border rounded px-2 py-1 text-sm">
        <option value="">Toda prioridad</option>
        <option value="Baja">Baja</option>
        <option value="Media">Media</option>
        <option value="Alta">Alta</option>
        <option value="Critica">Crítica</option>
      </select>

      <select data-testid="filtro-categoria" v-model="filtroCategoria" class="border rounded px-2 py-1 text-sm">
        <option value="">Toda categoría</option>
        <option v-for="c in store.categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
      </select>

      <label class="flex items-center gap-1 text-sm text-gray-600">
        <input data-testid="filtro-vencidas" type="checkbox" v-model="filtroVencidas" />
        Solo vencidas
      </label>

      <input
        data-testid="filtro-busqueda"
        v-model="filtroBusqueda"
        @keyup.enter="page = 1; cargar()"
        placeholder="Buscar..."
        class="border rounded px-2 py-1 text-sm flex-1 min-w-[180px]"
      />

      <button
        data-testid="btn-limpiar-filtros"
        class="text-sm text-gray-600 border rounded px-3 py-1"
        @click="limpiarFiltros"
      >
        Limpiar filtros
      </button>
    </div>

    <div v-if="store.cargando" data-testid="listado-cargando" class="text-center py-10 text-gray-500">
      Cargando solicitudes...
    </div>

    <div v-else-if="store.error" class="text-center py-10 text-red-600">
      {{ store.error }}
    </div>

    <div
      v-else-if="!store.listado || store.listado.items.length === 0"
      data-testid="listado-vacio"
      class="text-center py-10 text-gray-500"
    >
      No hay solicitudes que coincidan con los filtros.
    </div>

    <table v-else data-testid="tabla-solicitudes" class="w-full bg-white border border-gray-200 rounded text-sm">
      <thead class="bg-gray-50 text-left text-gray-600">
        <tr>
          <th class="px-3 py-2">Código</th>
          <th class="px-3 py-2">Título</th>
          <th class="px-3 py-2">Estado</th>
          <th class="px-3 py-2">Prioridad</th>
          <th class="px-3 py-2">SLA</th>
          <th class="px-3 py-2"></th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="s in store.listado.items"
          :key="s.id"
          data-testid="fila-solicitud"
          :data-codigo="s.codigo"
          class="border-t border-gray-100 hover:bg-gray-50 cursor-pointer"
          @click="irADetalle(s.id)"
        >
          <td data-testid="celda-codigo" class="px-3 py-2">{{ s.codigo }}</td>
          <td class="px-3 py-2">{{ s.titulo }}</td>
          <td data-testid="celda-estado" class="px-3 py-2">{{ s.estado }}</td>
          <td data-testid="celda-prioridad" class="px-3 py-2">{{ s.prioridad }}</td>
          <td data-testid="celda-sla" class="px-3 py-2">
            {{ new Date(s.fechaLimiteSla).toLocaleString() }}
            <span
              v-if="s.vencida"
              data-testid="badge-vencida"
              class="ml-1 text-xs bg-red-100 text-red-700 px-1.5 py-0.5 rounded"
            >
              Vencida
            </span>
          </td>
          <td class="px-3 py-2 text-blue-600">Ver</td>
        </tr>
      </tbody>
    </table>

    <div v-if="store.listado" class="flex items-center justify-between mt-4 text-sm">
      <button
        data-testid="paginacion-anterior"
        :disabled="page <= 1"
        @click="paginaAnterior"
        class="border rounded px-3 py-1 disabled:opacity-40"
      >
        Anterior
      </button>
      <span data-testid="paginacion-info">
        Página {{ store.listado.page }} de {{ store.listado.totalPaginas }} — {{ store.listado.total }} resultados
      </span>
      <button
        data-testid="paginacion-siguiente"
        :disabled="page >= store.listado.totalPaginas"
        @click="paginaSiguiente"
        class="border rounded px-3 py-1 disabled:opacity-40"
      >
        Siguiente
      </button>
    </div>
  </div>
</template>