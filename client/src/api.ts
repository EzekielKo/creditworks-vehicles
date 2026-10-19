const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5221'

export type Category = {
  id: number
  name: string
  icon: string
  minWeight: number
  maxWeight: number | null
}
export type CategoryRequest = { name: string; icon: string; minWeight: number }
export type Manufacturer = { id: number; name: string }
export type VehicleRequest = {
  ownerName: string
  manufacturerId: number
  year: number
  weight: number
}
export type Vehicle = {
  id: number
  ownerName: string
  manufacturer: string
  year: number
  weight: number
  categoryName: string
  categoryIcon: string
}
export type SortKey =
  | 'owner'
  | 'manufacturer'
  | 'year'
  | 'weight'
  | 'category'

/** Thrown for any failed request; `errors` holds user-readable messages. */
export class ApiError extends Error {
  errors: string[]
  constructor(errors: string[]) {
    super(errors.join(' '))
    this.errors = errors
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${API_URL}${path}`, {
      ...init,
      headers: { 'Content-Type': 'application/json' },
    })
  } catch {
    throw new ApiError(['Could not reach the server. Is the API running?'])
  }

  if (!response.ok) {
    let errors = ['Something went wrong. Please try again.']
    try {
      const problem = await response.json()
      if (Array.isArray(problem.errors)) errors = problem.errors
      else if (problem.detail) errors = [problem.detail]
      else if (problem.title) errors = [problem.title]
    } catch {
      // body was not JSON; keep the generic message
    }
    throw new ApiError(errors)
  }

  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export const getVehicles = (sortBy?: SortKey, descending = false) =>
  request<Vehicle[]>(
    `/api/vehicles?sortBy=${sortBy ?? ''}&descending=${descending}`,
  )

export const createVehicle = (body: VehicleRequest) =>
  request<Vehicle>('/api/vehicles', { method: 'POST', body: JSON.stringify(body) })

export const getManufacturers = () => request<Manufacturer[]>('/api/manufacturers')

export const getCategories = () => request<Category[]>('/api/categories')

export const createCategory = (body: CategoryRequest) =>
  request<Category>('/api/categories', { method: 'POST', body: JSON.stringify(body) })

export const updateCategory = (id: number, body: CategoryRequest) =>
  request<Category>(`/api/categories/${id}`, { method: 'PUT', body: JSON.stringify(body) })

export const deleteCategory = (id: number) =>
  request<void>(`/api/categories/${id}`, { method: 'DELETE' })
