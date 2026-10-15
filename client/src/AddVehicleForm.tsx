import { useEffect, useState, type FormEvent } from 'react'
import { ApiError, createVehicle, getManufacturers, type Manufacturer } from './api'

export function AddVehicleForm({ onAdded }: { onAdded: () => void }) {
  const [manufacturers, setManufacturers] = useState<Manufacturer[]>([])
  const [ownerName, setOwnerName] = useState('')
  const [manufacturerId, setManufacturerId] = useState('')
  const [year, setYear] = useState('')
  const [weight, setWeight] = useState('')
  const [errors, setErrors] = useState<string[]>([])
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    getManufacturers()
      .then(setManufacturers)
      .catch((e) => setErrors(e instanceof ApiError ? e.errors : ['Failed to load manufacturers.']))
  }, [])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setErrors([])
    try {
      await createVehicle({
        ownerName,
        manufacturerId: Number(manufacturerId) || 0,
        year: Number(year) || 0,
        weight: Number(weight) || 0,
      })
      onAdded()
    } catch (err) {
      setErrors(err instanceof ApiError ? err.errors : ['Failed to save the vehicle.'])
    } finally {
      setSubmitting(false)
    }
  }

  const inputClass = 'w-full rounded border border-gray-300 p-2'

  return (
    <section>
      <h2 className="mb-3 text-xl font-semibold">Add vehicle</h2>
      {errors.length > 0 && (
        <ul className="mb-3 list-disc rounded bg-red-50 p-3 pl-8 text-red-700" role="alert">
          {errors.map((m) => (
            <li key={m}>{m}</li>
          ))}
        </ul>
      )}
      <form onSubmit={handleSubmit} className="max-w-md space-y-4" noValidate>
        <label className="block">
          <span className="mb-1 block font-medium">Owner's name</span>
          <input className={inputClass} value={ownerName} onChange={(e) => setOwnerName(e.target.value)} />
        </label>
        <label className="block">
          <span className="mb-1 block font-medium">Manufacturer</span>
          <select className={inputClass} value={manufacturerId} onChange={(e) => setManufacturerId(e.target.value)}>
            <option value="">Select a manufacturer…</option>
            {manufacturers.map((m) => (
              <option key={m.id} value={m.id}>
                {m.name}
              </option>
            ))}
          </select>
        </label>
        <label className="block">
          <span className="mb-1 block font-medium">Year of manufacture</span>
          <input
            className={inputClass}
            type="number"
            value={year}
            onChange={(e) => setYear(e.target.value)}
          />
        </label>
        <label className="block">
          <span className="mb-1 block font-medium">Weight (kg)</span>
          <input
            className={inputClass}
            type="number"
            step="0.01"
            value={weight}
            onChange={(e) => setWeight(e.target.value)}
          />
        </label>
        <button
          type="submit"
          disabled={submitting}
          className="cursor-pointer rounded bg-blue-700 px-4 py-2 font-semibold text-white disabled:opacity-50"
        >
          {submitting ? 'Saving…' : 'Add vehicle'}
        </button>
      </form>
    </section>
  )
}
