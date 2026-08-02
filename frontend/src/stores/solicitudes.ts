import { defineStore } from 'pinia'
import { ref } from 'vue'
import http from '@/api/http'
import type {
  SolicitudDetalle,
  ListadoSolicitudes,
  FiltrosSolicitudes,
  Categoria,
  AccionSolicitud,
} from '@/types/dtos'

export const useSolicitudesStore = defineStore('solicitudes', () => {
  const listado = ref<ListadoSolicitudes | null>(null)
  const detalle = ref<SolicitudDetalle | null>(null)
  const categorias = ref<Categoria[]>([])
  const cargando = ref(false)
  const error = ref<string | null>(null)

  async function cargarCategorias() {
    const { data } = await http.get<Categoria[]>('/categorias')
    categorias.value = data
  }

  async function listar(filtros: FiltrosSolicitudes = {}) {
    cargando.value = true
    error.value = null
    try {
      const { data } = await http.get<ListadoSolicitudes>('/solicitudes', { params: filtros })
      listado.value = data
    } catch (e: any) {
      error.value = e.response?.data?.detail ?? 'Ocurrió un error al cargar las solicitudes.'
    } finally {
      cargando.value = false
    }
  }

  async function obtenerDetalle(id: string) {
    cargando.value = true
    error.value = null
    detalle.value = null
    try {
      const { data } = await http.get<SolicitudDetalle>(`/solicitudes/${id}`)
      detalle.value = data
    } catch (e: any) {
      error.value = e.response?.data?.detail ?? 'No se pudo cargar la solicitud.'
    } finally {
      cargando.value = false
    }
  }

  async function crear(payload: {
    titulo: string
    descripcion: string
    categoriaId: string
    prioridad: string
  }) {
    const { data } = await http.post<SolicitudDetalle>('/solicitudes', payload)
    return data
  }

  async function editar(
    id: string,
    payload: { titulo: string; descripcion: string; categoriaId: string; prioridad: string }
  ) {
    const { data } = await http.put<SolicitudDetalle>(`/solicitudes/${id}`, payload)
    detalle.value = data
    return data
  }

  async function ejecutarAccion(
    id: string,
    accion: AccionSolicitud,
    extra?: { agenteId?: string; motivo?: string }
  ) {
    const { data } = await http.post<SolicitudDetalle>(`/solicitudes/${id}/transiciones`, {
      accion,
      ...extra,
    })
    detalle.value = data
    return data
  }

  return {
    listado,
    detalle,
    categorias,
    cargando,
    error,
    cargarCategorias,
    listar,
    obtenerDetalle,
    crear,
    editar,
    ejecutarAccion,
  }
})