import axios from 'axios'
import router from '@/router'

const http = axios.create({
  baseURL: 'http://localhost:5080/api/v1',
})

http.interceptors.request.use((config) => {
  const token = localStorage.getItem('mesasitec_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

http.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('mesasitec_token')
      localStorage.removeItem('mesasitec_usuario')
      router.push('/login')
    }
    return Promise.reject(error)
  }
)

export default http