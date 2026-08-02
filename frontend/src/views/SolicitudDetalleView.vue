<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSolicitudesStore } from '@/stores/solicitudes'
import { useAuthStore } from '@/stores/auth'
import http from '@/api/http'
import type { AccionSolicitud } from '@/types/dtos'

const route = useRoute()
const router = useRouter()
const store = useSolicitudesStore()
const auth = useAuthStore()

const id = route.params.id as string

const modalAbierto = ref(false)
const accionActual = ref<AccionSolicitud | null>(null)
const agenteSeleccionado = ref('')
const motivo = ref('')
const modalError = ref('')
const agentesDisponibles = ref<{ id: string; nombre: string }[]>([])

async function cargar() {
  await store.obtenerDetalle(id)
}

const rol = computed(() => auth.usuario?.rol)
const usuarioId = computed(() => auth.usuario?.id)
const sol = computed(() => store.detalle)

const esDueno = computed(() => sol.value?.solicitante.id === usuarioId.value)

const puedeEditar = computed(() => {
  if (!sol.value) return false
  if (rol.value === 'Admin' || rol.value === 'Agente') return true
  return esDueno.value && sol.value.estado === 'Nueva'
})

function puedeAccion(accion: AccionSolicitud): boolean {
  if (!sol.value) return false
  const estado = sol.value.estado

  const transicionesPorEstado: Record<string, AccionSolicitud[]> = {
    Nueva: ['asignar', 'cancelar'],
    Asignada: ['iniciar', 'asignar', 'cancelar'],
    EnProceso: ['resolver', 'asignar', 'cancelar'],
    Resuelta: ['cerrar', 'reabrir'],
    Cerrada: [],
    Cancelada: [],
  }
  if (!transicionesPorEstado[estado]?.includes(accion)) return false

  if (accion === 'cancelar') return rol.value === 'Admin'
  if (accion === 'cerrar') {
    return rol.value === 'Admin' || rol.value === 'Agente' || (rol.value === 'Solicitante' && esDueno.value)
  }
  return rol.value === 'Admin' || rol.value === 'Agente'
}

async function abrirModal(accion: AccionSolicitud) {
  accionActual.value = accion
  agenteSeleccionado.value = ''
  motivo.value = ''
  modalError.value = ''
  modalAbierto.value = true

  if (accion === 'asignar') {
    const { data } = await http.get('/solicitudes')
    // Se listan usuarios agente/admin activos vía endpoint de categorias como referencia rápida
    // En este proyecto no existe endpoint de usuarios; se simplifica pidiendo el id manualmente si hiciera falta
    agentesDisponibles.value = []
  }
}

function cerrarModal() {
  modalAbierto.value = false
  accionActual.value = null
}

async function confirmarAccion() {
  if (!accionActual.value) return
  modalError.value = ''

  try {
    const extra: { agenteId?: string; motivo?: string } = {}
    if (accionActual.value === 'asignar') extra.agenteId = agenteSeleccionado.value
    if (accionActual.value === 'resolver' || accionActual.value === 'cancelar') extra.motivo = motivo.value

    await store.ejecutarAccion(id, accionActual.value, extra)
    modalAbierto.value = false
  } catch (e: any) {
    modalError.value = e.response?.data?.detail ?? 'No se pudo ejecutar la acción.'
  }
}

onMounted(cargar)
</script>

