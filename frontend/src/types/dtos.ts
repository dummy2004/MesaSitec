export type RolUsuario = 'Admin' | 'Agente' | 'Solicitante'
export type PrioridadSolicitud = 'Baja' | 'Media' | 'Alta' | 'Critica'
export type EstadoSolicitud = 'Nueva' | 'Asignada' | 'EnProceso' | 'Resuelta' | 'Cerrada' | 'Cancelada'
export type AccionSolicitud = 'asignar' | 'iniciar' | 'resolver' | 'cerrar' | 'reabrir' | 'cancelar'

export interface Usuario {
  id: string
  nombre: string
  email: string
  rol: RolUsuario
  tenantId: string
  tenantNombre: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: Usuario
}

export interface Categoria {
  id: string
  nombre: string
  slaHoras: number
}

export interface SolicitudResumen {
  id: string
  codigo: string
  titulo: string
  estado: EstadoSolicitud
  prioridad: PrioridadSolicitud
  categoria: { id: string; nombre: string }
  agente: { id: string; nombre: string } | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudDetalle extends SolicitudResumen {
  descripcion: string
  solicitante: { id: string; nombre: string }
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
}

export interface ListadoSolicitudes {
  items: SolicitudResumen[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface FiltrosSolicitudes {
  estado?: EstadoSolicitud
  prioridad?: PrioridadSolicitud
  categoriaId?: string
  agenteId?: string
  q?: string
  vencidas?: boolean
  page?: number
  pageSize?: number
  sort?: string
}

export interface ApiError {
  type: string
  title: string
  status: number
  detail: string
  codigo: string
  errores?: Record<string, string[]>
}