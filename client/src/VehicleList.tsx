import { useEffect, useState } from 'react'
import { ApiError, getVehicles, type SortKey, type Vehicle } from './api'
import { CategoryIcon } from './CategoryIcon'

const columns: { key: SortKey; label: string }[] = [
  { key: 'owner', label: "Owner's name" },
  { key: 'manufacturer', label: 'Manufacturer' },
  { key: 'year', label: 'Year' },
  { key: 'weight', label: 'Weight (kg)' },
  { key: 'category', label: 'Category' },
]

export function VehicleList() {
  const [sortBy, setSortBy] = useState<SortKey>('owner')
  const [descending, setDescending] = useState(false)
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    async function load() {
      setLoading(true)

      try {
        const data = await getVehicles(sortBy, descending)

        if (!cancelled) {
          setVehicles(data)
          setError(null)
        }
      } catch (e) {
        if (!cancelled) {
          setError(
            e instanceof ApiError
              ? e.message
              : 'Failed to load vehicles.',
          )
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    load()

    return () => {
      cancelled = true
    }
  }, [sortBy, descending])

  function handleSort(key: SortKey) {
    if (key === sortBy) {
      setDescending((current) => !current)
    } else {
      setSortBy(key)
      setDescending(false)
    }
  }

  return (
    <section>
      <h2 className="mb-3 text-xl font-semibold">Vehicles</h2>

      {error && (
        <p className="mb-3 rounded bg-red-50 p-3 text-red-700">
          {error}
        </p>
      )}

      <table className="w-full border-collapse text-left">
        <thead>
          <tr className="border-b border-gray-300">
            {columns.map((column) => (
              <th
                key={column.key}
                aria-sort={
                  column.key === sortBy
                    ? descending
                      ? 'descending'
                      : 'ascending'
                    : 'none'
                }
                className="p-2"
                scope="col"
              >
                <button
                  type="button"
                  onClick={() => handleSort(column.key)}
                  className={`cursor-pointer font-semibold hover:underline ${
                    column.key === sortBy ? 'text-blue-700' : ''
                  }`}
                >
                  {column.label}
                  <span className="ml-2" aria-hidden="true">
                    {column.key === sortBy
                      ? descending
                        ? '▼'
                        : '▲'
                      : '↕'}
                  </span>
                </button>
              </th>
            ))}
          </tr>
        </thead>

        <tbody>
          {vehicles.map((vehicle) => (
            <tr key={vehicle.id} className="border-b border-gray-200">
              <td className="p-2">{vehicle.ownerName}</td>
              <td className="p-2">{vehicle.manufacturer}</td>
              <td className="p-2">{vehicle.year}</td>
              <td className="p-2">
                {vehicle.weight.toLocaleString()}
              </td>
              <td className="p-2">
                <CategoryIcon
                  icon={vehicle.categoryIcon}
                  label={vehicle.categoryName}
                />
              </td>
            </tr>
          ))}

          {!loading && vehicles.length === 0 && !error && (
            <tr>
              <td colSpan={columns.length} className="p-4 text-gray-500">
                No vehicles yet.
              </td>
            </tr>
          )}
        </tbody>
      </table>

      {loading && (
        <p className="mt-3 text-gray-500" role="status">
          Loading…
        </p>
      )}
    </section>
  )
}