<template>
  <div v-if="store.cargando" data-testid="listado-cargando" class="text-gray-500">Cargando...</div>
  <div v-else-if="store.error" class="text-red-600">{{ store.error }}</div>

  <div v-else-if="sol" class="bg-white p-6 rounded border border-gray-200">
    <div class="flex items-center justify-between mb-4">
      <h1 data-testid="detalle-codigo" class="text-lg font-semibold">{{ sol.codigo }}</h1>
      <button class="text-sm text-gray-500" @click="router.push('/solicitudes')">Volver</button>
    </div>

    <h2 data-testid="detalle-titulo" class="text-base font-medium mb-2">{{ sol.titulo }}</h2>
    <p data-testid="detalle-descripcion" class="text-sm text-gray-600 mb-4">{{ sol.descripcion }}</p>

    <div class="grid grid-cols-2 gap-3 text-sm mb-4">
      <div>Estado: <span data-testid="detalle-estado" class="font-medium">{{ sol.estado }}</span></div>
      <div>Prioridad: <span data-testid="detalle-prioridad" class="font-medium">{{ sol.prioridad }}</span></div>
      <div>Categoría: <span data-testid="detalle-categoria" class="font-medium">{{ sol.categoria.nombre }}</span></div>
      <div>Agente: <span data-testid="detalle-agente" class="font-medium">{{ sol.agente?.nombre ?? 'Sin asignar' }}</span></div>
      <div>Creada: <span data-testid="detalle-fecha-creacion" class="font-medium">{{ new Date(sol.fechaCreacion).toLocaleString() }}</span></div>
      <div>
        Límite SLA:
        <span data-testid="detalle-fecha-limite" class="font-medium">{{ new Date(sol.fechaLimiteSla).toLocaleString() }}</span>
        <span v-if="sol.vencida" data-testid="detalle-vencida" class="ml-1 text-xs bg-red-100 text-red-700 px-1.5 py-0.5 rounded">Vencida</span>
      </div>
    </div>

    <p v-if="sol.motivoResolucion || sol.motivoCancelacion" data-testid="detalle-motivo" class="text-sm text-gray-600 mb-4">
      Motivo: {{ sol.motivoResolucion || sol.motivoCancelacion }}
    </p>

    <div class="flex flex-wrap gap-2">
      <button v-if="puedeEditar" data-testid="btn-editar" class="border rounded px-3 py-1.5 text-sm" @click="router.push(`/solicitudes/${id}/editar`)">Editar</button>
      <button v-if="puedeAccion('asignar')" data-testid="btn-accion-asignar" class="bg-blue-600 text-white rounded px-3 py-1.5 text-sm" @click="abrirModal('asignar')">Asignar</button>
      <button v-if="puedeAccion('iniciar')" data-testid="btn-accion-iniciar" class="bg-blue-600 text-white rounded px-3 py-1.5 text-sm" @click="abrirModal('iniciar')">Iniciar</button>
      <button v-if="puedeAccion('resolver')" data-testid="btn-accion-resolver" class="bg-green-600 text-white rounded px-3 py-1.5 text-sm" @click="abrirModal('resolver')">Resolver</button>
      <button v-if="puedeAccion('cerrar')" data-testid="btn-accion-cerrar" class="bg-gray-700 text-white rounded px-3 py-1.5 text-sm" @click="abrirModal('cerrar')">Cerrar</button>
      <button v-if="puedeAccion('reabrir')" data-testid="btn-accion-reabrir" class="border rounded px-3 py-1.5 text-sm" @click="abrirModal('reabrir')">Reabrir</button>
      <button v-if="puedeAccion('cancelar')" data-testid="btn-accion-cancelar" class="border border-red-300 text-red-600 rounded px-3 py-1.5 text-sm" @click="abrirModal('cancelar')">Cancelar</button>
    </div>

    <div v-if="modalAbierto" data-testid="modal-accion" class="fixed inset-0 bg-black/30 flex items-center justify-center">
      <div class="bg-white rounded p-6 w-full max-w-sm">
        <h3 class="font-medium mb-4">Confirmar: {{ accionActual }}</h3>

        <select v-if="accionActual === 'asignar'" data-testid="modal-select-agente" v-model="agenteSeleccionado" class="w-full border rounded px-2 py-1.5 text-sm mb-3">
          <option value="">Selecciona un agente</option>
          <option v-for="a in agentesDisponibles" :key="a.id" :value="a.id">{{ a.nombre }}</option>
        </select>

        <textarea
          v-if="accionActual === 'resolver' || accionActual === 'cancelar'"
          data-testid="modal-motivo"
          v-model="motivo"
          placeholder="Motivo"
          class="w-full border rounded px-2 py-1.5 text-sm mb-3"
          rows="3"
        ></textarea>

        <p v-if="modalError" data-testid="modal-error" class="text-red-600 text-sm mb-3">{{ modalError }}</p>

        <div class="flex justify-end gap-2">
          <button data-testid="modal-cancelar" class="text-sm text-gray-600 px-3 py-1.5" @click="cerrarModal">Cancelar</button>
          <button data-testid="modal-confirmar" class="bg-blue-600 text-white text-sm px-3 py-1.5 rounded" @click="confirmarAccion">Confirmar</button>
        </div>
      </div>
    </div>
  </div>
</template>